using System;
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Text;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Animations;
using Object = UnityEngine.Object;
#if UNITY_2018_3_OR_NEWER
using UnityEngine.SceneManagement;
#endif

namespace sr
{
  /**
   * When a user executes a search, a SearchJob is generated. Execute() is called
   * and SearchSubJobs are created based on the type of data being searched.
   * SearchJob keeps track of searched assets to stop searches from accidentally
   * being executed multiple times (possibly due to Dependencies being checked).
   * This also contains a number of utility functions for subjobs and search items
   * to call during execution. Things such as recursively searching objects or
   * searching more complicated objects like AnimatorControllers.
   */
  public class SearchJob
  {
    //What we are searching for.
    public SearchItemSet search;
    // How we want to search for it. This is always a copy.
    public SearchOptions options;
    // The results of the search.
    public SearchResultSet resultSet;
    // Where we should search.
    public SearchScopeData scope;
    
    // The data model for our currently searched asset.
    // Searching in a prefab vs. a prefab stage, vs a selection all have different parameters. This abstracts a lot of
    // silliness of where an asset came from and how it is saved into something that can be consistently worked with.
    public SearchAssetData assetData;

    public Dictionary<string, SearchAssetData> searchAssetsData = new Dictionary<string, SearchAssetData>();

    public List<SearchAssetData> searchAssets = new List<SearchAssetData>();
    public List<SearchAssetData> dependencyAssets = new List<SearchAssetData>();

    // If an asset is going to search a scene other than the current, we must 
    // open that scene, causing us to prompt the user.
    public bool searchIncludesScenes = false;

    public bool searchIncludesScripts = false;

    //Progress watching
    int currentItem;
    int maxItemsAllowed = 10000;
    bool shownMaxItemsWarning = false;

    StringBuilder logBuilder = new StringBuilder();

    List<SearchJob> subJobs = new List<SearchJob>();

    private HashSet<Type> blacklistedTypes = new HashSet<Type>();

    private List<string> blacklistedList = new List<string>()
    {
      "UnityEngine.Polybrush.PolybrushMesh, Unity.Polybrush",
    };


    private bool cancelled = false;

    public HashSet<string> supportedFileTypes = new HashSet<string>()
    {
      ".unity",
      ".prefab",
      ".mat",
      ".asset",
      ".anim",
      ".controller",
      ".wav",
      ".mp3",
      ".png",
      ".psd",
      ".tiff",
      ".tif",
      ".tga",
      ".gif",
      ".bmp",
      ".jpg",
      ".jpeg"
    };

    public SearchJob(SearchItemSet s, SearchOptions o, SearchScopeData sc)
    {
      
      search = s;
      options = o;
      scope = sc;
      foreach (string blacklistedType in blacklistedList)
      {
        Type t = Type.GetType(blacklistedType);
        if (t != null)
        {
          blacklistedTypes.Add(t);
        }
      }
      // this.searchAssets = searchAssets;
      // foreach(SearchAssetData asset in searchAssets)
      // {
      //   searchAssetsData[asset.assetPath] = asset;
      // }
    }

    public void Log(string message)
    {
      logBuilder.Append(message);
      logBuilder.Append("\n");
    }

    void addIgnorableResources()
    {
      addIgnorableResource("Library/unity default resources");
      addIgnorableResource("Resources/unity_builtin_extra");
    }

    void addIgnorableResource(string path)
    {
      SearchAssetData data = new IgnoredSearchAssetData(path);
      data.hasBeenSearched = true;
      searchAssetsData[path] = data;
    }

    public void Cancel()
    {
      foreach(SearchJob subJob in subJobs)
      {
        Cancel();
      }
      cancelled = true;
    }

    public SearchStatus SearchStatus
    {
      get{
        if(resultSet == null)
        {
          return SearchStatus.None;
        }else{
          return resultSet.status;
        }
      }
    }

    public void Execute()
    {
      initializeSearch();
      foreach(SearchAssetData searchAssetData in searchAssets)
      {
        searchAsset(searchAssetData);
        searchAssetData.Unload();
      }
    }

    void initializeSearch()
    {
      addIgnorableResources();
      AnimationMode.StopAnimationMode(); // If recording is on, it will hose the animation.
      search.OnSearchBegin();
      resultSet = new SearchResultSet(search);
      resultSet.status = SearchStatus.InProgress;

      currentItem = 1;


    }

