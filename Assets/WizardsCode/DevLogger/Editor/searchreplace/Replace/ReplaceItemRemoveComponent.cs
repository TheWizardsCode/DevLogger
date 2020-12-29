using UnityEngine;
using UnityEditor;
using System.IO;
using System;

namespace sr
{
  [System.Serializable]
  public class ReplaceItemRemoveComponent : ReplaceItem
  {

    public override void Draw()
    {
      GUILayout.Label("Searching and replacing will delete the monobehaviour from the game object.");
    }

    public override void OnDeserialization()
    {
      popupLabel = "Remove Component";
    }

    protected override void replace(SearchJob job, SerializedProperty prop, SearchResult result)
    {
      Component component = (Component)prop.serializedObject.targetObject;
      result.replaceStrRep = component.GetType().ToString();
      result.actionTaken = SearchAction.ObjectRemoved;
      result.pathInfo = PathInfo.GetPathInfo(component.gameObject, job.assetData);
      UnityEngine.Object.DestroyImmediate(component, true);
    }
  } // End Class
} // End Namespace