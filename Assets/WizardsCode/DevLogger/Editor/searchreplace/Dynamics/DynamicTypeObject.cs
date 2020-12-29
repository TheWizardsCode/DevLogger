using UnityEngine;
using UnityEditor;
using System;
using System.Collections;
using System.Collections.Generic;

namespace sr
{
  /**
   * Provides a search field for UnityEngine.Object.
   */
  [System.Serializable]
  public class DynamicTypeObject : DynamicTypeData<ObjectID>
  {
    [System.NonSerialized]
    bool searchTypesOK;

    [System.NonSerialized]
    bool searchScopeOK;

    bool overrideSafeSearch = false;

    //Whether or not the user has selected they want to search for sub objects of
    // this object.
    bool searchSubObjects =  true;

    public bool SearchSubObjects
    {
      get
      {
        return searchSubObjects;
      }
      set
      {
        searchSubObjects = value;
      }
    }

    // Whether the type of search we are currently doing allows us to do a search
    // for sub-objects.
    bool shouldSearchSubObjects = false;

    // Whether the currently selected object is a type that allows for subobjects.
    bool hasSubObject = false;

    [System.NonSerialized]
    Dictionary<UnityEngine.Object, string> subObjects;

    public static List<Type> replaceOptionTypes = new List<Type>()
    {
      typeof(ReplaceItemObject),  
      typeof(ReplaceItemWithReference),  
    };

    public static List<Type> replaceOptionTypesForGlobal = new List<Type>()
    {
      typeof(ReplaceItemObject),  
    };

    public void InitReplaceOptionsForGlobal()
    {
      initReplaceOptions(replaceOptionTypesForGlobal);
    }

    public override void Draw(SearchOptions options)
    {
      drawControlStart();
      UnityEngine.Object newValue = EditorGUILayout.ObjectField(searchValue.obj, parent.type, true);
      drawControlEnd();
      if(newValue != null && !parent.type.IsAssignableFrom(newValue.GetType()))
      {
        newValue = null;
      }
      if(newValue != searchValue.obj)
      {
        SetObject(newValue);
        SRWindow.Instance.PersistCurrentSearch();
      } 
      if(shouldSearchSubObjects && hasSubObjects())
      {
        bool newSearchSubObjects = EditorGUILayout.Toggle("Search for "+subObjectsType()+"?", searchSubObjects);
        if(newSearchSubObjects != searchSubObjects)
        {
          searchSubObjects = newSearchSubObjects;
          hashSubObjects();
          SRWindow.Instance.PersistCurrentSearch();
        }
      }

      if(searchValue.obj is Material && Application.isPlaying)
      {
        EditorGUILayout.HelpBox("Searching materials while the game is playing may not return results due to Unity copying the material under the hood. It is still possible to search inside materials for textures, colors, and other data.", MessageType.Warning);

      }
   
      if(searchValue.isSceneObject && !SRWindow.Instance.isSearchingInScene() )
      { 
        if(SRWindow.Instance.getCurrentScope() == ProjectScope.CurrentSelection)
        {
          // We've got a correct scope, but the selection is not a 
          // scene object! Try to help by informing the user that they have to
          // select something in the scene first.
          EditorGUILayout.HelpBox("Cannot search the current selection because scene objects can only be searched for inside scenes.", MessageType.Warning);
          searchScopeOK = false;

        }else{
          // We've got an incorrect scope for the scene object! Try to help by
          // informing the user this is an invalid search and give them an 
          // option out of it.
          EditorGUILayout.HelpBox("The current selection is a scene object and you aren't searching the current scene.", MessageType.Warning);
          searchScopeOK = false;
          
          GUILayout.BeginHorizontal();
          if(GUILayout.Button("Set Scope to the Scene View"))
          {
            SRWindow.Instance.changeScopeTo(ProjectScope.SceneView);
            searchScopeOK = true;
          }
          GUILayout.EndHorizontal();
        }
        // Debug.Log("[Search & Replace] Objects within a scene cannot be searched for outside of a scene. ");
      }else{
        searchScopeOK = true;
      }

      drawReplaceItem(options);
      if(replaceItem is ReplaceItemObject)
      {
        ReplaceItemObject rio = (ReplaceItemObject)replaceItem;
        UnityEngine.Object ro = rio.replaceValue.obj;
        if(ro != null &&
           searchValue.obj != null &&
          !searchValue.obj.GetType().IsAssignableFrom(ro.GetType()) &&
          options.searchType == SearchType.SearchAndReplace
          )
        {
          EditorGUILayout.HelpBox("The objects you're trying to search and replace are incompatible.", MessageType.Warning);
          searchTypesOK = false;
        }else{
          searchTypesOK = true;
        }
      }else{
        searchTypesOK = true;
      }
      if(!searchTypesOK)
      {
        // It is possible that users may want to search for disparate object which
        // is highly unsafe. For example an interface may change from a scriptable
        // object to a monobehaviour or vice versa.
        bool newOverride = EditorGUILayout.Toggle("Allow Unsafe Search", overrideSafeSearch);
        if(newOverride != overrideSafeSearch)
        {
          overrideSafeSearch = newOverride;
          SRWindow.Instance.PersistCurrentSearch();
        }
      }

    }

