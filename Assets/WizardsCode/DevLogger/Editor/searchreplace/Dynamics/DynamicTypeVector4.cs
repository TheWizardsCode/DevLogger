using UnityEngine;
using UnityEditor;
using System;
using System.Collections;
using System.Collections.Generic;

namespace sr
{
  /**
   * Provides a search field for Vector4.
   */
  [System.Serializable]
  public class DynamicTypeVector4 : DynamicTypeData<Vector4Serializable>
  {

    public static List<Type> replaceOptionTypes = new List<Type>()
    {
      typeof(ReplaceItemVector4),  
    };

    [System.NonSerialized]
    public Vector4 val;

    public OptionalFloatField xField;
    public OptionalFloatField yField;
    public OptionalFloatField zField;
    public OptionalFloatField wField;
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

      zField.Draw(showMoreOptions, enabled);
      wField.Draw(showMoreOptions, enabled);

      if(!SRWindow.Instance.Compact())
      {
        GUILayout.EndHorizontal();
        GUILayout.BeginHorizontal();
      }

      if(xField.updated || yField.updated || zField.updated || wField.updated)
      {
        val = new Vector4(xField.fieldValue, yField.fieldValue, zField.fieldValue, wField.fieldValue);
        searchValue = Vector4Serializable.FromVector4(val);
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
      
      val = searchValue.ToVector4();


      if(xField == null)
      {
        xField = new OptionalFloatField("X", val.x);
      }
      if(yField == null )
      {
        yField = new OptionalFloatField("Y", val.y);
      }
      if(zField == null )
      {
        zField = new OptionalFloatField("Z", val.z);
      }
      if(wField == null )
      {
        wField = new OptionalFloatField("W", val.w);
      }

    }

    public override bool ValueEquals(SerializedProperty prop)
    {
      Vector4 v4 = prop.vector4Value;
      return xField.ValueEquals(v4.x, approximate) && yField.ValueEquals(v4.y, approximate) && zField.ValueEquals(v4.z, approximate) && wField.ValueEquals(v4.w, approximate);
    }

    public override bool IsValid()
    {
      if(showMoreOptions)
      {
        return true;
      }
      return xField.selected || yField.selected || zField.selected || wField.selected;
    }

    public override string StringValue()
    {
      return "("+xField.StringValue() + ","+yField.StringValue()+","+zField.StringValue()+","+wField.StringValue()+")";
    }

    public override string StringValueFor(SerializedProperty prop)
    {
      return prop.vector4Value.ToString();
    }

    public override SerializedPropertyType PropertyType()
    {
      return SerializedPropertyType.Vector4;
    }

    public override bool _hasAdvancedOptions()
    {
      return true;
    } 

    protected override void initializeDefaultValue(SerializedProperty prop)
    {
      Vector4 v4 = prop.vector4Value;
      xField.fieldValue = v4.x;
      yField.fieldValue = v4.y;
      zField.fieldValue = v4.z;
      wField.fieldValue = v4.w;
    }
  }
}
