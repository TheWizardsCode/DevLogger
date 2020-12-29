using UnityEngine;
using UnityEditor;

namespace sr
{
  /**
   * This class used to be used more but now is only used once in SRWindow. 
   * On the chopping block for refactor?
   */
  public class PopupData
  {
    protected int index = 0;
    protected string[] options;
    protected string key;
    protected string label;
    public float labelWidth;
    public float boxPad = SRWindow.boxPad;

    public PopupData(int i, string[] o, string k, string l)
    {
      options = o;
      key = k;
      index = EditorPrefs.GetInt(key, i);
      label = l;
    }

    public void Draw()
    {
      float lw = EditorGUIUtility.labelWidth;
      EditorGUIUtility.labelWidth = labelWidth;
      int newIndex = EditorGUILayout.Popup(label, index, options, GUILayout.MaxWidth(SRWindow.Instance.position.width - boxPad));
      EditorGUIUtility.labelWidth = lw; // i love stateful gui! :(

      if(newIndex != index)
      {
        index = newIndex;
        EditorPrefs.SetInt(key, index);
      }
    }

    public string Value()
    {
      return options[index];
    }
  }
}