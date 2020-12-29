using UnityEngine;
using UnityEditor;
using System.IO;
using System;

namespace sr
{
  [System.Serializable]
  public class ReplaceItemWithReference : ReplaceItem<DynamicTypeObject, ObjectID> 
  {

    public override void Draw()
    {
      GUILayout.BeginHorizontal();
      GUILayout.Width(SRWindow.compactLabelWidthNP);
      type = dtd.parent.type;
      if(IsValid())
      {
        GUILayout.Label("Replacing with the "+type+" found on the game object.");
      }
      else
      {
        EditorGUILayout.HelpBox("You can only replace components via GetComponent. \nType:" + type, MessageType.Warning);
      }
      GUILayout.EndHorizontal();
    }

    // Fixes nulls in serialization manually...*sigh*.
    public override void OnDeserialization()
    { 
      popupLabel = "Replace With GetComponent()";

    }

    public override void OnDTDUpdate()
    {
      type = dtd.parent.type;
    }

    public override bool IsValid()
    {
      return typeof(Component).IsAssignableFrom(dtd.parent.type);
    }

    protected override void Swap() 
    {
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
      if(typeof(Component).IsAssignableFrom(type))
      {
        GameObject go = ObjectUtil.GetGameObject(prop.serializedObject.targetObject);

        UnityEngine.Object replaceVal = go.GetComponent(type);
        prop.objectReferenceValue = replaceVal;
        // prop.objectReferenceValue = replaceValue.obj;
        string objName = replaceVal == null ? "(null)" : replaceVal.ToString();
        result.replaceStrRep = objName;
      }
    }


  } // End Class
} // End Namespace