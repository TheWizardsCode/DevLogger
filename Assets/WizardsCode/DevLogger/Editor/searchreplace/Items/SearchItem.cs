using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

namespace sr
{
  /** 
   * The base class for a search item. There are three types of search items:
   * SearchItemGlobal - Searches based on object type globally.
   * SearchItemProperty - Searches a property of an object.
   * SearchItemInstances - Searches for instances of a prefab.
   * These can be nested in the form of 'subsearches'.
   *
   * In general when a search is performed there are three different paths based
   * on the type of search occurring:
   * SearchProperty() - searching a specific SerializedProperty of an object.
   * SearchGameObject() - searching everything inside a game object.
   * SearchObject() - searching a UnityEngine.Object (material/texture/etc)
   * How these searches occur is dependent upon the sub-classes.
   *
   * When a SearchJob is initialized, it will create SearchSubJobs which then
   * call these functions.
   */ 
  [System.Serializable]
  public class SearchItem : INestable
  {

    //HACK! Used for sorting, but also for insertion!
    public int sortIndex = 0; 
    
    //HACK! Unity throws a big fat warning when you nest in your serialization.
    // I'm using an interface and that seems to do the trick. :(
    public INestable subsearch;
    public SubsearchScope subScope = SubsearchScope.GameObject;
    public bool subScopeRecurse = true;
    public static string subScopeRecurseLabel = "Child Objects?";

    public bool showMoreOptions = true; 

    [System.NonSerialized]
    public int searchDepth = 0;

    [System.NonSerialized]
    public SearchItem parent;

    [System.NonSerialized]
    public SearchItem root;

    public virtual string GetDescription()
    {
        return string.Empty;
    }

    public virtual string GetWarning()
    {
      return string.Empty;
    }

    public virtual bool IsValid()
    {
        return false;
    }

    public virtual bool IsReplaceValid()
    {
        return false;
    }

    public virtual void OnDeserialization()
    {

    }

    public virtual bool caresAboutAsset(SearchJob job)
    {
      return true;
    }


    protected void OnDeserializeSubSearch()
    {
      //initialize subsearches
      if(subsearch != null)
      {
        SearchItem item = (SearchItem)subsearch;
        item.searchDepth = searchDepth + 1;
        item.parent = this;
        item.root = root;
        item.OnDeserialization();
      }
    }
    
    public virtual void Draw(SearchOptions options)
    {

    }

    public virtual void DrawResult(SearchResult result)
    {

    }


    protected void drawSubsearch()
    {
      float lw = EditorGUIUtility.labelWidth;
      EditorGUIUtility.labelWidth = SRWindow.compactLabelWidth;
      if(searchDepth > 0)
      {
        GUILayout.BeginHorizontal();
        SubsearchScope newSubscope = (SubsearchScope)EditorGUILayout.EnumPopup("Scope:", subScope);
        if(newSubscope != subScope)
        {
          subScope = newSubscope;
          SRWindow.Instance.PersistCurrentSearch();
        }
        if(subScope == SubsearchScope.SameObject)
        {
          GUI.enabled = false;
          EditorGUILayout.ToggleLeft(subScopeRecurseLabel, false);
          GUI.enabled = true;
        }
        else
        {
          bool newSubScopeRecurse = EditorGUILayout.ToggleLeft(subScopeRecurseLabel, subScopeRecurse);
          if(newSubScopeRecurse != subScopeRecurse)
          {
            subScopeRecurse = newSubScopeRecurse;
            SRWindow.Instance.PersistCurrentSearch();
          }
        }

        GUILayout.EndHorizontal();
      }
      EditorGUIUtility.labelWidth = lw; // i love stateful gui! :(
    }


    // Called when the parent has made a match.
    // public virtual
    public virtual void SubsearchProperty(SearchJob job, SerializedProperty prop)
    {
      GameObject go;
      switch(subScope)
      {
        case SubsearchScope.ReturnValue:
        searchReturnedValue(job, prop);
        break;
        case SubsearchScope.SameObject:
        searchSameObject(job, prop);
        break;
        case SubsearchScope.GameObject:
        searchGameObject(job, prop, subScopeRecurse);
        break;
        case SubsearchScope.Children:
        go = getGameObjectFromProperty(prop);
        if(go != null)
        {
          foreach(Transform child in go.transform)
          {
            doSubSearch(child.gameObject, job, subScopeRecurse);
          }
        }
        break;
        case SubsearchScope.Parent:
        go = getGameObjectFromProperty(prop);
        if(go != null && go.transform.parent != null)
        {
          doSubSearch(go.transform.parent.gameObject, job, subScopeRecurse);
        }
        break;
        case SubsearchScope.Prefab:
        GameObject prefabRoot = PrefabUtil.getPrefabRoot(prop.serializedObject.targetObject);
        if(prefabRoot != null)
        {
          doSubSearch(prefabRoot, job, subScopeRecurse);
        }
        break;
      }
    }

