using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

namespace sr
{
  /**
   * Defines the scope of a search.
   * There are two types of scope: locations and assets.
   * Location scope defines whether we want to search the entire project or 
   * limit the search to a specific location.
   * Asset scope defines the types of assets we want to search. Each type of 
   * search has dramatic implications in terms of search speed. This class provides
   * a UI to select the options, and also provides search jobs information that 
   * can be used to pre-determine which assets to search in order to speed up 
   * search.
   */
  [System.Serializable]
  public class SearchScope
  {

    [System.NonSerialized]
    string[] scopeOptionStrings;

    ProjectScope projectScope = defaultScope;
    
    static ProjectScope defaultScope =  ProjectScope.EntireProject;
    public static AssetScope allAssets = (AssetScope)0xFF;

    public AssetScope assetScope = allAssets;


    public ObjectID objID; //folder or item to search.

    bool searchDependencies = true;
    /// <summary>
    /// Controls how prefabs are loaded. If prefabs should be manipulated directly, or if it should be loaded into its own scene.
    /// In 2018.3 or lower, this has no effect. In 2018.3 or newer this has an effect.
    /// </summary>
    bool loadPrefab = true;

    [System.NonSerialized]
    string scopeDescription;

    // Fixes nulls in serialization manually...*sigh*.
    public void OnDeserialization()
    {
      if(objID == null)
      {
        objID = new ObjectID();
      }
      // hey chris! if you screw up up your hexadecimal bitmasking again, uncomment this!
      // assetScope = allAssets;

      if(scopeOptionStrings == null)
      {
        scopeOptionStrings = new string[]{
          Keys.Everything,
          Keys.Location
        };
      }
      objID.OnDeserialization();
    }

    public void Draw(SearchOptions options)
    {
        // GUILayout.Space(2);

        GUILayout.BeginHorizontal();
        SRWindow.Instance.CVS();
        GUILayout.BeginHorizontal();

        float lw = EditorGUIUtility.labelWidth;
        EditorGUIUtility.labelWidth = SRWindow.compactLabelWidth;
        ProjectScope newProjectScope = (ProjectScope)EditorGUILayout.EnumPopup("Scope:", projectScope);
        if(newProjectScope != projectScope)
        {
          projectScope = newProjectScope;
          SRWindow.Instance.PersistCurrentSearch();
        }
        if(projectScope == ProjectScope.SpecificLocation)
        {
          //Display interface for selecting a folder or scene.
          EditorGUIUtility.labelWidth = SRWindow.compactLabelWidth + 10;

          UnityEngine.Object newValue = EditorGUILayout.ObjectField(objID.obj, typeof(UnityEngine.Object), true);
          if(newValue != objID.obj)
          {
            objID.SetObject(newValue);
            SRWindow.Instance.PersistCurrentSearch();
          }
          GUILayout.EndHorizontal();

          if(objID.isDirectory)
          {
            drawAssetScope();
          }
        }else if(projectScope == ProjectScope.SceneView)
        {
          //Don't display scope.
          GUILayout.EndHorizontal();
        }else if(projectScope == ProjectScope.CurrentSelection)
        {
          GUILayout.EndHorizontal();
        }else{
          //scope only.
          GUILayout.EndHorizontal();
          drawAssetScope();

        }
        EditorGUIUtility.labelWidth = lw; // i love stateful gui! :(

        SRWindow.Instance.CVE();
        GUILayout.EndHorizontal();
        
        if (SRWindow.Instance.Compact())
        {
          GUILayout.BeginVertical();
        }
        else
        {
          GUILayout.BeginHorizontal();
        }

        drawDependencySelector();
        drawLoadPrefabsUI(options);
        
        if (SRWindow.Instance.Compact())
        {
          GUILayout.EndVertical();
        }
        else
        {
          GUILayout.EndHorizontal();
        }
    }

    public void drawDependencySelector()
    {
      GUILayout.BeginHorizontal();
      GUILayout.Space(SRWindow.compactLabelWidth + 4); //magical padding numbers.
      bool newVal = EditorGUILayout.ToggleLeft("Dependencies?", searchDependencies);
      if(newVal != searchDependencies)
      {
        searchDependencies = newVal;
        SRWindow.Instance.PersistCurrentSearch();
      }
      GUILayout.EndHorizontal();
      

    }

    void drawLoadPrefabsUI(SearchOptions options)
    {
#if UNITY_2018_3_OR_NEWER
      GUILayout.BeginHorizontal();

      if (SRWindow.Instance.Compact())
      {
        GUILayout.Space(SRWindow.compactLabelWidth + 4); //magical padding numbers.
      }
      
      
      GUIContent loadPrefabGUI = new GUIContent("Load Prefabs?", "Whether this prefab should be loaded into its own scene before being searched. This should be on by default. If you have scripts that modify your objects in the editor upon load, you can turn this off. When this is off, certain replace operations will not correctly save. For more info, please read 'Search Options' in the help section. ");
      bool newLoadPrefab = EditorGUILayout.ToggleLeft(loadPrefabGUI, loadPrefab);
      if(newLoadPrefab != loadPrefab)
      {
        loadPrefab = newLoadPrefab; 
        SRWindow.Instance.PersistCurrentSearch();
      } 
      GUILayout.EndHorizontal();

#endif
    }

