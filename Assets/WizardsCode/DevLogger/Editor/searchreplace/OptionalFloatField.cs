using UnityEngine;
using UnityEditor;

namespace sr
{
  /**
   * Many types are composed of floats that the user may optionally include in
   * the search. For example a user may want to search just for an 'x' value in
   * a vector 3, ignoring y and z. This class encapsulates the functionality of
   * a float and is used in many DynamicTypeData classes.
   */
  [System.Serializable]
  public class OptionalFloatField
  {
    protected string label;
    public bool selected = true;
    public float fieldValue;
    public bool updated = false;

    bool advanced = false;

    public OptionalFloatField(string l, float v)
    {
      label = l;
      fieldValue = v;
      selected = true;
    }

    public void Draw(bool showMoreOptions, bool enabled=true)
    {
      GUILayout.BeginHorizontal();
      updated = false;
      advanced = showMoreOptions;
      if(advanced)
      {
        GUI.enabled = enabled && selected;
      }
      GUILayout.Label(label, GUILayout.Width(14));
      float newValue = EditorGUILayout.FloatField(fieldValue, EditorStyles.numberField);
      if(newValue != fieldValue)
      {
        fieldValue = newValue;
        updated = true;
      }
      if(enabled)
      {
        GUI.enabled = true;
      }
      if(advanced)
      {
        bool newSelected = EditorGUILayout.Toggle(selected, GUILayout.Width(30));
        if(newSelected != selected)
        {
          selected = newSelected;
          SRWindow.Instance.PersistCurrentSearch();
        }
      } 
      GUILayout.EndHorizontal();

    }

    public string StringValue()
    {
      if(!advanced)
      {
        return fieldValue.ToString();
      }
      if(selected)
      {
        return fieldValue.ToString();
      }else{
        return "any";
      }
    }

    public bool ValueEquals(float val, bool approximate)
    {
      if(!advanced)
      {
        return val == fieldValue;
      }
      if(!selected)
      {
        return true;
      }
      if(approximate)
      {
        return Mathf.Approximately(val, fieldValue);
      }else{
        return val == fieldValue;
      }
    }

    public float Replace(float currentValue)
    {
      if(!advanced)
      {
        return fieldValue;
      }
      if(selected)
      {
        return fieldValue;
      }
      return currentValue;
    }

    public void Swap(OptionalFloatField fieldB)
    {
      bool tmpSelected = selected;
      float tmpValue = fieldValue;
      
      selected = fieldB.selected;
      fieldValue = fieldB.fieldValue;

      fieldB.selected = tmpSelected;
      fieldB.fieldValue = tmpValue;

      fieldB.updated = true;
      updated = true;
      SRWindow.Instance.PersistCurrentSearch();

    }

  }
}