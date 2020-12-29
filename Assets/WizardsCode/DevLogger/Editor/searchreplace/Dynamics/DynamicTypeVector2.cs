using UnityEngine;
using UnityEditor;
using System.Text;
using System;
using System.Collections;
using System.Collections.Generic;
namespace sr
{
  /**
   * Provides a search field for vector2.
   */
  [System.Serializable]
  public class DynamicTypeVector2 : DynamicTypeData<Vector2Serializable>
  {

    public static List<Type> replaceOptionTypes = new List<Type>()
    {
      typeof(ReplaceItemVector2),  
    };

    [System.NonSerialized]
    public Vector2 val;

    public OptionalFloatField xField;
    public OptionalFloatField yField;

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

      if(xField.updated || yField.updated)
      {
        val = new Vector2(xField.fieldValue, yField.fieldValue);
        searchValue = Vector2Serializable.FromVector2(val);
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
      val = searchValue.ToVector2();

      if(xField == null)
      {
        xField = new OptionalFloatField("X", val.x);
      }
      if(yField == null )
      {
        yField = new OptionalFloatField("Y", val.y);
      }

    }

    public override bool ValueEquals(SerializedProperty prop)
    {
      Vector2 v2 = prop.vector2Value;
      return xField.ValueEquals(v2.x, approximate) && yField.ValueEquals(v2.y, approximate);
    }

    public override bool IsValid()
    {
      if(showMoreOptions)
      {
        return true;
      }

      return xField.selected || yField.selected;
    }

    public override string StringValue()
    {
      return "("+xField.StringValue() + ","+yField.StringValue()+")";
    }

    public override string StringValueFor(SerializedProperty prop)
    {
      return prop.vector2Value.ToString();
    }


    public override SerializedPropertyType PropertyType()
    {
      return SerializedPropertyType.Vector2;
    }

    public override bool _hasAdvancedOptions()
    {
      return true;
    }

    protected override void initializeDefaultValue(SerializedProperty prop)
    {
      Vector2 v2 = prop.vector2Value;
      xField.fieldValue = v2.x;
      yField.fieldValue = v2.y;
    }
  }
}
