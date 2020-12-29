using UnityEngine;
using UnityEditor;
using System;
using System.Collections;
using System.Collections.Generic;

namespace sr
{
  /**
   * Provides a search field for color.
   */
  [System.Serializable]
  public class DynamicTypeColor : DynamicTypeData<ColorSerializable>
  {

    public static List<Type> replaceOptionTypes = new List<Type>()
    {
      typeof(ReplaceItemColor),  
    };

    [System.NonSerialized]
    Color searchColor;

    public override void Draw(SearchOptions options)
    {
      drawControlStart();
      Color newValue = EditorGUILayout.ColorField(searchColor);
      drawControlEnd();
      if(newValue != searchColor)
      {
        searchValue = ColorSerializable.FromColor(newValue);
        UpdateColor();
        SRWindow.Instance.PersistCurrentSearch();
      }

      drawReplaceItem(options);
    }

    public override void OnDeserialization()
    {
      initReplaceOptions(replaceOptionTypes);
      if(searchValue == null)
      {
        searchValue = ColorSerializable.FromColor(Color.white);
      }
      UpdateColor();
    }

    public override bool ValueEquals(SerializedProperty prop)
    {
      Color c = prop.colorValue;
      return Approximately(c.r, searchColor.r) && Approximately(c.g, searchColor.g) && Approximately(c.b, searchColor.b) && Approximately(c.a, searchColor.a);
    }

    bool Approximately(float a, float b)
    {
      return Mathf.Abs(a-b) < 0.00392f;  // 1 / 255
    }

    public override bool IsValid()
    {
      return true;
    }

    public override string StringValue()
    {
      return searchColor.ToString();
    }

    public override string StringValueFor(SerializedProperty prop)
    {
      return prop.colorValue.ToString();
    }

    public override SerializedPropertyType PropertyType()
    {
      return SerializedPropertyType.Color;
    }

    public override bool _hasAdvancedOptions()
    {
      return true;
    }

    public void UpdateColor()
    {
      searchColor = searchValue.ToColor();
    }

    protected override void initializeDefaultValue(SerializedProperty prop)
    {
      searchColor = prop.colorValue;
      searchValue = ColorSerializable.FromColor(searchColor);
    } 

  }
}
