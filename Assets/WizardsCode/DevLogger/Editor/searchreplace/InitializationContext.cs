using UnityEngine;
using UnityEditor;
using System;

namespace sr
{
  /**
   * When a DynamicTypeField needs to be initialized by a SearchItem, this is 
   * passed in to provide information about the type of data it needs to display
   * a search UI for.
   */
  public class InitializationContext
  {

    public FieldData fieldData;
    //Don't ever expect this to be the correct type...or even exist!
    public UnityEngine.Object obj;

    // Instructs users of this context that they should
    // update regardless of whether things like the 
    // field data match, etc.
    // This is used so we don't overwrite user's values on save.
    public bool forceUpdate;

    public InitializationContext(FieldData fd, UnityEngine.Object o)
    {
      fieldData = fd;
      obj = o;
    }

    public InitializationContext(Type t)
    {
      fieldData = new FieldData();
      fieldData.fieldType = t;
    }

    public void updateFieldData(FieldData fd)
    {
      if(fd != fieldData)
      {
        fieldData = fd;
        forceUpdate = true;
      }
    }

    public SerializedProperty getProp()
    {
      if(!forceUpdate)
      {
        return null;
      }
      if(fieldData.fieldName != null)
      {
        // Debug.Log("[DynamicTypeString] name:"+fieldData.fieldName);
        if(obj != null)
        {
          SerializedObject so = null;
          if(obj.GetType() == fieldData.objectType)
          {
            so = new SerializedObject(obj);
          }else{
            if(obj is GameObject && fieldData.objectType != null)
            {
              GameObject go = (GameObject)obj;
              Component c = go.GetComponent(fieldData.objectType);
              if(c != null)
              {
                so = new SerializedObject(c);
              }
            }
          }
          if(so == null)
          {
            return null;
          }
          return so.FindProperty(fieldData.fieldName);
        }
      }

      return null;
    }
  }
}