    public void SetObject(UnityEngine.Object newValue)
    {
      searchValue.SetObject(newValue);
      searchTypesOK = true;
      searchScopeOK = true;
      hashSubObjects();
      replaceItem.OnDTDUpdate();
    }

    public override void OnDeserialization()
    {
      initReplaceOptions(replaceOptionTypes);
      if(searchValue == null)
      {
        searchValue = new ObjectID();
      }
      searchValue.OnDeserialization();
      hashSubObjects();
    }

    public override bool ValueEquals(SerializedProperty prop)
    {
      UnityEngine.Object o = prop.objectReferenceValue;
      bool isObject = searchValue.obj == o;
      if(isObject)
      {
        return true;
      }
      if(o == null)
      {
        return false;
      }
      if(shouldSearchSubObjects && searchSubObjects)
      {
        return subObjects.ContainsKey(prop.objectReferenceValue);
      }
      return false;
    }

    public override bool IsValid()
    {
      bool safeSearch = searchTypesOK;
      if(overrideSafeSearch)
      {
        safeSearch = true;
      }
      return safeSearch && searchScopeOK;
    }

    public override bool IsReplaceValid()
    {
      return IsValid() && base.IsReplaceValid();
    }


    public override string StringValue()
    {
      if(searchValue.obj == null)
      {
        return "null";
      }
      string retVal = searchValue.obj.name;
      if(shouldSearchSubObjects && searchSubObjects && hasSubObject)
      {
        retVal += " (and "+ subObjectsType()+ ")";
      }
      return retVal;
    }

    public override string StringValueFor(SerializedProperty prop)
    {
      UnityEngine.Object o = prop.objectReferenceValue;
      if(o == null)
      {
        return "null";
      }
      if(searchValue.obj == o)
      {
        return o.name;
      }
      if(shouldSearchSubObjects && searchSubObjects && hasSubObject)
      {
        return subObjects[o];
      }
      return "";
    }


    public override SerializedPropertyType PropertyType()
    {
      return SerializedPropertyType.ObjectReference;
    }

    public override bool _hasAdvancedOptions()
    {
      return true;
    }

    protected override void initializeDefaultValue(SerializedProperty prop)
    {
      searchValue.SetObject(prop.objectReferenceValue);
    }

    public override string GetWarning()
    {
      if(searchValue.obj == null)
      {
        return "Searching for NULL globally will return a large number of results. Are you sure you want to proceed?";
      }
      return string.Empty;
    }

    public override void OnSelect(InitializationContext ic)
    {
      shouldSearchSubObjects = ic.fieldData.fieldDataType == FieldData.FieldDataType.Weak;
      base.OnSelect(ic);
    }

    bool hasSubObjects()
    {
      UnityEngine.Object obj = searchValue.obj;
      if(obj is GameObject)
      {
        return true;
      }
      if(obj is Texture2D)
      {
        UnityEngine.Object[] objects = AssetDatabase.LoadAllAssetsAtPath( searchValue.assetPath );
        return objects.Length > 1;
      }
      return false;
    }

    string subObjectsType()
    {
      UnityEngine.Object obj = searchValue.obj;
      if(obj is GameObject)
      {
        return "components";
      }
      if(obj is Texture2D)
      {
        return "sprites";
      }
      return "";
    }

    // Hashes our object into sub-objects that will also be searched for.
    void hashSubObjects()
    {
      if(subObjects == null)
      {
        subObjects = new Dictionary<UnityEngine.Object, string>();
      }
      if(searchSubObjects)
      {
        hasSubObject = hasSubObjects();//checks for null!
        if(hasSubObject) 
        {

          UnityEngine.Object obj = searchValue.obj;
          if(obj is GameObject)
          {
            GameObject go = (GameObject)obj;
             Component[] components = go.GetComponents<Component>();
             foreach(Component c in components)
             {
              if(c != null)
              {
                subObjects[c] = c.name +"."+c.GetType();
              }
             }
          }
          if(obj is Texture2D)
          {
            UnityEngine.Object[] objects = AssetDatabase.LoadAllAssetsAtPath( searchValue.assetPath );
            foreach(UnityEngine.Object o in objects)
            {
              subObjects[o] = o.name + " (Sprite)";
            }
          }
        }else{
          subObjects.Clear();
        }
      }else{
        subObjects.Clear();
      }
    }
  }
}