    public IEnumerator ExecuteAsync()
    {
      initializeSearch();
      if(searchIncludesScenes)
      {
        bool shouldContinue = SceneUtil.SaveDirtyScenes();
        if(!shouldContinue)
        {
          userAbortedSearch();
          yield break;
        }
      }
    
    
      foreach(SearchAssetData searchAssetData in searchAssets)
      {
        if(cancelled)
        {
          break;
        }
        searchAsset(searchAssetData);
        yield return searchDependencies(searchAssetData.Roots);
        searchAssetData.Unload();

        if(resultSet.resultsCount > maxItemsAllowed && !shownMaxItemsWarning)
        {
          shownMaxItemsWarning = true;
          bool userContinues = EditorUtility.DisplayDialog("Too Many Results", "The search and replace plugin has found "+ resultSet.resultsCount+" results so far. Do you want to continue searching?", "Continue", "Cancel");
          if(!userContinues)
          {
            cancelled = true;
          }
        }
        currentItem++;
        yield return null;
      }

      EditorUtility.ClearProgressBar();
      search.OnSearchEnd(this);

      if(cancelled)
      {
        userAbortedSearch();
      }

      calculateSearchedItems();
      
      string log = logBuilder.ToString();
      if(log.Length > 0) 
      {
        Debug.Log("[Search & Replace] Log:\n"+log);
      }
      SRWindow.Instance.Repaint();
    }

    void calculateSearchedItems()
    {
      
      //calculate searchedItems
      int searchedItems = 0;
      foreach(SearchAssetData searchAsset in searchAssets)
      {
        if(searchAsset.searchExecuted)
        {
          searchedItems++;
        }
      }
      foreach(SearchAssetData searchAsset in dependencyAssets)
      {
        if(searchAsset.searchExecuted)
        {
          searchedItems++;
        }
      }
      if(resultSet.status == SearchStatus.InProgress)
      {
        //standard termination.
        resultSet.searchedItems = searchedItems;
        resultSet.status = SearchStatus.Complete;
      }
      if(options.searchType == SearchType.SearchAndReplace && resultSet.resultsCount > 0)
      {
        AssetDatabase.SaveAssets();
      }
      resultSet.OnSearchComplete();

    }

    void userAbortedSearch()
    {
      resultSet.status = SearchStatus.UserAborted;
      resultSet.statusMsg = "User cancelled search.";
    }

    //Callback while searching.
    public void MatchFound(SearchResult r, SearchItem s)
    {
      if (r == null)
      {
        return;
      }
      if(r.actionTaken != SearchAction.Ignored)
      {
        resultSet.Add(r, s);
      }
    }

    public void AddJob(SearchJob subJob)
    {
      subJobs.Add(subJob);
    }

    public void AddResultsFromSubsearch(SearchItem parentItem, SearchResultGroup resultsGroup)
    {
      foreach(SearchResult result in resultsGroup.results)
      {
        MatchFound(result, parentItem);
      }
    }
    public void DrawResult()
    {
      resultSet.Draw();
    }

    public void Draw()
    {
      GUILayout.BeginVertical();

      if(resultSet.status == SearchStatus.InProgress)
      {
        var rect = EditorGUILayout.GetControlRect(false, EditorGUIUtility.singleLineHeight);
        float cancelButtonWidth = 25;
        Rect progressBarRect = rect;
        progressBarRect.width -= cancelButtonWidth;
        Rect cancelButtonRect = rect;
        cancelButtonRect.width = cancelButtonWidth;
        cancelButtonRect.x = progressBarRect.width;
        
        EditorGUI.ProgressBar(progressBarRect, (float)currentItem / (float)searchAssets.Count, "Searching "+assetData.assetName+ " " + currentItem+"/"+searchAssets.Count ); 
        Color currentColor = GUI.backgroundColor;
        GUI.backgroundColor = new Color(0.7f, 0.3f, 0.3f, 1.0f);
        if(GUI.Button(cancelButtonRect, "X"))
        {
          Cancel();
        }
        GUI.backgroundColor = currentColor;
      }
      resultSet.Draw();

      GUILayout.EndVertical();
      if(resultSet.status == SearchStatus.Complete)
      {
        GUILayout.BeginHorizontal();
        if(GUILayout.Button("Copy To Clipboard"))
        {
          resultSet.CopyToClipboard();
        }
        if(GUILayout.Button("Select Objects"))
        {
          resultSet.SelectAll();
        }

        GUILayout.EndHorizontal();
      }else{
        GUILayout.FlexibleSpace();
        
        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
 
        GUILayout.EndHorizontal();

      }
  }

