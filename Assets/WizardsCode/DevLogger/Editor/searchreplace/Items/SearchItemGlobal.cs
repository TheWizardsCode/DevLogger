using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;
using System;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace sr
{
  /**
   * The search item for searching globally for a specific object or primitive type.
   */
  [System.Serializable]
  public class SearchItemGlobal : SearchItem
  {

    [System.NonSerialized]
    Dictionary<string, Type> typeHash;
    
    [System.NonSerialized]
    Dictionary<string, Type> subTypeHash;

    [System.NonSerialized]
    string[] typeOptions;

    [System.NonSerialized]
    string[] subTypeOptions;
    
    [System.NonSerialized]
    Type type;

    [System.NonSerialized]
    GameObject searchAsGameObject = null;

    [System.NonSerialized]
    InitializationContext initializationContext;

    public int depth = 0;

    int typeIndex;

    DynamicTypeField typeField;

    static int recursiveDepth = 0;

    public override void Draw(SearchOptions options)
    { 
      GUIStyle boxStyle = depth == 0 ? SRWindow.searchBox : SRWindow.searchInnerDepthBox ;
      GUILayout.BeginHorizontal(boxStyle);
      GUILayout.BeginVertical();
      SRWindow.Instance.CVS();
      drawSubsearch();

      GUILayout.BeginHorizontal();

      
      float lw = EditorGUIUtility.labelWidth;
      EditorGUIUtility.labelWidth = SRWindow.compactLabelWidth;

      string typeLabel = depth == 0 ? "Type:" : "Subtype:";
      string[] tOptions = depth == 0 ? typeOptions : subTypeOptions;
      int newIndex = EditorGUILayout.Popup(typeLabel, typeIndex, tOptions, GUILayout.MaxWidth(SRWindow.Instance.position.width - 40));
      EditorGUIUtility.labelWidth = lw; // i love stateful gui! :(

      if(newIndex != typeIndex)
      {
        typeIndex = newIndex;
        initializationContext = new InitializationContext(typeHash[tOptions[typeIndex]]);
        typeField.SetType(initializationContext);
        SRWindow.Instance.PersistCurrentSearch();
      }

      if(depth == 0)
      {
        if(typeField.hasAdvancedOptions()) 
        {
          bool newShowMoreOptions = EditorGUILayout.Toggle( showMoreOptions, SRWindow.optionsToggle, GUILayout.Width(15));
          if(newShowMoreOptions != showMoreOptions)
          {
            showMoreOptions = newShowMoreOptions;
            typeField.showMoreOptions = showMoreOptions;
            SRWindow.Instance.PersistCurrentSearch();
          }
        }
      }else{
        // Debug.Log("[SearchItemGlobal] depth:"+depth);
        //show more options is controlled by this search item's parent.
      }
      GUILayout.EndHorizontal();

      typeField.showMoreOptions = showMoreOptions;
      SearchOptions typeFieldOptions = options.Copy();

      if(subsearch != null)
      {
        typeFieldOptions.searchType = SearchType.Search;
      }
      typeField.Draw(typeFieldOptions);

      if(depth == 0)
      {
        if(subsearch != null)
        {
          SearchItem item = (SearchItem)subsearch;
          item.Draw(options);
        }

        drawAddRemoveButtons();
      }

     GUILayout.EndVertical();

      SRWindow.Instance.CVE();

      GUILayout.EndHorizontal();
    }


    // Fixes nulls in serialization manually...*sigh*.
    public override void OnDeserialization()
    {
      if(typeField == null)
      {
        typeField = new DynamicTypeField();
        typeField.showMoreOptions = true;
      }

      typeField.searchItem = this;
      typeHash = new Dictionary<string, Type>();
      typeHash["Text"] = typeof(string);
      typeHash["Object"] = typeof(UnityEngine.Object);
      typeHash["Float, Double"] = typeof(float);
      typeHash["Integer, Long"] = typeof(long);
      typeHash["Boolean"] = typeof(bool);
      typeHash["Color"] = typeof(Color);
      typeHash["Vector2"] = typeof(Vector2);
      typeHash["Vector3"] = typeof(Vector3);
      typeHash["Vector4"] = typeof(Vector4);
      typeHash["Quaternion"] = typeof(Quaternion);
      typeHash["Rect"] = typeof(Rect); 
      string classSearchLabel = "Scripts";
      typeHash[classSearchLabel] = typeof(Type); 
      typeOptions = typeHash.Keys.ToArray();
      subTypeHash = new Dictionary<string, Type>(typeHash);
      subTypeHash.Remove(classSearchLabel);
      subTypeOptions = subTypeHash.Keys.ToArray();

      typeField.OnDeserialization(); // sigh...this is getting old.
      typeField.showMoreOptions = showMoreOptions;

      typeField.depth = depth;
      initializationContext = new InitializationContext(typeHash[typeOptions[typeIndex]]);

      // I feel like this could be improved. If perhaps we implement more places where
      // replace functionality becomes more complicated this should be revisited.
      DynamicTypeObject objData = (DynamicTypeObject)GetDTDFor(typeof(UnityEngine.Object));
      objData.InitReplaceOptionsForGlobal();
      typeField.SetType(initializationContext);
      OnDeserializeSubSearch();
    }

    /**
     * Used to build out the 
     */
    public void SetType(string key)
    {
      initializationContext = new InitializationContext(typeHash[key]);
      typeField.SetType(initializationContext);
    }

    public DynamicTypeData GetDTDFor(Type type)
    {
      return typeField.GetDTDFor(type);
    }

    public void SearchProperty(SearchJob job, SearchItem item, SerializedProperty prop)
    {
      bool onlyVisible = !AnimationUtil.isAnimationObject(prop.serializedObject.targetObject);
      bool isScene = job.assetData.assetScope == AssetScope.Scenes;
      SerializedProperty iterator = prop.Copy();
      SerializedProperty endProperty = null;
      if(depth == 0)
      {
      }else{
        endProperty = iterator.GetEndProperty();
      }

      // if(endProperty != null)
      // {
      //   Debug.Log("[SearchItemGlobal] STARTING on "+iterator.propertyPath + " ENDING on "+endProperty.propertyPath);
      // }

      while(Next(iterator, onlyVisible))
      {

        if(ignorePropertyOfType(iterator))
        {
          continue;
        }
        if(endProperty != null && SerializedProperty.EqualContents(iterator, endProperty))
        {
          return;
        }


        // Its possible that we may have sub-objects that are serialized 
        // scriptable objects within a scene. These aren't scriptable objects 
        // on disk, and so we need to create a SerializedObject representation
        // and go a bit deeper.
        if(isScene)
        {
          if(iterator.propertyType == SerializedPropertyType.ObjectReference &&iterator.objectReferenceValue is ScriptableObject)
          {
            string path = AssetDatabase.GetAssetPath(iterator.objectReferenceValue.GetInstanceID());
            if(path == "")
            {
              // Debug.Log("[SearchItemGlobal] found scriptable object serialized within a scene."+iterator.propertyPath);
              SerializedObject so = new SerializedObject(iterator.objectReferenceValue);
              recursiveDepth ++;
              if(recursiveDepth < 100)
              {
                string internalID = job.assetData.assetPath + so.targetObject.GetInstanceID();
                SearchAssetData internalObjectData = null;
                if(!job.searchAssetsData.TryGetValue(internalID, out internalObjectData))
                {

                  // Debug.Log("Searching:"+ iterator.propertyPath + " " +so.targetObject.GetInstanceID());
                  job.addInternalAsset(job.assetData.assetPath, so.targetObject.GetInstanceID());
                  SearchProperty(job, item, so.GetIterator());
                }
                // else{
                //   Debug.Log("Already searched this internal object:"+so.targetObject.GetInstanceID());
                // }
              }else{
                // hit recursive depth!
                Debug.Log("[Search & Replace] Recursive depth hit!");
              }
              recursiveDepth --;
            }
          }
        }
        searchPropertyInternal(job, item, prop, iterator);
      }

      if(typeHidesName(prop.serializedObject.targetObject))
      {
        SerializedProperty nameProp = prop.serializedObject.FindProperty("m_Name");
        searchPropertyInternal(job, item, prop, nameProp);
      }
    }

    // Some object types have the visibility of the name unseen. This function
    // looks at the type and returns true if its of this type.
    bool typeHidesName(UnityEngine.Object obj)
    {
      if(
          obj is ScriptableObject ||
          obj is Material ||
          obj is Texture ||
          obj is AudioClip
        )
      {
        return true;
      }
      return false;
    }

    void searchPropertyInternal(SearchJob job, SearchItem item, SerializedProperty prop, SerializedProperty iterator)
    {
      if(iterator.propertyType == typeField.PropertyType())
      {
        if(typeField.ValueEqualsWithConditional(iterator))
        {
          if(item.subsearch != null)
          {
            SearchItem subItem = (SearchItem)item.subsearch;
            subItem.SubsearchProperty(job,prop);
          }else{

            SearchResultProperty result = new SearchResultProperty(iterator, typeField.StringValueFor(iterator), job);
            // If the prefab type is 'instance' then only add the
            // search result if it is a modification to the original
            // prefab.
            // We iterate over prefab separately and the PrefabType will simply 
            // be 'Prefab' and we should handle that before we handle scenes.
            if(result.pathInfo.prefabType == PrefabTypes.PrefabInstance || result.pathInfo.prefabType == PrefabTypes.NestedPrefabInstance)
            {
                if(job.options.searchType == SearchType.SearchAndReplace)
                {
                  result.actionTaken = SearchAction.InstanceReplaced;
                }else{
                  result.actionTaken = SearchAction.InstanceFound;
                }
                // if(item.subsearch == null && PrefabUtil.isInstanceModification(iterator))
                // {
                  //Apply change, only if change already exists!
                  typeField.ReplaceProperty(job, iterator, result);
                // }
                job.MatchFound(result, item);
            }else{
              if(item.subsearch == null)
              {
                typeField.ReplaceProperty(job, iterator, result);
              }
              job.MatchFound(result, item); 
            }
          }
        }
      }
    }

    bool Next(SerializedProperty prop, bool visible)
    {
      if(visible)
      {
        return prop.NextVisible(true);
      }else{
        return prop.Next(true);
      }
    }

    public override void SearchProperty(SearchJob job, SerializedProperty prop)
    {
      SearchProperty(job, this, prop);
    }

    public override void OnSearchBegin()
    {
      searchAsGameObject = typeField.GetGameObject();
      typeField.OnSearchBegin();
    }

    public override void OnSearchEnd(SearchJob job)
    {
      typeField.OnSearchEnd(job, this);
    }

    public override void OnAssetSearchBegin(SearchJob job)
    {
      typeField.OnAssetSearchBegin(job, this);
    }

    public override void OnAssetSearchEnd(SearchJob job)
    {
      typeField.OnAssetSearchEnd(job, this);
    }


    public override void SearchObject(SearchJob job, UnityEngine.Object obj)
    {
       job.assetData.searchExecuted = true;
       typeField.SearchObject(job, this, obj);
    }

    public override void SearchGameObject(SearchJob job, GameObject go)
    {
      job.assetData.searchExecuted = true;
      //We only do this search if its a game object AND its a search (NOT a replace).
      if(searchAsGameObject != null && job.options.searchType == SearchType.Search && subsearch == null)
      {
#if UNITY_2018_2_OR_NEWER
        UnityEngine.Object goPrefab = PrefabUtility.GetCorrespondingObjectFromSource(go);
#else
        UnityEngine.Object goPrefab = PrefabUtility.GetPrefabParent(go);
#endif

        // Debug.Log("[SearchItemGlobal] goPrefab:"+goPrefab);
        if(goPrefab != null && goPrefab == searchAsGameObject)
        {
          SearchResult result = new SearchResult();
          result.strRep = "";
          result.pathInfo = PathInfo.GetPathInfo(go, job.assetData);
          result.actionTaken = SearchAction.InstanceFound;
          job.MatchFound(result, this);
        }
      }
      typeField.SearchGameObject(job, this, go);
    }


    //This seems unnecessary as long as I use 'nextVisible'?
    public bool ignorePropertyOfType(SerializedProperty prop)
    {
      bool ignore = false;
      if(prop.serializedObject.targetObject is Material)
      {
        //don't return the transform's internal m_children var.
        ignore = prop.propertyPath.IndexOf("first.name") > -1;
        if(ignore)
        {
          return true;
        }

      }
      return ignore;
    }

    public override bool IsValid()
    {
      return typeField.IsValid();
    }

    public override bool IsReplaceValid()
    {
      return typeField.IsReplaceValid(); //Typefield has implicit checks for search being valid in addition to search and replace
    }

    public override string GetWarning()
    {
      if(depth == 0)
      {
        return typeField.GetWarning();
      }
      return string.Empty;
    }

    public override string GetDescription()
    {
      return typeField.StringValueWithConditional();
    }

    public override bool hasAdvancedOptions()
    {
        return typeField.hasAdvancedOptions();
    }


  }
}