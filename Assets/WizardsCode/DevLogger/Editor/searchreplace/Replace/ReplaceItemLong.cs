using UnityEngine;
using UnityEditor;
using System.IO;

namespace sr
{
  [System.Serializable]
  public class ReplaceItemLong : ReplaceItem<DynamicTypeLong, long>
  {

    protected override long drawEditor()
    {
      return EditorGUILayout.LongField(Keys.Replace, replaceValue);
    }

    protected override void replace(SearchJob job, SerializedProperty prop, SearchResult result)
    {
      prop.longValue = replaceValue;
      result.replaceStrRep = replaceValue.ToString();
    }
  }

}