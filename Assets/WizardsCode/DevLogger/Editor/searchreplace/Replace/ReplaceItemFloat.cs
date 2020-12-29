using UnityEngine;
using UnityEditor;
using System.IO;

namespace sr
{
  [System.Serializable]
  public class ReplaceItemFloat : ReplaceItem<DynamicTypeFloat, float>
  {

    protected override float drawEditor()
    {
      return EditorGUILayout.FloatField(Keys.Replace, replaceValue);
    }

    protected override void replace(SearchJob job, SerializedProperty prop, SearchResult result)
    {
      prop.floatValue = replaceValue;
      result.replaceStrRep = replaceValue.ToString();
    }

  }

}