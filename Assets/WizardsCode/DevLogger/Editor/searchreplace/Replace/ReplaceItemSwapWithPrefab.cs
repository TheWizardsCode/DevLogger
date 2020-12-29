using UnityEngine;
using UnityEditor;
using System.IO;
using System;
#if UNITY_2018_3_OR_NEWER
using UnityEditor.Experimental.SceneManagement;
using UnityEditor.SceneManagement;
#endif

namespace sr
{ 
  [System.Serializable]
  public class ReplaceItemSwapWithPrefab : ReplaceItem
  {
    public ObjectID objID;

    public bool rename;
    public bool updateTransform;

    public override void Draw()
    {
      GUILayout.BeginHorizontal();
      Type fieldType = type != null ? type : typeof(UnityEngine.Object);

      GUILayout.Label(Keys.Replace, GUILayout.Width(SRWindow.compactLabelWidthNP));
      UnityEngine.Object newObj = EditorGUILayout.ObjectField( objID.obj, fieldType, true);

      if(newObj != null)
      {
        if(!fieldType.IsAssignableFrom(newObj.GetType()))
        {
          // Debug.Log("[ReplaceItemObject] nulling out!"+newObj.GetType() + " : " +fieldType );
          newObj = null;
        }else
        if(!PrefabUtil.isPrefab(newObj))
        {
          Debug.Log("[Search & Replace] "+ newObj.name + " is not a prefab.");
          newObj = null;
        }
      }
      if(objID.obj != newObj)
      {
        objID.SetObject(newObj);
        SRWindow.Instance.PersistCurrentSearch();
      }
      drawSwap();
      GUILayout.EndHorizontal();
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
      return objID.obj != null;
    }

    // Fixes nulls in serialization manually...*sigh*.
    public override void OnDeserialization()
    {
      popupLabel = "Swap With Prefab";
      if(objID == null)
      {
        objID = new ObjectID();
      }
      objID.OnDeserialization();
    }

    protected override void replace(SearchJob job, SerializedProperty prop, SearchResult result)
    {
      UnityEngine.Object objToSwap = prop.serializedObject.targetObject;
      ObjectID swapObjID = new ObjectID(objToSwap);
      GameObject gameObjToSwap = swapObjID.GetGameObject();
      if(gameObjToSwap != null)
      {
        if(PrefabUtil.SwapPrefab(job, dtd.parent.searchItem, result, gameObjToSwap, objID.GetGameObject(), updateTransform, rename))
        {
          result.actionTaken = SearchAction.ObjectSwapped;
          result.replaceStrRep = objID.GetGameObject().name;
        }
      }
      else
      {
        result.actionTaken = SearchAction.Ignored;
      }
    }


  } // End Class
} // End Namespace