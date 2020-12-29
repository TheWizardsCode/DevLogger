using UnityEngine;
using UnityEditor;
using System;
using System.Collections;
using System.Collections.Generic;

namespace sr
{
  /**
   * Provides a search field for floats.
   */
  [System.Serializable]
  public class DynamicTypeFloat : DynamicTypeData<float>
  {
    bool approximate = false;

    public static List<Type> replaceOptionTypes = new List<Type>()
    {
      typeof(ReplaceItemFloat),  
    };


    public override void Draw(SearchOptions options)
    {
      GUILayout.BeginHorizontal();
      SRWindow.Instance.CVS();

      drawControlStart();
      float newValue = EditorGUILayout.FloatField(searchValue);
      GUILayout.EndHorizontal();
      if(newValue != searchValue)
      {
        searchValue = newValue;
        SRWindow.Instance.PersistCurrentSearch();
      }

      if(showMoreOptions)
      {
        if(SRWindow.Instance.Compact())
        {
          GUILayout.BeginHorizontal();
          GUILayout.Space(SRWindow.compactLabelWidthEP);
        }

        float lw = SRWindow.approxLabelWidth;
        EditorGUIUtility.labelWidth = SRWindow.approxLabelWidth;
        bool newApproximate = EditorGUILayout.Toggle("Approximate?", approximate, GUILayout.Width(SRWindow.approxWidth - 4)); // -4 for style padding/margin
        EditorGUIUtility.labelWidth = lw; // i love stateful gui! :(
        if(newApproximate != approximate)
        { 
          approximate = newApproximate;
          SRWindow.Instance.PersistCurrentSearch();
        }
        if(SRWindow.Instance.Compact())
        {
          GUILayout.EndHorizontal();
        }
      }
      GUI.enabled = true;

      SRWindow.Instance.CVE();
      GUILayout.EndHorizontal();

      drawReplaceItem(options);
    }

    public override void OnDeserialization()
    {
      initReplaceOptions(replaceOptionTypes);
    }

    public override bool ValueEquals(SerializedProperty prop)
    {
      if(showMoreOptions && approximate)
      {
        return Mathf.Approximately (prop.floatValue, searchValue);
      }else{
        return prop.floatValue == searchValue;
      }
    }

    public override bool IsValid()
    {
      return true;
    }

    public override string StringValue()
    {
      return searchValue.ToString();
    }

    public override string StringValueFor(SerializedProperty prop)
    {
      return prop.floatValue.ToString();
    }

    public override SerializedPropertyType PropertyType()
    {
      return SerializedPropertyType.Float;
    }

    public override bool _hasAdvancedOptions()
    {
      return true;
    }

    protected override void initializeDefaultValue(SerializedProperty prop)
    {
      searchValue = prop.floatValue;
    }
  }
}
