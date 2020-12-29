using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
#if UNITY_2018_3_OR_NEWER
using UnityEditor.Experimental.SceneManagement;
#endif
namespace sr
{
  /**
   * Utility methods to find out useful bits about prefabs.
   */
  public class PrefabUtil
  {
    /**
     * Provides a method to determine if the given property on the given prefab instance is a modification to the existing prefab locally (in a scene).
     */
    public static bool isInstanceModification(SerializedProperty prop)
    {
      Object obj = prop.serializedObject.targetObject;

      PrefabTypes prefabType = PrefabUtil.GetPrefabType(obj);
      if(prefabType == PrefabTypes.PrefabInstance || prefabType == PrefabTypes.NestedPrefabInstance)
      {
        PropertyModification[] pms = PrefabUtility.GetPropertyModifications(obj);
#if UNITY_2018_3_OR_NEWER
        UnityEngine.Object parent = PrefabUtility.GetOutermostPrefabInstanceRoot(obj);
#else
        UnityEngine.Object parent = PrefabUtility.GetPrefabParent(obj);
#endif

        if(pms.Length > 0)
        {
          foreach(PropertyModification pm in pms)
          {
            // Debug.Log("[PrefabUtil] pm:"+pm.propertyPath);
            if(pm.target == parent && pm.propertyPath.StartsWith(prop.propertyPath) )
            {
              return true;
            }
          }
        }
      }
      return false;
    }

    public static bool isInPrefabInstance(UnityEngine.Object obj)
    {
      // Debug.Log("[PrefabUtil] prefab type:"+PrefabUtility.GetPrefabType(obj)+ " : "+ obj.name + " parent:"+PrefabUtility.GetPrefabObject(obj));
      var type = GetPrefabType(obj);
      return type == PrefabTypes.PrefabInstance || type == PrefabTypes.NestedPrefabInstance;
    }

    public static bool isPrefab(UnityEngine.Object obj)
    {
      // Debug.Log("[PrefabUtil] prefab type:"+PrefabUtility.GetPrefabType(obj)+ " : "+ obj.name + " parent:"+PrefabUtility.GetPrefabObject(obj));
      PrefabTypes p = PrefabUtil.GetPrefabType(obj);
      return p == PrefabTypes.Prefab || p == PrefabTypes.PrefabVariant || p == PrefabTypes.NestedPrefab;
    }

    public static bool isPrefabRoot(UnityEngine.Object obj)
    {
      if(obj == null)
      {
        return false;
      }
      if(obj is GameObject)
      {
        GameObject go = (GameObject) obj;
        if(isPrefab(go))
        {
#if UNITY_2018_3_OR_NEWER
          //we know this is a prefab.
          return go.transform.root.gameObject;
#else
          return PrefabUtility.FindPrefabRoot(go) == obj;
#endif
        }
      }
      return false;
    }

    /// <summary>
    /// Returns true if the prefab is nested
    /// </summary>
    public static bool IsNestedPrefab(Object obj)
    {
      if (obj == null)
        return false;
      
#if UNITY_2018_3_OR_NEWER
      var go = obj as GameObject;
      if (go == null)
        return false;
      var isAsset = go.scene.name == null;
      if (isAsset)
      {
        return go.transform.root.gameObject != go;
      }

      var outermost = PrefabUtility.GetOutermostPrefabInstanceRoot(go);
      return outermost != null && outermost != go;
#else
      return false;
#endif
    }

    public static bool IsPrefabObjectReplaceable(GameObject gameObject, SearchAssetData assetData, out string reason)
    {
      reason = "";
#if UNITY_2018_3_OR_NEWER

      if (!assetData.CanReplaceObject(gameObject, out reason))
      {
        return false;
      }
      if(PrefabUtility.GetPrefabInstanceStatus(gameObject) == PrefabInstanceStatus.NotAPrefab)
      {
        return true;
      }
      if(PrefabUtility.IsOutermostPrefabInstanceRoot(gameObject))
      {
        return true;
      }else{
        reason = "Cannot replace internal prefab objects.";
        return false;

      }
#else
      return true;
#endif
    }

