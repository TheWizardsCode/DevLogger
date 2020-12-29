using UnityEngine;
using UnityEditor;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace sr
{
  /**
   * Uses a regex match to attempt to find a match against a Serialized property.
   * This iterates over literally EVERY SerializedProperty of an object and so
   * can be quite slow. In order to speed this up we also test against the object
   * type and only iterate if it is of the given type.
   *
   * Currently used for animation clip searching, so its worth it!
   */
  public class RegexFieldData : FieldData
  {
    Regex expr;

    static RegexOptions regexOptions = RegexOptions.Singleline | RegexOptions.Compiled;

    SerializedPropertyType propertyType;

    public RegexFieldData(){}

    public RegexFieldData(Type t, Type ot,  string n, string dn, SerializedPropertyType propType)
    {
      fieldType = t;
      displayName = dn;
      fieldName = dn; // Use the display name for the field name as well.
      expr = new Regex(n, regexOptions);
      propertyType = propType;
      objectType = ot;
    }

    public override List<SerializedProperty> findProperties(SerializedProperty iterator)
    {
      foundProps.Clear();

      UnityEngine.Object obj = iterator.serializedObject.targetObject;
      if(obj == null)
      {
        // Nothing to search!
        return foundProps;
      }

      Type objType = obj.GetType();
      if(!objType.IsAssignableFrom(objectType))
      {
        // Not the correct object type! Get outta here.
        return foundProps;
      }

      SerializedProperty prop = iterator.Copy();
      while(prop.Next(true))
      {
        if(prop.propertyType == propertyType)
        {
          if(expr.Matches(prop.propertyPath).Count > 0)
          {
            foundProps.Add(prop.Copy());
          }
        }
      }
      return foundProps;
    }

  }
}
