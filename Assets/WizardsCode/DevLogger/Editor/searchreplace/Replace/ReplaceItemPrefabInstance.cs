using UnityEngine;
using UnityEditor;
using System.IO;
using System;
using System.Collections;
using System.Collections.Generic;

namespace sr
{
  [System.Serializable]
  public class ReplaceItemPrefabInstance : ReplaceItem
  {
    public ObjectID objID;

    public bool rename;
    public bool updateTransform = true;


    [System.NonSerialized]
    bool searchValid = false;

    [System.NonSerialized]
    public SearchItemInstances parentSearchItem;

    public override void Draw()
    {
      GUILayout.BeginVertical();

      GUILayout.BeginHorizontal();

      float lw = EditorGUIUtility.labelWidth;
      EditorGUIUtility.labelWidth = SRWindow.compactLabelWidth;
      UnityEngine.Object newObj = EditorGUILayout.ObjectField(Keys.Replace, objID.obj, typeof(UnityEngine.Object), true);
      EditorGUIUtility.labelWidth = lw; // i love stateful gui! :(

      if(objID.obj != newObj)
      { 
        newObj = PrefabUtil.getPrefabRoot(newObj);
        objID.SetObject(newObj);
        searchValid = parentSearchItem.validateSearch(objID.obj);
        SRWindow.Instance.PersistCurrentSearch();
      }
      if(GUILayout.Button(GUIContent.none, SRWindow.swapToggle))
      {
        parentSearchItem.Swap();
      }
      GUILayout.EndHorizontal();
      parentSearchItem.DrawValidSearch(searchValid, objID.obj);
      GUILayout.EndVertical();

      GUILayout.BeginHorizontal();
      GUILayout.Space(SRWindow.compactLabelWidth);
      GUILayout.BeginVertical();
      bool newRename = EditorGUILayout.Toggle("Keep Name", rename);
      {
        if(newRename != rename)
        {
          rename = newRename;
          SRWindow.Instance.PersistCurrentSearch();
        }
      }
      bool newUpdateTransform = EditorGUILayout.Toggle("Keep Transform Values", updateTransform);
      {
        if(newUpdateTransform != updateTransform)
        {
          updateTransform = newUpdateTransform;
          SRWindow.Instance.PersistCurrentSearch();
        }
      }

      GUILayout.EndVertical();
      GUILayout.EndHorizontal();
    }

    public override bool IsValid()
    {
      return searchValid;
    }

    // Fixes nulls in serialization manually...*sigh*.
    public override void OnDeserialization()
    {
      if(objID == null)
      {
        objID = new ObjectID();
        updateTransform = true;
        rename = true;
      }
      objID.OnDeserialization();
      searchValid = parentSearchItem.validateSearch(objID.obj);
    }


    public void ReplaceInstance(SearchJob job, SearchItem item, GameObject gameObjToSwap, SearchResult result)
    {
      if(job.options.searchType == SearchType.SearchAndReplace)
      {  
        if(PrefabUtil.SwapPrefab(job, item, result, gameObjToSwap, objID.GetGameObject(), updateTransform, rename))
        {
          result.actionTaken = SearchAction.InstanceReplaced;
        }
      }
    }

  } // End Class
} // End Namespace