    public static PrefabTypes GetPrefabType(UnityEngine.Object obj)
    {
#if UNITY_2018_3_OR_NEWER
      switch(PrefabUtility.GetPrefabAssetType(obj))
      {
        case PrefabAssetType.NotAPrefab:
        return PrefabTypes.NotAPrefab;
        case PrefabAssetType.Model:
        case PrefabAssetType.Regular:
        switch(PrefabUtility.GetPrefabInstanceStatus(obj))
        {
          case PrefabInstanceStatus.NotAPrefab:
            if (!PrefabUtility.IsPartOfAnyPrefab(obj))
            {
              
              Debug.Log("[PrefabUtil] instance not a prefab");
              return PrefabTypes.NotAPrefab;
            }
            return IsNestedPrefab(obj) ? PrefabTypes.NestedPrefab : PrefabTypes.Prefab;

          case PrefabInstanceStatus.Connected:
            return IsNestedPrefab(obj) ? PrefabTypes.NestedPrefabInstance : PrefabTypes.PrefabInstance;

          default:
          return PrefabTypes.MissingOrDisconnected;
        }
        case PrefabAssetType.Variant:
        switch(PrefabUtility.GetPrefabInstanceStatus(obj))
        {
          // if something is 'not a prefab' but inside a variant
          // then it has been added to the prefab variant
          case PrefabInstanceStatus.NotAPrefab:
          return PrefabTypes.PrefabVariant;

          case PrefabInstanceStatus.Connected:
          return PrefabTypes.PrefabVariantInstance;

          default:
          return PrefabTypes.MissingOrDisconnected;

        }

        default:
        return PrefabTypes.MissingOrDisconnected;

      }
#else
      switch(PrefabUtility.GetPrefabType(obj))
      {
        case PrefabType.None:
        return PrefabTypes.NotAPrefab;
        
        case PrefabType.Prefab:
        return PrefabTypes.Prefab;
        
        case PrefabType.PrefabInstance: 
          return PrefabTypes.PrefabInstance;

        case PrefabType.ModelPrefab:
        return PrefabTypes.ModelPrefab;
        
        case PrefabType.ModelPrefabInstance:
        return PrefabTypes.PrefabInstance;

        default:
        return PrefabTypes.MissingOrDisconnected;
      }
#endif
    }

    public static GameObject getPrefabRoot(UnityEngine.Object obj)
    {
      if(obj == null)
      {
        return null;
      }
      if(obj is GameObject)
      {
        return getPrefabRoot((GameObject)obj);
      }
      if(obj is Component)
      {
        Component c = (Component)obj;
        return getPrefabRoot(c.gameObject);
      }
      return null;
    }

    public static bool IsStagedPrefab(GameObject go)
    {
#if UNITY_2018_3_OR_NEWER
        var stage = PrefabStageUtility.GetCurrentPrefabStage();
        if (stage == null)
            return false;

        return stage.IsPartOfPrefabContents(go);
#else
        return false;
#endif
    }


    public static GameObject getPrefabRoot(GameObject go)
    {
      PrefabTypes type = PrefabUtil.GetPrefabType(go);
      if(type == PrefabTypes.PrefabInstance)
      {
#if UNITY_2018_3_OR_NEWER
        go = (GameObject)PrefabUtility.GetCorrespondingObjectFromSource(go);
#else
        go = (GameObject)PrefabUtility.GetPrefabObject(go);
#endif
        return go;
      }
#if UNITY_2018_3_OR_NEWER
      if (type == PrefabTypes.NestedPrefabInstance)
      {
        go = (GameObject) PrefabUtility.GetOutermostPrefabInstanceRoot(go);
      }
#endif
      if(type == PrefabTypes.Prefab || type == PrefabTypes.PrefabVariant || type == PrefabTypes.NestedPrefab)
      {
        return go.transform.root.gameObject;
      }
      return null;
    }

