using UnityEngine;
using UnityEditor;
using System;
using System.Collections;
using System.Collections.Generic;

namespace sr
{
  /**
   * Provides a search field for PropSheets. PropSheet is a class used internally
   * for searching in materials. It is a generic type (for ex. PropSheet<float> or PropSheet<Texture>).
   */
  [System.Serializable]
  public class DynamicTypePropSheet : DynamicTypeData
  {

    [System.NonSerialized]
    InitializationContext initializationContext;
    
    DynamicTypeField typeField;
    
    private string searchByNameValue;
    
    private bool isSearchByName;

    public override void Draw(SearchOptions options)
    {
      Type subType = parent.type.GetGenericArguments()[0];
      initializationContext = new InitializationContext(subType);

//      Debug.Log("[DynamicTypeCollection] array of "+subType);
      if(typeField == null)
      {
        typeField = new DynamicTypeField();
        typeField.OnDeserialization();
      }
      if (isSearchByName)
      {
        EditorGUILayout.BeginHorizontal();
        GUILayout.Label("Field:", GUILayout.Width(SRWindow.compactLabelWidthNP));
        searchByNameValue = EditorGUILayout.TextField(searchByNameValue);
        EditorGUILayout.EndHorizontal();
      }
      typeField.SetType(initializationContext);
      typeField.Draw(options);
    }

    public override void OnDeserialization()
    {
      if(typeField != null)
      {
        typeField.OnDeserialization();
      }
    }

    public override void SearchProperty( SearchJob job, SearchItem item, SerializedProperty prop)
    {
      //Time for some freakish magic!
      SerializedProperty iterator = prop.Copy();
      while(iterator.NextVisible(true))
      {
        if(typeField.PropertyType() == iterator.propertyType)
        {
          if (isSearchByName)
          {
            // path = prop.propertyPath + .data[x].second
            var path = iterator.propertyPath;
            // path = data[x].second
            path = path.Substring(prop.propertyPath.Length + 1);
            // path = data[x]
            const string endOfPath = "].";
            path = path.Substring(0, path.LastIndexOf(endOfPath) + endOfPath.Length);
            if (prop.FindPropertyRelative(path + "first").stringValue != searchByNameValue)
            {
              continue;
            }
          }
          //might need to add a guard against collections or something?
          typeField.SearchProperty(job, item, iterator);
        }
      }
      // iterator.Next(true);
      // iterator.Next(true);
      // int count = iterator.intValue;
      // for(int i=0;i < count; i++)
      // {
      //   iterator.Next(false);
      //   typeField.SearchProperty(job, item, iterator);
      // }
    }

    public override bool IsReplaceValid()
    {
      return IsValid() && typeField.IsReplaceValid();
    }
    public override void ReplaceProperty(SearchJob job, SerializedProperty prop, SearchResult result)
    {
      typeField.ReplaceProperty(job, prop, result);
    }

    public override bool IsValid()
    {
      if (isSearchByName)
      {
        return typeField.IsValid() && !string.IsNullOrEmpty(searchByNameValue);
      }
      else
      {
        return typeField.IsValid();
      }
    }

    public override string StringValue()
    {
      if(typeField == null)
      {
        return "";
      }
      return typeField.StringValue();
    }

    public override void OnSelect(InitializationContext ic)
    {
      base.OnSelect(ic);
      isSearchByName = ic.fieldData.displayName.ToLowerInvariant().Contains("specific");
    }
  }
}
