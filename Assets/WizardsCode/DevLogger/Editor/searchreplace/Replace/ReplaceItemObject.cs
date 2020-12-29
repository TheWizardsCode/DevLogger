using UnityEngine;
using UnityEditor;
using System.IO;
using System;

namespace sr
{
  [System.Serializable]
  public class ReplaceItemObject : ReplaceItem<DynamicTypeObject, ObjectID> 
  {

    public override void Draw()
    {
      GUILayout.BeginHorizontal();
      OnDTDUpdate();
      Type fieldType = type != null ? type : typeof(UnityEngine.Object);

      GUILayout.Label(Keys.Replace, GUILayout.Width(SRWindow.compactLabelWidthNP));
      UnityEngine.Object newObj = EditorGUILayout.ObjectField( replaceValue.obj, fieldType, true);

      if(newObj != null && !fieldType.IsAssignableFrom(newObj.GetType()))
      {
        // Debug.Log("[ReplaceItemObject] nulling out!"+newObj.GetType() + " : " +fieldType );
        newObj = null;
      }
      if(replaceValue.obj != newObj)
      {
        SetObject(newObj);
        SRWindow.Instance.PersistCurrentSearch();
      }
      drawSwap();
      GUILayout.EndHorizontal();
    }

    public void SetObject(UnityEngine.Object obj)
    {
        replaceValue.SetObject(obj);
    }

    // Fixes nulls in serialization manually...*sigh*.
    public override void OnDeserialization()
    {
      if(replaceValue == null)
      {
        replaceValue = new ObjectID();
      }
      replaceValue.OnDeserialization();
    }

    public override void OnDTDUpdate()
    {
      type = dtd.parent.type;
    }

    protected override void Swap() 
    {
        UnityEngine.Object tmp = parent.searchValue.obj;
        parent.searchValue.SetObject(replaceValue.obj);
        replaceValue.SetObject(tmp);
        SRWindow.Instance.PersistCurrentSearch();
    }

    protected override void replace(SearchJob job, SerializedProperty prop, SearchResult result)
    {
      if(prop.name == "m_Script")
      {
        if(replaceValue.obj == null)
        {
          //don't do it.
          result.actionTaken = SearchAction.Error;
          result.replaceStrRep = "(null)";
          result.error = "Cannot set a MonoBehaviour's Script to null.";
          return;
        }
      }
      prop.objectReferenceValue = replaceValue.obj;
      string objName = "(null)";
      if(replaceValue.obj != null)
      {
        objName = replaceValue.obj.name;
      }
      result.replaceStrRep = objName;
    }


  } // End Class
} // End Namespace