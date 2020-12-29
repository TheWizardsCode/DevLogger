using UnityEngine;
using UnityEditor;
using System;
using System.Collections;
using System.Collections.Generic;

namespace sr
{
  /**
   * FieldData objects abstract out the data we need about a field from both SerializedProperty and
   * FieldInfo objects. FieldDataUtil contains a list of internal/difficult to search
   * properties on Unity objects, which are extracted through reflection of these
   * objects.
   */
  public class FieldData
  {
    public enum FieldDataType
    {
      // Strong means that the type is part of a property, and we can't look for
      // subobjects.
      Strong, 
      // Weak means we are doing a broader search and can look for subobjects
      Weak
    }

    // The type of the object we are looking at.
    public Type objectType;
    // The type of field on the object.
    public Type fieldType;
    // The actual field name.
    public string fieldName;
    // A human readable string describing the field.
    public string displayName;

    public FieldDataType fieldDataType = FieldDataType.Strong;

    public List<SerializedProperty> foundProps = new List<SerializedProperty>();
    
    public FieldData(){
      fieldDataType = FieldDataType.Weak;
    }

    public FieldData(Type t, Type ot, string n, string dn)
    {
      fieldType = t;
      fieldName = n;
      displayName = dn;
      objectType = ot;
    }

    public virtual List<SerializedProperty> findProperties(SerializedProperty iterator)
    {
      foundProps.Clear();
      SerializedProperty foundProp = iterator.serializedObject.FindProperty(fieldName);
      if(foundProp != null)
      {
        foundProps.Add(foundProp);
      }
      return foundProps;
    }

  }
}