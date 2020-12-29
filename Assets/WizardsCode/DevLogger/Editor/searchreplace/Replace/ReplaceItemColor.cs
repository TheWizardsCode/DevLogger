using UnityEngine;
using UnityEditor;
using System.IO;

namespace sr
{
  [System.Serializable]
  public class ReplaceItemColor : ReplaceItem<DynamicTypeColor, ColorSerializable>
  {
    // [System.NonSerialized]
    // public Color replaceValue;
    // ColorSerializable valSerialized;


    public override void Draw()
    {
      GUILayout.BeginHorizontal();

      float lw = EditorGUIUtility.labelWidth;
      EditorGUIUtility.labelWidth = SRWindow.compactLabelWidth;
      Color replaceColor = replaceValue.ToColor();
      Color newValue = EditorGUILayout.ColorField(Keys.Replace, replaceColor);
      EditorGUIUtility.labelWidth = lw; // i love stateful gui! :(

      if(replaceColor != newValue)
      {
        replaceValue = ColorSerializable.FromColor(newValue);
        SRWindow.Instance.PersistCurrentSearch();
      }
      drawSwap();
      GUILayout.EndHorizontal();
    }

    public override void OnDeserialization()
    {
      if(replaceValue == null)
      {
        replaceValue = ColorSerializable.FromColor(Color.black);
      }
    }
  
  protected override void Swap()
  { 
    ColorSerializable tmp = parent.searchValue; 
    parent.searchValue = replaceValue; 
    replaceValue = new ColorSerializable(tmp);
    parent.UpdateColor();
    SRWindow.Instance.PersistCurrentSearch();
  }

    protected override void replace(SearchJob job, SerializedProperty prop, SearchResult result)
    {
      prop.colorValue = replaceValue.ToColor(); 
      result.replaceStrRep =prop.colorValue.ToString();
    }
  }
 


}