    public bool isSearchingInScene()
    {
      if(projectScope == ProjectScope.CurrentSelection)
      {
        return Selection.transforms.Length > 0;
      }
      //else
      return projectScope == ProjectScope.SceneView;
    }

    public bool isSearchingDependencies()
    {
      return searchDependencies;
    }


    bool selectionIsSceneObjects()
    {
      UnityEngine.Object[] filteredSelection = Selection.GetFiltered(typeof(GameObject), SelectionMode.Unfiltered);
      return filteredSelection.Length == Selection.objects.Length;
    }

    string prettyAssetScope()
    {
      if(assetScope ==  allAssets)
      {
        return "All Assets";
      }
      if(assetScope == AssetScope.None)
      {
        return "Nothing";
      }
      List<string> items = new List<string>();
      if( (assetScope & AssetScope.Prefabs) == AssetScope.Prefabs)
      {
        items.Add("Prefabs");
      }
      if( (assetScope & AssetScope.Scenes) == AssetScope.Scenes)
      {
        items.Add("Scenes");
      }
      if( (assetScope & AssetScope.Materials) == AssetScope.Materials)
      {
        items.Add("Materials");
      }
      if( (assetScope & AssetScope.ScriptableObjects) == AssetScope.ScriptableObjects)
      {
        items.Add("Scriptable Objects");
      }
      if( (assetScope & AssetScope.Animations) == AssetScope.Animations)
      {
        items.Add("Animation Clips");
      }

      return string.Join(", ", items.ToArray());

    }

    string prettySelection()
    {
      if(Selection.objects.Length == 0)
      {
        return "nothing";
      }
      int maxObjects = 3;
      int itemNum = System.Math.Min(maxObjects, Selection.objects.Length);
      List<string> items = new List<string>();
      for(int i = 0; i < itemNum; i++)
      {
        UnityEngine.Object obj = Selection.objects[i];
        items.Add(obj.name);
      }
      string itemsStr = string.Join(", ", items.ToArray());
      if(Selection.objects.Length <= maxObjects)
      {
        return itemsStr;
      }else{
        return itemsStr + " and "+ (Selection.objects.Length - itemNum) + " more objects"; 
      }
    }

    void drawAssetScope()
    {
      if(SRWindow.Instance.Compact())
      {
        GUILayout.BeginHorizontal();
      }
      else{
        GUILayout.BeginHorizontal();
      }

      string[] scopeLabels = new string[]{"Prefabs", "Scenes", "Scriptable Objects", "Materials", "Animations", "Animators", "Textures", "AudioClips"};
      AssetScope newAssetScope = (AssetScope)EditorGUILayout.MaskField("Assets:", (int)assetScope, scopeLabels);

      if(newAssetScope != assetScope)
      {
        assetScope = newAssetScope;
        SRWindow.Instance.PersistCurrentSearch();
      }

      if(SRWindow.Instance.Compact())
      {
        GUILayout.EndHorizontal();
      }
      else{
        // GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();
      }

    }

    public bool IsValid(SearchOptions options)
    {
      if(Application.isPlaying)
      {
        if(projectScope == ProjectScope.SceneView)
        {
          return true;
        }
        if(projectScope == ProjectScope.CurrentSelection)
        {
          return true;
        }
        if(projectScope == ProjectScope.SpecificLocation && objID.isSceneObject && objID.obj != null)
        {
          return true;
        }else{

          return false;
        }
      }

      if(projectScope == ProjectScope.EntireProject)
      {
        if(assetScope != AssetScope.None)
        {
          return true;
        }
      }
      if(projectScope == ProjectScope.SceneView)
      {
        // We do not care what the scope says.
        return true;
      }

      if(projectScope == ProjectScope.CurrentSelection)
      {
        if(Selection.objects.Length > 0)
        {
          return true;
        }else{
          return false;
        }
      }
      else
      {
        // Debug.Log("[SearchScope] "+(uint)assetScope);
        if(objID.obj != null && assetScope != AssetScope.None)
        {
          return true;
        }
      }
      return false;

    }

    AssetScope DrawAssetScopeButton(AssetScope val, AssetScope currentMask, GUIContent content)
    {
      bool selected = GUILayout.Toggle((currentMask & val) == val, content, SRWindow.toolbarButton, GUILayout.MinWidth(30));
      if(selected)
      {
        if( (currentMask & val) == 0)
        {
          currentMask |= val;
        }
      }else{
        if( (currentMask & val) == val)
        {
          currentMask &= ~val;
        }
      }
      return currentMask;
    }

    public SearchScopeData ToData(SearchOptions options)
    {
      return new SearchScopeData(projectScope, assetScope, objID.Copy(), searchDependencies, loadPrefab);
    }

    public void changeScopeTo(ProjectScope scope)
    {
      projectScope = scope;

    }

    public ProjectScope getCurrentScope()
    {
      return projectScope;
    } 


  }
}