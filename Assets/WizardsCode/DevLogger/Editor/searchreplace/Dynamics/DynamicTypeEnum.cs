using UnityEngine;
using UnityEditor;
using System;
using System.Collections;
using System.Collections.Generic;

namespace sr
{

  /**
   * Provides a search field for enums.
   */
  [System.Serializable]
  public class DynamicTypeEnum : DynamicTypeData<int>
  {
    public static List<Type> replaceOptionTypes = new List<Type>()
    {
      typeof(ReplaceItemEnum),  
    };

    string[] names;

    public override void Draw(SearchOptions options)
    {
      // Debug.Log("[DynamicTypeEnum] type:"+parent.type);
      if(!parent.type.IsEnum)
      {
        GUILayout.Label("Type is not an enum.");
        return;
      }
        names = System.Enum.GetNames(parent.type);
      // Debug.Log("[DynamicTypeEnum] parent.type:"+parent.type);
      drawControlStart();
      int newValue = EditorGUILayout.Popup(searchValue, names);
      drawControlEnd();

      if(newValue != searchValue)
      {
        searchValue = newValue;
        SRWindow.Instance.PersistCurrentSearch();
      }
      ReplaceItemEnum replaceEnum = (ReplaceItemEnum)replaceItem;
      replaceEnum.names = names;
      replaceEnum.type = parent.type;
      drawReplaceItem(options);
    }

    public override void OnDeserialization()
    {
      initReplaceOptions(replaceOptionTypes);
    }

    public override bool ValueEquals(SerializedProperty prop)
    {
      return intValue(prop) == searchValue;
    }

    public override bool IsValid()
    {
      return true;
    }

    public override string StringValue()
    {
      if(names == null)
      {
        return "";
      }
      return nameForInt(searchValue);
    }

    public override string StringValueFor(SerializedProperty prop)
    {
      return nameForInt(intValue(prop));
    }


    public override bool _hasAdvancedOptions()
    {
      return true;
    }


    public override SerializedPropertyType PropertyType()
    {
      return SerializedPropertyType.Enum;
    }

    protected override void initializeDefaultValue(SerializedProperty prop)
    {
      searchValue = intValue(prop);
    }

    int intValue(SerializedProperty prop)
    {
      if(prop.propertyType == SerializedPropertyType.Enum)
      {
        return prop.enumValueIndex;
      }else{
        return prop.intValue;
      }
    }

    string nameForInt(int val)
    {
      if(val >= names.Length)
      {
        return "<out of range>";
      }
      return names[val];
    }

  }
}