    // SUBJOB HELPER FUNCTIONS.

    // Called when a search asset begins its search so it can do any per-asset checks.
    public void OnAssetSearchBegin()
    {
      search.OnAssetSearchBegin(this);
    }

    // Called when a search asset ends its search so it can do any per-asset checks.
    public void OnAssetSearchEnd()
    {
      search.OnAssetSearchEnd(this);
    }

    public void searchObject(UnityEngine.Object obj)
    {
      if(obj == null)
      {
        // Debug.Log("[SearchJob] obj is null::"+assetData.assetPath);
        return;
      }else{
        // Debug.Log("[SearchJob] searching:"+PathInfo.GetPathInfo(obj).FullPath() , obj);
      }
      SerializedObject so = new SerializedObject(obj);
      SerializedProperty iterator = so.GetIterator();
      search.SearchProperty(this, iterator);
      search.SearchObject(this, obj);
    }

    public void searchGameObject(GameObject go, HashSet<Object> excludedObjects=null)
    {
      if (excludedObjects != null && excludedObjects.Contains(go))
      {
        return;
      }
      // search the game object itself.
      searchObject(go);
      if(go == null)
      {
        return;
      }
      Component[] components = go.GetComponents<Component>();
      search.SearchGameObject(this, go);
      foreach(Component c in components)
      {
        if(c != null)
        {
          if (isComponentSearchable(c))
          {
            SerializedObject obj = new SerializedObject(c);
            SerializedProperty iterator = obj.GetIterator();
            search.SearchProperty(this, iterator);
          }
          else
          {
            Debug.Log(c.name + " is not searchable.");          
          }
        }
      }
      if(go != null)
      {
        IEnumerator children = go.transform.GetEnumerator();
        while(children.MoveNext())
        {
          Transform child =(Transform)children.Current;
          if(child != null)
          {
            searchGameObject(child.gameObject, excludedObjects);
          }else{
            break;
          }
        }
      }
    }

    List<string> internalPath = new List<string>();

    public void searchAnimatorController(AnimatorController ac)
    {
      SerializedObject so = new SerializedObject(ac);
      SerializedProperty layers = so.FindProperty("m_AnimatorLayers");

      if(layers != null)
      {
        layers.Next(true);
        layers.Next(true);
        int count = layers.intValue;
        for(int i=0;i < count; i++)
        {
          layers.Next(false);
          // Debug.Log("[SearchJob] found:"+layers.propertyPath);

          search.SearchProperty(this, layers.Copy());

          //grab the state machine from this layer.
          SerializedProperty stateMachine = layers.FindPropertyRelative("m_StateMachine");
          UnityEngine.Object stateMachineObj = stateMachine.objectReferenceValue;
          // Debug.Log("[SearchJob] stateMachineObj:"+stateMachineObj);
          pushPath(stateMachineObj);
          SerializedObject stateMachineSO = new SerializedObject(stateMachineObj);
          searchObject(stateMachineObj);
          searchArray(stateMachineSO.FindProperty("m_AnyStateTransitions"));
          List<SerializedObject> states = searchArrayRelative(stateMachineSO.FindProperty("m_ChildStates"), "m_State");
          foreach(SerializedObject stateSO in states)
          {
            pushPath(stateSO.targetObject);
            searchArray(stateSO.FindProperty("m_Transitions"));
            popPath();
          }
          popPath();
        }
      }
    }

    List<SerializedObject> searchArrayRelative(SerializedProperty arrayProp, string relativeName)
    {
      List<SerializedObject> retVal = new List<SerializedObject>();
      arrayProp.Next(true);
      arrayProp.Next(true);
      int count = arrayProp.intValue;
      for(int i=0; i < count; i++)
      {
        arrayProp.Next(false);
        SerializedProperty item = arrayProp.FindPropertyRelative(relativeName);
        pushPath(item.objectReferenceValue);
        searchObject(item.objectReferenceValue);
        retVal.Add(new SerializedObject(item.objectReferenceValue));
        popPath();

      }
      return retVal;
    }

