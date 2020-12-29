using UnityEngine;
using UnityEditor;
using System;
using System.Collections;
using System.Collections.Generic;

namespace sr
{
  /**
   * When an object isn't of an easily recognisable type, we fall back on this
   * class, which displays an internal global (aka 'type' search) search as a 
   * subfield of this field.
   */
  [System.Serializable]
  public class DynamicCustomData : DynamicTypeData
  {
    [System.NonSerialized]
    Type type;

    public SearchItemGlobal subField;

    public override void Draw(SearchOptions options)
    {
      if(type != parent.type)
      {
        type = parent.type;
        //update shit.
        SRWindow.Instance.PersistCurrentSearch();
      }
      initSubField();
      subField.showMoreOptions = showMoreOptions;
      subField.Draw(options);
    }

    void initSubField()
    {
      if(subField == null)
      {
        subField = new SearchItemGlobal();
        subField.depth = 1;
        subField.OnDeserialization();
      }
    }

    public override void OnDeserialization()
    {
      type = parent.type;
      if(subField != null)
      {
        subField.depth = 1;
        subField.OnDeserialization();
        subField.showMoreOptions = showMoreOptions;
      }
    }

    public override void SearchProperty( SearchJob job, SearchItem item, SerializedProperty prop)
    {
      subField.SearchProperty(job, item, prop);
    }

    public override void ReplaceProperty(SearchJob job, SerializedProperty prop, SearchResult result)
    {
      //handled by subfield...no need?
    }

    public override bool IsValid()
    {
      initSubField();
      return subField.IsValid();
    }

    public override bool IsReplaceValid()
    {
      // This DTD has no replaceItem. Instead we have a subField that has a replace
      // item.
      return subField.IsReplaceValid();
    }

    public override string StringValue()
    {
      initSubField();
      return subField.GetDescription();
    }

    public override string StringValueWithConditional()
    {
      initSubField();
      return subField.GetDescription();
    }

    public override bool _hasAdvancedOptions()
    {
      initSubField();
      return subField.hasAdvancedOptions();
    }

  }
}
