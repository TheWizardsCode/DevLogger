using UnityEngine;
using UnityEditor;
using System;
using System.Text;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace sr
{
  /**
   * The search item for searching for instances of a specific prefab.
   */
  [System.Serializable]
  public class SearchItemInstances : SearchItem
  {

    ObjectID objID;

    [System.NonSerialized]
    bool searchValid = false;

    ReplaceItemPrefabInstance replaceItem;

    public override void Draw(SearchOptions options)
    { 
      GUILayout.BeginHorizontal(SRWindow.searchBox );
      SRWindow.Instance.CVS();
      GUILayout.BeginVertical();

      GUILayout.BeginHorizontal();
      float lw = EditorGUIUtility.labelWidth;
      EditorGUIUtility.labelWidth = SRWindow.compactLabelWidth;

      UnityEngine.Object newObj = (UnityEngine.Object) EditorGUILayout.ObjectField("Prefab:", objID.obj, typeof(UnityEngine.Object), true, GUILayout.MaxWidth(SRWindow.Instance.position.width - SRWindow.boxPad));
      EditorGUIUtility.labelWidth = lw; // i love stateful gui! :(

      if(objID.obj != newObj)
      {
        newObj = PrefabUtil.getPrefabRoot(newObj);

        objID.SetObject(newObj);
        searchValid = validateSearch(objID.obj);
        SRWindow.Instance.PersistCurrentSearch();
      }
      GUILayout.EndHorizontal();

      DrawValidSearch(searchValid, objID.obj);
      if(options.searchType == SearchType.SearchAndReplace)
      {
        replaceItem.Draw();
      }


      drawAddRemoveButtons();
      GUILayout.EndVertical();
      SRWindow.Instance.CVE();

      GUILayout.EndHorizontal();
    }

    public void Swap()
    {
      UnityEngine.Object obj = objID.obj;
      objID.SetObject(replaceItem.objID.obj);
      replaceItem.objID.SetObject(obj);
      SRWindow.Instance.PersistCurrentSearch();
    }

    public void DrawValidSearch(bool isValid, UnityEngine.Object obj)
    {
      if(!isValid)
      {
        string warningInfo = "";
        if(obj != null)
        {
          PrefabTypes type = PrefabUtil.GetPrefabType(obj);
          if(type == PrefabTypes.PrefabInstance || type == PrefabTypes.NestedPrefabInstance)
          {
            warningInfo = obj.name + " is a prefab instance, not a prefab.";
          }else{
            warningInfo = obj.name + " is not a prefab root.";
          }
        }

        if(warningInfo.Length > 0)
        {
          EditorGUILayout.HelpBox(warningInfo, MessageType.Warning);
        }
      }
    }

    public bool validateSearch(UnityEngine.Object obj)
    {
      return PrefabUtil.isPrefabRoot(obj);
    }

    public void InitWithObject(UnityEngine.Object obj)
    {
      if(objID == null)
      {
        objID = new ObjectID();
      }
      obj = PrefabUtil.getPrefabRoot(obj);
      objID.SetObject(obj);
      OnDeserialization();

    }

    // Fixes nulls in serialization manually...*sigh*.
    public override void OnDeserialization()
    {
      if(objID == null)
      {
        objID = new ObjectID();
      }
      objID.OnDeserialization();
      searchValid = validateSearch(objID.obj);

      if(replaceItem == null)
      {
        replaceItem = new ReplaceItemPrefabInstance();
      }
      replaceItem.parentSearchItem = this;
      replaceItem.OnDeserialization();

    }


    public void SearchProperty(SearchJob job, SearchItem item, SerializedProperty prop)
    {

    }

    public override void SearchProperty(SearchJob job, SerializedProperty prop)
    {
      SearchProperty(job, this, prop);
    }

    public override void SearchObject(SearchJob job, UnityEngine.Object obj)
    {

    }

    public override void SearchGameObject(SearchJob job, GameObject go)
    {
      job.assetData.searchExecuted = true;
      PrefabTypes goType = PrefabUtil.GetPrefabType(go);
      if(goType == PrefabTypes.PrefabInstance || goType == PrefabTypes.NestedPrefabInstance || goType == PrefabTypes.PrefabVariantInstance)
      {
        GameObject goPrefabObj = null;
#if UNITY_2018_3_OR_NEWER
        if(goType == PrefabTypes.PrefabInstance || goType == PrefabTypes.NestedPrefabInstance)
        {
          // nested prefabs or normal instances means we should search for the *original* source.
          // prefab variant instances will show up here as nestedprefab instances
          // if the variants are in variants. :P
          goPrefabObj = PrefabUtility.GetCorrespondingObjectFromOriginalSource(go);
        }else{ 
          // PrefabVariantInstance means we need the corresponding source.
          goPrefabObj = PrefabUtility.GetCorrespondingObjectFromSource(go);
        }
        GameObject root = go;
#else
        goPrefabObj = (GameObject)PrefabUtility.GetPrefabParent(go);
        GameObject root = PrefabUtility.FindPrefabRoot(go);
#endif
        bool isRootOfPrefab = root == go;
        if(isRootOfPrefab && goPrefabObj == objID.obj)
        {
          //Instance found!
          // Debug.Log("[SearchItemInstances] instance found!"+go.name);
          SearchResult result = new SearchResult();
          result.strRep = "";
          result.pathInfo = PathInfo.GetPathInfo(go, job.assetData);
          result.actionTaken = SearchAction.InstanceFound;
          job.MatchFound(result, this);
          replaceItem.ReplaceInstance(job, this, go, result);
        }
      }
    }

    public override bool IsValid()
    {
      return searchValid && !Application.isPlaying;
    }

    public override bool IsReplaceValid()
    {
      return IsValid() && replaceItem.IsValid(); 
    }

    public override string GetDescription()
    {
      return objID.obj.name;
    }

    public override bool hasAdvancedOptions()
    {
        return false;
    }

    public override bool canSubsearch()
    {
      return false;
    }

    public override bool caresAboutAsset(SearchJob job)
    {
      if(job.scope.projectScope == ProjectScope.CurrentSelection)
      {
        return true;
      }
      if(job.scope.projectScope == ProjectScope.SpecificLocation)
      {
        if(job.scope.scopeObj.isDirectory)
        {
          return true;
        }
        if(job.scope.scopeObj.obj == null)
        {
          return false;
        }
        return job.scope.scopeObj.obj is GameObject;
      }

      if((job.assetData.assetScope & AssetScope.Scenes) == AssetScope.Scenes)
      {
        //ok, we're searching scenes or prefab stages....
        return job.assetData.assetExtension == ".unity" || job.assetData.assetExtension == ".prefab";
      }

      return false;
    }

  }
}