    void searchReturnedValue(SearchJob job, SerializedProperty prop)
    {
      if(prop.propertyType == SerializedPropertyType.ObjectReference)
      {
        if(prop.objectReferenceValue != null)
        {
          SerializedObject so = new SerializedObject(prop.objectReferenceValue);
          SerializedProperty iterator = so.GetIterator();
          if(subScopeRecurse)
          {
            searchGameObject(job, iterator, subScopeRecurse);
          }else{
            SearchProperty(job, iterator);
          }
        }
      }
    }

    void searchSameObject(SearchJob job, SerializedProperty prop)
    {
      SerializedProperty iterator = prop.serializedObject.GetIterator();
      SearchProperty(job, iterator);
    }

    void searchGameObject(SearchJob job, SerializedProperty prop, bool recurse)
    {
      GameObject go = getGameObjectFromProperty(prop);
      if(go == null)
      {
        //not a go or component...but the flag says 'same game object'. Dependency?
        searchSameObject(job, prop);
      }else{
        doSubSearch(go, job, recurse);
      }
    }

    GameObject getGameObjectFromProperty(SerializedProperty prop)
    {
      UnityEngine.Object obj = prop.serializedObject.targetObject;
      if(obj is GameObject)
      {
        GameObject go = (GameObject)obj;
        return go;
      }
      if(obj is Component)
      {
        Component m = (Component)obj;
        return m.gameObject;
      }
      return null;
    }

    void doSubSearch(GameObject go, SearchJob job, bool recurse)
    {
      SerializedObject so = new SerializedObject(go);
      SerializedProperty iterator = so.GetIterator();
      SearchProperty(job, iterator);
      Component[] components = go.GetComponents<Component>();
      foreach(Component c in components)
      {
        if(c != null)
        {
          SerializedObject sc = new SerializedObject(c);
          SerializedProperty sci = sc.GetIterator();
          SearchProperty(job, sci);
        }
      }
      if(recurse)
      {
        foreach(Transform child in go.transform)
        {
          doSubSearch(child.gameObject, job, recurse);
        }
      }
    }

    public virtual void SearchProperty(SearchJob job, SerializedProperty prop)
    {

    }

    public virtual void SearchGameObject(SearchJob job, GameObject obj)
    {
      
    }

    public virtual void SearchObject(SearchJob job, UnityEngine.Object obj)
    {
      
    }

    public virtual void OnSearchBegin()
    {
        
    }

    public virtual void OnSearchEnd(SearchJob job)
    {
        
    }

    public virtual void OnAssetSearchBegin(SearchJob job)
    {

    }

    public virtual void OnAssetSearchEnd(SearchJob job)
    {

    }


    public virtual bool hasAdvancedOptions()
    {
        return false;
    }

    public SearchItem Copy()
    {
        return new SearchItem();
    }

    public virtual bool canSubsearch()
    {
      return subsearch == null;
    }

    public void drawAddRemoveButtons()
    {
      GUILayout.Space(2.0f);
      GUILayout.BeginHorizontal(); // 2
      if(searchDepth == 0)
      {
        if(GUILayout.Button(GUIContent.none, SRWindow.olPlusPlus))
        {
          SRWindow.Instance.duplicateSearchItem(this);
        }
      }
      GUILayout.FlexibleSpace();
      if(canSubsearch())
      {
        int selectedIndex = EditorGUILayout.Popup("", 0, new string[]{"Add Subsearch", Keys.PropertyLabel, Keys.GlobalLabel });
        if( selectedIndex > 0)
        {
          if( selectedIndex == 1)
          {
            if(this is SearchItemProperty)
            {
              subsearch = (SearchItemProperty)SerializationUtil.Copy(this);
            }else{
              subsearch = new SearchItemProperty();
            }
          }

          if( selectedIndex == 2)
          {
            if(this is SearchItemGlobal)
            {
              subsearch = (SearchItemGlobal)SerializationUtil.Copy(this);
            }else{
              subsearch = new SearchItemGlobal();
            }
          }
          OnDeserializeSubSearch();
          SRWindow.Instance.PersistCurrentSearch();
        }
      }
      GUILayout.FlexibleSpace();
      bool showMinus = true;
      if(searchDepth == 0 && sortIndex == 0 && SRWindow.Instance.currentSearch.items.Count == 1)
      {
        showMinus = false;
      }
      if(showMinus)
      {
        if(GUILayout.Button(GUIContent.none, SRWindow.olMinusMinus))
        {
          if(searchDepth == 0)
          {
            SRWindow.Instance.RemoveSearchItem(this); 
          }else{
            parent.subsearch = null;
            SRWindow.Instance.PersistCurrentSearch();
          }
        }
      }
      GUILayout.EndHorizontal(); // 2
    }

  }
}