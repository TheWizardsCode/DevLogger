using UnityEngine;
using UnityEditor;
using System;
using System.Collections;
using System.Collections.Generic;

namespace sr
{
  /**
   * Provides a search field for Rects.
   */
  [System.Serializable]
  public class DynamicTypeRect : DynamicTypeData<RectSerializable>
  {
    public static List<Type> replaceOptionTypes = new List<Type>()
    {
      typeof(ReplaceItemRect),  
    };

    [System.NonSerialized]
    public Rect val;

    public OptionalFloatField xField;
    public OptionalFloatField yField;
    public OptionalFloatField wField;
    public OptionalFloatField hField;
    bool approximate = true;

    public override void Draw(SearchOptions options)
    {
      this.options = options;

      GUILayout.BeginHorizontal();
      drawControlStart();
      GUILayout.BeginHorizontal();
      SRWindow.Instance.CVS();
      bool enabled = !anyValueEquals();

      if(!SRWindow.Instance.Compact())
      {
        GUILayout.BeginVertical();
        GUILayout.BeginHorizontal();
      }

      xField.Draw(showMoreOptions, enabled);
      yField.Draw(showMoreOptions, enabled);

      if(!SRWindow.Instance.Compact())
      {
        GUILayout.EndHorizontal();
        GUILayout.BeginHorizontal();
      }

      wField.Draw(showMoreOptions, enabled);
      hField.Draw(showMoreOptions, enabled);

      if(!SRWindow.Instance.Compact())
      {
        GUILayout.EndHorizontal();
        GUILayout.BeginHorizontal();
      }

      if(xField.updated || yField.updated || wField.updated || hField.updated )
      {
        val = new Rect(xField.fieldValue, yField.fieldValue, wField.fieldValue, hField.fieldValue);
        searchValue = RectSerializable.FromRect(val);
        SRWindow.Instance.PersistCurrentSearch();
      }
      if(showMoreOptions)
      {
        float lw = EditorGUIUtility.labelWidth;
        EditorGUIUtility.labelWidth = SRWindow.approxLabelWidth;
        bool newApproximate = EditorGUILayout.Toggle("Approximate?", approximate, GUILayout.Width(SRWindow.approxWidth - 4)); // -4 for style padding/margin
        EditorGUIUtility.labelWidth = lw; // i love stateful gui! :(
        if(newApproximate != approximate)
        {
          approximate = newApproximate;
          SRWindow.Instance.PersistCurrentSearch();
        }
      }

      if(!SRWindow.Instance.Compact())
      {
        GUILayout.EndHorizontal();
        GUILayout.EndVertical();
      }
      GUILayout.EndHorizontal();
      GUILayout.EndHorizontal();

      SRWindow.Instance.CVE();
      drawControlEnd();
      drawReplaceItem(options);
    }

    public override void OnDeserialization()
    {
      initReplaceOptions(replaceOptionTypes);

      val = searchValue.ToRect();
      if(xField == null)
      {
        xField = new OptionalFloatField("X", val.x);
      }
      if(yField == null )
      {
        yField = new OptionalFloatField("Y", val.y);
      }
      if(wField == null )
      {
        wField = new OptionalFloatField("W", val.width);
      }
      if(hField == null )
      {
        hField = new OptionalFloatField("H", val.height);
      }
    }

    public override bool ValueEquals(SerializedProperty prop)
    {
      Rect v4 = prop.rectValue;
      return xField.ValueEquals(v4.x, approximate) && yField.ValueEquals(v4.y, approximate) && wField.ValueEquals(v4.width, approximate) && hField.ValueEquals(v4.height, approximate);

      // Debug.Log("[DynamicTypeRect] prop:"+prop.propertyPath);
      // return prop.rectValue == val;
    }

    public override bool IsValid()
    {
      if(showMoreOptions)
      {
        return true;
      }
      return xField.selected || yField.selected || wField.selected || hField.selected;
    }

    public override string StringValue()
    {
      return "(x:"+xField.StringValue() + ", y:"+yField.StringValue()+", width:"+wField.StringValue()+", height:"+hField.StringValue()+")";
    }

    public override string StringValueFor(SerializedProperty prop)
    {
      return prop.rectValue.ToString();
    }

    public override SerializedPropertyType PropertyType()
    {
      return SerializedPropertyType.Rect;
    }

    public override bool _hasAdvancedOptions()
    {
      return true;
    }

    protected override void initializeDefaultValue(SerializedProperty prop)
    {
      Rect r = prop.rectValue;
      xField.fieldValue = r.x;
      yField.fieldValue = r.y;
      wField.fieldValue = r.width;
      hField.fieldValue = r.height;
    }
  }
}
