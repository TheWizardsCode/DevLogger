using UnityEngine;
using UnityEditor;
using System;
using System.Collections;
using System.Collections.Generic;
namespace sr
{

  /**
   * Provides a search field for doubles.
   */
  [System.Serializable]
  public class DynamicTypeDouble : DynamicTypeData<double>
  {

    public static List<Type> replaceOptionTypes = new List<Type>()
    {
      typeof(ReplaceItemDouble),  
    };

    public override void Draw(SearchOptions options)
    {
      GUILayout.BeginHorizontal();
      if(showMoreOptions)
      {
        drawConditionalControl(SRWindow.compactLabelWidthNP);
      }else{
        GUILayout.Label("Value:", GUILayout.Width(SRWindow.compactLabelWidthNP)); //minus element padding when manually creating control groups.
      }
      GUI.enabled = !anyValueEquals();
      
      double newValue = EditorGUILayout.DoubleField(searchValue);
      GUI.enabled = true;
      GUILayout.EndHorizontal();
      if(newValue != searchValue)
      {
        searchValue = newValue;
        SRWindow.Instance.PersistCurrentSearch();
      }
      drawReplaceItem(options);
    }

    public override void OnDeserialization()
    {
      initReplaceOptions(replaceOptionTypes);
    }

    public override bool ValueEquals(SerializedProperty prop)
    {
      return prop.doubleValue == searchValue;
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
      return prop.doubleValue.ToString();
    }


    public override SerializedPropertyType PropertyType()
    {
      return SerializedPropertyType.Float;
    }

    protected override void initializeDefaultValue(SerializedProperty prop)
    {
      searchValue = prop.doubleValue;
    }

    public override bool _hasAdvancedOptions()
    {
      return true;
    }

  }
}
