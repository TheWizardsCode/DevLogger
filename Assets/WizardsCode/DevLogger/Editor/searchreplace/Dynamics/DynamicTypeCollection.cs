using UnityEngine;
using UnityEditor;
using System;
using System.Collections;
using System.Collections.Generic;

namespace sr
{
  /**
   * Encapsulates a search field for searching for collection types.
   */
  [System.Serializable]
  public class DynamicTypeCollection : DynamicTypeData
  {

    // The type of object that is in the collection.
    DynamicTypeField typeField;

    [System.NonSerialized]
    InitializationContext initializationContext;

    public override void Draw(SearchOptions options)
    {
      if(isCollection())
      {
        setSubtype();
        // Debug.Log("[DynamicTypeCollection] array of "+subType);
        initTypeField();
        typeField.showMoreOptions = showMoreOptions;
        typeField.SetType(initializationContext);
        typeField.Draw(options);
      }
    }

    void initTypeField()
    {
      if(typeField == null)
      {
        typeField = new DynamicTypeField();
        typeField.OnDeserialization();
      }
    }

    void setSubtype()
    {
      Type subType = null;
      if(parent.type.IsArray)
      {
        subType = parent.type.GetElementType();
      }else{
        //Generic?
        subType = parent.type.GetGenericArguments()[0];
        // Debug.Log("[DynamicTypeCollection] subtype of generic:"+subType);
      }
      initializationContext = new InitializationContext(subType);
    }

    // This needs to set the subtype correctly, otherwise showAdvancedOptions
    // will return false during layout, then true afterwards causing a 
    // mismatch in layout data.
    // don't use the ic passed in...that's the wrong type!
    public override void OnSelect(InitializationContext ic)
    {
      setSubtype();
      if(typeField != null)
      {
        typeField.SetType(initializationContext);
      }
    }

    public override void OnDeserialization()
    {
      if(typeField != null)
      {
        typeField.OnDeserialization();
        typeField.showMoreOptions = parent.showMoreOptions;
      }
    }

    
    bool isCollection()
    {
      if(parent.type.IsArray)
      {
        return true;
      }
      if(parent.type.IsGenericType && parent.type.GetGenericTypeDefinition().IsAssignableFrom(typeof(List<>)))
      {
        return true;
      }
      return false;
    }

    public override void SearchProperty( SearchJob job, SearchItem item, SerializedProperty prop)
    {
      if(isCollection())
      {
        //Time for some freakish magic!
        SerializedProperty iterator = prop.Copy();
        iterator.Next(true);
        iterator.Next(true);
        int count = iterator.intValue;
        for(int i=0;i < count; i++)
        {
          iterator.Next(false);
          typeField.SearchProperty(job, item, iterator);
        }
      }
    }

    public override void ReplaceProperty(SearchJob job, SerializedProperty prop, SearchResult result)
    {
      typeField.ReplaceProperty(job, prop, result);
    }

    public override bool IsValid()
    {
      if(isCollection())
      {
        initTypeField();
        return typeField.IsValid();
      }else{
        return false;
      }
    }

    public override bool IsReplaceValid()
    {
      if(isCollection())
      {
        initTypeField();
        return typeField.IsReplaceValid();
      }else{
        return false;
      }
    }

    public override string StringValue()
    {
      if(isCollection())
      {
        initTypeField();
        return typeField.StringValue();
      }else{
        return "";
      }
    }

    public override string StringValueWithConditional()
    {
      if(isCollection())
      {
        initTypeField();
        return typeField.StringValueWithConditional();
      }else{
        return "";
      }
    }

    public override bool _hasAdvancedOptions()
    {
      if(isCollection())
      {
        initTypeField();
        return typeField.hasAdvancedOptions();
      }
      return false;
    }


  }
}
