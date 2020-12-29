using UnityEngine;
using UnityEditor;
using System;
using System.Collections;
using System.Collections.Generic;

namespace sr
{
  /**
   * Provides a search field for ints.
   */
  [System.Serializable]
  public class DynamicTypeInt : DynamicTypeData<int>
  {

    public static List<Type> replaceOptionTypes = new List<Type>()
    {
      typeof(ReplaceItemInt),  
    };

    public override void Draw(SearchOptions options)
    {
      drawControlStart();
      int newValue = EditorGUILayout.IntField(searchValue);
      drawControlEnd();
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
      return prop.intValue == searchValue;
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
      return prop.intValue.ToString();
    }

    public override SerializedPropertyType PropertyType()
    {
      return SerializedPropertyType.Integer;
    }

    public override bool _hasAdvancedOptions()
    {
      return true;
    }

    protected override void initializeDefaultValue(SerializedProperty prop)
    {
      searchValue = prop.intValue;
    }

  }
}
