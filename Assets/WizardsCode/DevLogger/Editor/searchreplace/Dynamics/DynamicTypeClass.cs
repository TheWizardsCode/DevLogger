using UnityEngine;
using UnityEditor;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace sr
{
  /**
   * Provides a search field for searching and replacing monoscript classes.
   */

  [System.Serializable]
  public class DynamicTypeClass : DynamicTypeData<ObjectID>
  {

    public static List<Type> replaceOptionTypes = new List<Type>()
    {
      typeof(ReplaceItemClass),  
      typeof(ReplaceItemRemoveComponent)
    };

    // The type of the current search.
    [System.NonSerialized]
    public Type type;

    // Do we want to match this class specifically or also classes that inherit from it?
    bool matchChildClasses;

    // Should we keep track of whether the given objects are found during the search
    // and instead show all items that *weren't* found instead?
    bool showUnusedScripts;

    // Should we only return results if the given script was not found.
    bool matchAssetMissingScript;

    [System.NonSerialized]
    static string[] suffixes = new string[]{".cs", ".js"};

    // A hash of classes that can be used during the search when looking for unused
    // scripts.
    [System.NonSerialized]
    public Dictionary<Type, ClassSearchInfo> typeHash;

    // Data object for script results.
    public class ClassSearchInfo
    {
      public Type type;
      public MonoScript script;
      public int numFound = 0;
      public ClassSearchInfo(MonoScript ms)
      {
        script = ms;
        type = ms.GetClass();
      }

      public void Reset()
      {
        numFound = 0;
      }
    }

    public override void Draw(SearchOptions options)
    {
      float lw = EditorGUIUtility.labelWidth;
      EditorGUIUtility.labelWidth = SRWindow.compactLabelWidth;

      GUILayout.BeginHorizontal();
      UnityEngine.Object newValue = EditorGUILayout.ObjectField("Value:", searchValue.obj, typeof(UnityEngine.Object), true);
      EditorGUIUtility.labelWidth = lw; // i love stateful gui! :(

      if(newValue != searchValue.obj)
      {
        UnityEngine.Object scriptObject = ObjectUtil.getScriptObjectFromObject(newValue);
        if(scriptObject == null && newValue != null)
        {
          //looks like we couldn't get an object! Is it a directory?
          if(ObjectUtil.IsDirectory(newValue))
          {
            setFolder(newValue);
          }else{
            Debug.Log("[Project Search & Replace] "+newValue+" isn't a Folder, Component, MonoBehaviour, ScriptableObject or MonoScript.");
          }
        }else{
          SetObject(scriptObject);
        }
        SRWindow.Instance.PersistCurrentSearch();
      }
      if(type != null)
      {
        GUILayout.Label("("+type.Name+")");
      }
      GUILayout.EndHorizontal();
      if(searchValue.isDirectory)
      {
        GUILayout.BeginHorizontal();
        GUILayout.Space(SRWindow.compactLabelWidth + 4);
        bool newShowUnused = EditorGUILayout.ToggleLeft("Show Unused Scripts", showUnusedScripts);
        GUILayout.EndHorizontal();

        if(newShowUnused != showUnusedScripts)
        {
          showUnusedScripts = newShowUnused;
          SRWindow.Instance.PersistCurrentSearch();
        }
      }
      if(showMoreOptions)
      {
        if(SRWindow.Instance.Compact())
        {
          GUILayout.BeginHorizontal();
          GUILayout.Space(SRWindow.compactLabelWidth);
        }
        if(!searchValue.isDirectory)
        {
          GUILayout.BeginHorizontal();
          GUILayout.Space(SRWindow.compactLabelWidth + 4);
          bool newMatchChildClasses = EditorGUILayout.ToggleLeft("Match Child Classes?", matchChildClasses);
          GUILayout.EndHorizontal();
          if(newMatchChildClasses != matchChildClasses)
          {
            matchChildClasses = newMatchChildClasses;
            SRWindow.Instance.PersistCurrentSearch();
          }
        }

        GUILayout.BeginHorizontal();
        GUILayout.Space(SRWindow.compactLabelWidth + 4);
        bool newMatchAssetMissingScript = EditorGUILayout.ToggleLeft("Match assets where script is missing.", matchAssetMissingScript);
        GUILayout.EndHorizontal();
        if(newMatchAssetMissingScript != matchAssetMissingScript)
        {
          matchAssetMissingScript = newMatchAssetMissingScript;
          SRWindow.Instance.PersistCurrentSearch();
        }

        if(SRWindow.Instance.Compact())
        {
          GUILayout.EndHorizontal();
        }
      }

      drawReplaceItem(options);
    }


    public void SetObject(UnityEngine.Object o)
    {
      searchValue.SetObject(o);
      type = ObjectUtil.getTypeFromObject(o);
    }

    void setFolder(UnityEngine.Object o)
    {
      searchValue.SetObject(o);
      initializeTypeHash();
    }

    void initializeTypeHash()
    {
      typeHash = new Dictionary<Type, ClassSearchInfo>();
      string[] assetPaths = AssetDatabase.GetAllAssetPaths();
      IEnumerable<string> filteredPaths = assetPaths.Where( asset => suffixes.Any( suffix => asset.EndsWith(suffix, System.StringComparison.OrdinalIgnoreCase)));
      string scopePath = searchValue.assetPath;

      filteredPaths = filteredPaths.Where( asset => asset.StartsWith(scopePath)).ToArray();

      foreach(string path in filteredPaths)
      {
        MonoScript ms = (MonoScript)AssetDatabase.LoadAssetAtPath(path, typeof(MonoScript));

        Type t = ms.GetClass();

        if(typeof(ScriptableObject).IsAssignableFrom(t) || typeof(MonoBehaviour).IsAssignableFrom(t))
        {
          typeHash[t] = new ClassSearchInfo(ms);
        }
      }
    }


    public override void OnDeserialization()
    {

      initReplaceOptions(replaceOptionTypes);
      if(searchValue == null)
      {
        searchValue = new ObjectID();
      }

      searchValue.OnDeserialization();

      if(searchValue.obj != null)
      {
        type = ObjectUtil.getTypeFromObject(searchValue.obj);
      }
      if(searchValue.isDirectory)
      {
        initializeTypeHash();
      }
    }

    public override bool ValueEquals(SerializedProperty prop)
    {
      if(prop.propertyPath == "m_Script")
      {
        //Don't match scripts here, we'll do it later.
        return false;
      }
      if(type == null)
      {
        return false;
      }
      if(matchAssetMissingScript)
      {
        // do not show property results for asset searches.
        return false;
      }

      if(searchValue.obj is Component){
        //compare types.
        if(prop.objectReferenceValue != null)
        {
          // Debug.Log("[DynamicTypeClass] component searching:"+prop.propertyPath);
          Type t = prop.objectReferenceValue.GetType();
          return typeMatches(t);
        }else{
          return false;
        }

      }else if(typeof(ScriptableObject).IsAssignableFrom(type))
      {
        if(prop.objectReferenceValue != null)
        {
          Type t = prop.objectReferenceValue.GetType();
          return typeMatches(t);
        }else{
          return false;
        }
      }
      else if(typeof(MonoBehaviour).IsAssignableFrom(type))
      {
        if(prop.objectReferenceValue != null)
        {
          Type t = prop.objectReferenceValue.GetType();
          return typeMatches(t);
        }else{
          return false;
        }
      }
      else{
        return searchValue.obj == prop.objectReferenceValue;
      }
    }

    bool typeMatches(Type b)
    {
      if(matchChildClasses)
      {
        return type.IsAssignableFrom(b);
      }else{
        if(searchValue.isDirectory)
        {
          ClassSearchInfo csi = null;
          if(typeHash.TryGetValue(b, out csi))
          {
            csi.numFound++;
            return true;
          }else{
            return false;
          }
        }else{
          return type == b;
        }
      }
    }

    public override bool IsValid()
    {
      return true;
    }

    //Has implicit checks for search being valid in addition to search and replace
    public override bool IsReplaceValid()
    {
      if(searchValue.isDirectory)
      {
        return false;
      }
      if(matchAssetMissingScript)
      {
        return false;
      }
      return base.IsReplaceValid();
      // return replaceItemClass.IsValid();
    }

    public override string StringValue()
    {
      if(searchValue.obj == null)
      {
        return "null";
      }
      if(searchValue.isDirectory)
      {
        return typeHash.Count + " files in '"+searchValue.obj.name+"'' folder.";
      }
      return type.Name;
    }

    public override string StringValueFor(SerializedProperty prop)
    {
      if(searchValue.obj == null)
      {
        return "null";
      }
      return prop.objectReferenceValue.GetType().Name;
    }

    public override void OnSearchBegin()
    {
      if(searchValue.isDirectory)
      {
        List<Type> keyList = new List<Type>(typeHash.Keys);
        foreach(Type key in keyList)
        {
          typeHash[key].Reset();
        }
      }
    }

    public override void OnSearchEnd(SearchJob job, SearchItem item)
    {
      if(showUnusedScripts)
      {
        foreach(var kvp in typeHash)
        {
          ClassSearchInfo csi = kvp.Value;
          if(csi.numFound == 0)
          {
            SearchResult result = new SearchResult();
            result.pathInfo = PathInfo.GetPathInfo(csi.script, job.assetData);
            result.actionTaken = SearchAction.NotFound;
            result.strRep = kvp.Key.ToString();
            job.MatchFound(result, item);
          }
        }
      }
    }

    [System.NonSerialized]
    private bool assetMissingScript;

    public override void OnAssetSearchBegin(SearchJob job, SearchItem item)
    {
      assetMissingScript = true;
    }

    public override void OnAssetSearchEnd(SearchJob job, SearchItem item)
    {
      if(job.searchIncludesScripts && matchAssetMissingScript && assetMissingScript)
      {
        SearchResult result = new SearchResult();
        result.strRep = job.assetData.assetPath;
        result.pathInfo = PathInfo.GetPathInfo(AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(job.assetData.assetPath), job.assetData);
        result.actionTaken = SearchAction.AssetMissingScript;
        job.MatchFound(result, item);
      }
    }

    public override void SearchObject( SearchJob job, SearchItem item, UnityEngine.Object obj)
    {
      if(obj is ScriptableObject)
      {
        ScriptableObject sObj = (ScriptableObject) obj;
        Type t = MonoScript.FromScriptableObject(sObj).GetClass();
        if( typeMatches(t))
        {
          if(!showUnusedScripts)
          {
            if(matchAssetMissingScript)
            {
              assetMissingScript = false;
            }else{
              SearchResult result = new SearchResult();
              result.pathInfo = PathInfo.InitWithScriptableObject(sObj, job.assetData);
              result.strRep = t.Name;

              if(job.options.searchType == SearchType.SearchAndReplace)
              {
                SerializedObject so = new SerializedObject(sObj);
                SerializedProperty m_Script = so.FindProperty("m_Script");
                if(replaceItem != null)
                {
                  replaceItem.ReplaceProperty(job, m_Script, result);
                }
              }
              job.MatchFound(result, item);
            }
          }
        }
      }
    }


    public override void SearchGameObject( SearchJob job, SearchItem item, GameObject go)
    {
      //Make sure we don't attempt to search for ScriptableObjects in GameObjects, but allow null search.
      if(searchValue.isDirectory || typeof(Component).IsAssignableFrom(type) || type == null)
      {
        Component[] components = null;
        if(searchValue.isDirectory || type == null)
        {
          components = go.GetComponents<Component>();
        }else{
         components = go.GetComponents(type);
        }
        foreach(Component c in components)
        {
          if(c == null)
          {
            if(type == null)
            {
              SearchResult result = new SearchResult();
              result.pathInfo = PathInfo.InitWithComponent(c, go, job.assetData);
              result.strRep = "Missing Component";
              result.actionTaken = SearchAction.Found;
              job.MatchFound(result, item);
            }else{
              
            }
            continue;
          }
          Type cType = c.GetType();
          //The above GetComponents Calls will include inherited children.
          if(typeMatches(cType))
          {
            if(!showUnusedScripts)
            {
              if(matchAssetMissingScript)
              {
                assetMissingScript = false;
              }else{

                SearchResult result = new SearchResult();
                result.pathInfo = PathInfo.InitWithComponent(c, go, job.assetData);
                result.strRep = cType.Name;
                result.actionTaken = SearchAction.Found;
                if(PrefabUtil.isInPrefabInstance(c))
                {
                  //Don't modify monoscripts in prefabs!!!

                }else{
                  if(job.options.searchType == SearchType.SearchAndReplace)
                  {
                    SerializedObject so = new SerializedObject(c);
                    SerializedProperty m_Script = so.FindProperty("m_Script");
                    if(replaceItem != null)
                    {
                      replaceItem.ReplaceProperty(job, m_Script, result);
                    }
                  }
                }
                job.MatchFound(result, item);
              }
            }
          }
        }
      }
    }

    public override bool _hasAdvancedOptions()
    {
      return true;
    }

    public override SerializedPropertyType PropertyType()
    { 
      return SerializedPropertyType.ObjectReference;
    }

  }
}