    void searchArray(SerializedProperty arrayProp)
    {
      arrayProp.Next(true);
      arrayProp.Next(true);
      int count = arrayProp.intValue;
      for(int i=0; i < count; i++)
      {
        arrayProp.Next(false);
        pushPath(arrayProp.objectReferenceValue);
        searchObject(arrayProp.objectReferenceValue);
        popPath();
      }
    }

    public void pushPath(UnityEngine.Object obj)
    {
      if(obj != null)
      {
        internalPath.Add(obj.name);
        assetData.SetInternalAssetPath(internalPath);
      }
    }

    public void popPath()
    {
      internalPath.RemoveAt(internalPath.Count - 1);
    }

    //Instead of maintaining a separate hashset, we just put the internal asset
    //into the dictionary.
    public void addInternalAsset(string path, int instanceID)
    {
      SearchAssetData internalSearchObject = new ScriptableObjectSearchAssetData(path);
      searchAssetsData[path + instanceID] = internalSearchObject;
    }

    // DEPENDENCY HELPER FUNCTIONS

    /**
     * Called to handle whether we should search the given asset path.
     */
    public bool isSearchable(string assetPath)
    {
      return supportedFileTypes.Contains(Path.GetExtension(assetPath));
    }

    public bool isComponentSearchable(Component c)
    {
      return !blacklistedTypes.Contains(c.GetType());
    }

    /**
     * Whether or not the currently searched item is of a type that a SearchItem
     * wants to process.
     */
    public bool searchItemCaresAboutAsset()
    {
      // this has been temporarily removed until after the search asset flow refactor.
      return search.searchItemCaresAboutAsset(this);
    }
    private IEnumerator searchDependencies(UnityEngine.Object[] rootObjects)
    {
      if(rootObjects == null || rootObjects.Length == 0)
      {
        yield break;
      }

      if(!scope.searchDependencies)
      {
        yield break;
      }
      if(scope.assetScope == SearchScope.allAssets && scope.projectScope == ProjectScope.EntireProject)
      {
        // We are searching literally everything we can, so no need to load
        // things more.
        yield break;
      }
      //search dependencies
      UnityEngine.Object[] dependencies = EditorUtility.CollectDependencies(rootObjects);

      foreach(UnityEngine.Object dependency in dependencies)
      {
        string path = AssetDatabase.GetAssetPath(dependency);
        
        if(!searchAssetsData.ContainsKey(path))
        {
          // empty strings mean it is local object.
          if(path != string.Empty)
          {
            // do we support this file extension?
            if(isSearchable(path))
            {
              SearchAssetData dependencyAsset = SearchAssetFactory.AssetForPath(path);
              if (dependencyAsset != null)
              {
                searchAssetsData[path] = dependencyAsset;
                dependencyAssets.Add(dependencyAsset);

                searchAsset(dependencyAsset);
                dependencyAsset.Unload();
                yield return null;
              }
            }
          }
        }
      }
    }

    private void searchAsset(SearchAssetData searchAsset)
    {
      if (searchAsset == null)
      {
        return;
      }
      try{
        //todo: optimize and re-write caresAboutAsset().
        if (!searchAsset.hasBeenSearched)
        {
          assetData = searchAsset;
          searchAsset.ProcessAsset(this);
          searchAsset.hasBeenSearched = true;
        }
      }catch(System.Exception ex){
        Debug.LogException(ex);
        resultSet.status = SearchStatus.InProgress;
        resultSet.statusMsg = "An exception occurred: "+ex.ToString();
      }
    }

    public void AddAsset(SearchAssetData assetData)
    {
      searchAssetsData.Add(assetData.assetPath, assetData);
      if(assetData is SceneSearchAssetData)
      {
        searchIncludesScenes = true;
      }
      if(assetData.containsScripts)
      {
        searchIncludesScripts = true;
      }
      searchAssets.Add(assetData);
    }
    public void AddAssets(List<SearchAssetData> assetDatas)
    {
      foreach(SearchAssetData assetData in assetDatas)
      {
        AddAsset(assetData);
      }
    }

  }
}