    public static bool SwapPrefab(SearchJob job, SearchItem item, SearchResult result, GameObject gameObjToSwap, GameObject prefab, bool updateTransform, bool rename)
    {
      string reason;
      if(!PrefabUtil.IsPrefabObjectReplaceable(gameObjToSwap, job.assetData, out reason))
      {
        result.actionTaken = SearchAction.InstanceNotReplaced;
        result.error = reason;
        return false;

      }

      Transform swapParent = gameObjToSwap.transform.parent;
      int index = gameObjToSwap.transform.GetSiblingIndex();

      result.replaceStrRep = prefab.name;
      result.strRep = gameObjToSwap.name;
      // Debug.Log("[ReplaceItemSwapObject] Instantiating:"  +prefab, prefab);
      GameObject newObj = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
      if(newObj != null)
      {
        newObj.transform.parent = swapParent;
        newObj.transform.SetSiblingIndex(index);
        Transform oldT = gameObjToSwap.transform;
        if(updateTransform)
        {
          newObj.transform.rotation = oldT.rotation;
          newObj.transform.localPosition = oldT.localPosition;
          newObj.transform.localScale = oldT.localScale;
        }
        if(rename)
        {
          newObj.name = gameObjToSwap.name;
        }
        result.pathInfo = PathInfo.GetPathInfo(newObj, job.assetData);
        job.assetData.assetIsDirty = true;

        replaceInstances(job, item, gameObjToSwap, newObj);


        
        return true;
      }else{
        Debug.Log("[Search & Replace] No object instantiated...hrm!");
        return false;
      }
    }

    static void replaceInstances( SearchJob parentJob, SearchItem item, GameObject oldValue, GameObject newValue)
    {
      SearchItemSet searchSet = new SearchItemSet();
      searchSet.OnDeserialization();
      searchSet.AddNew(Keys.Global, false);
      SearchItemGlobal searchItem = (SearchItemGlobal)searchSet.items[0];
      searchItem.SetType("Object");
      DynamicTypeObject dto = (DynamicTypeObject)searchItem.GetDTDFor(typeof(UnityEngine.Object));
      dto.SearchSubObjects = true;
      dto.SetObject(oldValue);
      ReplaceItemObject replaceItem = (ReplaceItemObject)dto.ReplaceItem;
      replaceItem.SetObject(newValue);

      SearchOptions options = new SearchOptions();
      options.searchType = SearchType.SearchAndReplace;
      
      // does this matter anymore since asset scope is now essentially defined by what assets are passed in?
      SearchScopeData searchScope = new SearchScopeData(ProjectScope.SpecificLocation, AssetScope.Prefabs, new ObjectID(AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(parentJob.assetData.assetPath)), false, parentJob.scope.loadPrefab);
      SearchJob subJob = new SearchJob(searchSet, options, searchScope);
      SearchAssetData assetData = parentJob.assetData.Clone();
      assetData.ExcludeObject(oldValue); // don't search internally for this object.
      subJob.AddAsset(assetData);
      subJob.Execute();
      
      // Now that we've executed the job we have to save a list of all objects from search results, because as soon as
      // we swap out the new object, the old object's position may shift in the hierarchy, making the PathInfo stale.
      SearchResultGroup group = subJob.resultSet.results[searchItem.sortIndex];
      List<UnityEngine.Object> resultObjects = new List<Object>();
      foreach (SearchResult result in group.results)
      {
        UnityEngine.Object resultObj = EditorUtility.InstanceIDToObject(result.pathInfo.objectID);
        resultObjects.Add( resultObj );
      }
      UnityEngine.Object.DestroyImmediate(oldValue);
      
      // now that we've deleted the object, let's rebuild the objects.
      for (int i=0; i < resultObjects.Count; i++ )
      {
        SearchResult result = group.results[i];
        if (resultObjects[i])
        {
          result.pathInfo = PathInfo.GetPathInfo(resultObjects[i], assetData );
        }
        else
        {
          group.results[i] = null;
        }
      }

      parentJob.AddResultsFromSubsearch(item, subJob.resultSet.results[searchItem.sortIndex]);
      
      
    }
  }
}
