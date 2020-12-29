
using UnityEngine;
using UnityEditor;
using System.IO;

namespace sr
{
  [System.Serializable]
  public class ReplaceItemChar : ReplaceItem<DynamicTypeChar, char>
  {

    public override void Draw()
    {
      GUILayout.BeginHorizontal();
      float lw = EditorGUIUtility.labelWidth;
      EditorGUIUtility.labelWidth = SRWindow.compactLabelWidth;
      
      string strValue = EditorGUILayout.TextField(Keys.Replace, replaceValue.ToString());
      EditorGUIUtility.labelWidth = lw; // i love stateful gui! :(

      char newValue = replaceValue;
      if(strValue.Length > 0)
      {
        newValue = System.Convert.ToChar(strValue[0]);
      }
      if(replaceValue != newValue)
      {
        replaceValue = newValue;
        SRWindow.Instance.PersistCurrentSearch();
      }
      drawSwap();
      GUILayout.EndHorizontal();
    }

    protected override void replace(SearchJob job, SerializedProperty prop, SearchResult result)
    {
      prop.intValue = replaceValue;
      result.replaceStrRep = replaceValue.ToString();
    }
  }

}