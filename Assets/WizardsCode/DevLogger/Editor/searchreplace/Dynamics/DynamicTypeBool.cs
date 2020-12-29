using UnityEngine;
using UnityEditor;
using System;
using System.Collections;
using System.Collections.Generic;

namespace sr
{
  /**
   * Provides a search field for booleans.
   */
  [System.Serializable]
  public class DynamicTypeBool : DynamicTypeData<bool>
  {

    public static List<Type> replaceOptionTypes = new List<Type>()
    {
      typeof(ReplaceItemBool),  
    };


    public override void Draw(SearchOptions options)
    {
      drawControlStart();
      bool newValue = EditorGUILayout.Toggle(searchValue);
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
      return prop.boolValue == searchValue;
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
      return prop.boolValue.ToString();
    }

    public override SerializedPropertyType PropertyType()
    {
      return SerializedPropertyType.Boolean;
    }

    protected override void initializeDefaultValue(SerializedProperty prop)
    {
      searchValue = prop.boolValue;
    }

    public override bool _hasAdvancedOptions()
    {
      return true;
    }
  }
}
