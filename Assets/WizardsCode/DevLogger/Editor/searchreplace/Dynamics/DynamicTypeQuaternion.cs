using UnityEngine;
using UnityEditor;
using System;
using System.Collections;
using System.Collections.Generic;

namespace sr
{
  /**
   * Provides a search field for Quaternions.
   */
  [System.Serializable]
  public class DynamicTypeQuaternion : DynamicTypeData<Vector4Serializable>
  {
    public static List<Type> replaceOptionTypes = new List<Type>()
    {
      typeof(ReplaceItemQuaternion),  
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
      Quaternion q2 = prop.quaternionValue;
      return xField.ValueEquals(q2.x, approximate) && yField.ValueEquals(q2.y, approximate) && zField.ValueEquals(q2.z, approximate) && wField.ValueEquals(q2.w, approximate);
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
      return prop.quaternionValue.ToString();
    }

    public override SerializedPropertyType PropertyType()
    {
      return SerializedPropertyType.Quaternion;
    }

    public override bool _hasAdvancedOptions()
    {
      return true;
    }

    protected override void initializeDefaultValue(SerializedProperty prop)
    {
      Quaternion q = prop.quaternionValue;
      wField.fieldValue = q.w;
      xField.fieldValue = q.x;
      yField.fieldValue = q.y;
      zField.fieldValue = q.z;
    }
  }
}
