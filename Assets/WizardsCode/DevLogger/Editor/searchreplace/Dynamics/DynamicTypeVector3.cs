using UnityEngine;
using UnityEditor;
using System;
using System.Collections;
using System.Collections.Generic;

namespace sr
{
  /**
   * Provides a search field for Vector3.
   */
  [System.Serializable] 
  public class DynamicTypeVector3 : DynamicTypeData<Vector3Serializable>
  {

    public static List<Type> replaceOptionTypes = new List<Type>()
    {
      typeof(ReplaceItemVector3),  
    };

    [System.NonSerialized]
    public Vector3 val;

    public OptionalFloatField xField;
    public OptionalFloatField yField;
    public OptionalFloatField zField;
    bool approximate = true;

    public override void Draw(SearchOptions options)
    {
      this.options = options;

      GUILayout.BeginHorizontal();
      drawControlStart();
      GUILayout.BeginHorizontal();
      SRWindow.Instance.CVS();
      bool enabled = !anyValueEquals();
      xField.Draw(showMoreOptions, enabled);
      yField.Draw(showMoreOptions, enabled);
      zField.Draw(showMoreOptions, enabled);
      if(xField.updated || yField.updated || zField.updated)
      {
        val = new Vector3(xField.fieldValue, yField.fieldValue, zField.fieldValue);
        searchValue = Vector3Serializable.FromVector3(val);
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
      GUILayout.EndHorizontal();
      GUILayout.EndHorizontal();

      SRWindow.Instance.CVE();
      drawControlEnd();
      drawReplaceItem(options);

    }

    public override void OnDeserialization()
    {
      initReplaceOptions(replaceOptionTypes);
      val = searchValue.ToVector3();

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

    }

    public override bool ValueEquals(SerializedProperty prop)
    {
      Vector3 v3 = prop.vector3Value;
      //optional float field will take showMoreOptions into account!
      return xField.ValueEquals(v3.x, approximate) && yField.ValueEquals(v3.y, approximate) && zField.ValueEquals(v3.z, approximate);
    }

    public override bool IsValid()
    {
      if(showMoreOptions)
      {
        return true;
      }

      return xField.selected || yField.selected || zField.selected;
    }

    public override string StringValue()
    {
      return "("+xField.StringValue() + ","+yField.StringValue()+","+zField.StringValue()+")";
    }

    public override string StringValueFor(SerializedProperty prop)
    {
      return prop.vector3Value.ToString();
    }

    public override SerializedPropertyType PropertyType()
    {
      return SerializedPropertyType.Vector3;
    }

    public override bool _hasAdvancedOptions()
    {
      return true;
    }   

    protected override void initializeDefaultValue(SerializedProperty prop)
    {
      Vector3 v3 = prop.vector3Value;
      xField.fieldValue = v3.x;
      yField.fieldValue = v3.y;
      zField.fieldValue = v3.z;
    }
  }
}
