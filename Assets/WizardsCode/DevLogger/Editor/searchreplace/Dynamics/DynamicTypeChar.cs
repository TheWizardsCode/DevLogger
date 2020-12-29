using UnityEngine;
using UnityEditor;
using System;
using System.Collections;
using System.Collections.Generic;

namespace sr
{
  /**
   * Provides a search field for the char data type.
   */
  [System.Serializable]
  public class DynamicTypeChar : DynamicTypeData<char>
  {

    public static List<Type> replaceOptionTypes = new List<Type>()
    {
      typeof(ReplaceItemChar),  
    };

    public override void Draw(SearchOptions options)
    {
      drawControlStart();
      string strValue = EditorGUILayout.TextField(searchValue.ToString());
      drawControlEnd();
      char newValue = searchValue;
      if(strValue.Length > 0)
      {
        newValue = System.Convert.ToChar(strValue[0]);
      }
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
      return ((char)prop.intValue).ToString();
    }


    public override SerializedPropertyType PropertyType()
    {
      return SerializedPropertyType.Character;
    }

    protected override void initializeDefaultValue(SerializedProperty prop)
    {
      searchValue = (char)prop.intValue;
    }

    public override bool _hasAdvancedOptions()
    {
      return true;
    }

  }
}
