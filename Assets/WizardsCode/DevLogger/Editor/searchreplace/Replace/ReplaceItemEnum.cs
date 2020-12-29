using UnityEngine;
using UnityEditor;
using System;
using System.Text;
using System.IO;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;

namespace sr
{
  [System.Serializable]
  public class ReplaceItemEnum : ReplaceItem<DynamicTypeEnum, int>
  {
    public string[] names;

    protected override int drawEditor()
    {
      names = System.Enum.GetNames(type); //todo: improve caching?
      return EditorGUILayout.Popup(Keys.Replace, replaceValue, names);
    }

    protected override void replace(SearchJob job, SerializedProperty prop, SearchResult result)
    {
      if(prop.propertyType == SerializedPropertyType.Enum)
      {
        prop.enumValueIndex = replaceValue;
      }else{
        prop.intValue = replaceValue;
      }
      result.replaceStrRep = names[replaceValue];
    }

    protected override void reimport(SearchJob job, SerializedProperty prop, SearchResult result)
    {
      ImportableSearchAssetData assetData = (ImportableSearchAssetData) job.assetData;
      AssetImporter importer = assetData.GetImporter();

      MethodInfo m = AssetImporterMethodUtil.GetMethodForProperty(importer, prop);
      if(m != null)
      {
        Debug.Log("replacing");
        m.Invoke(importer, new object[]{replaceValue});
        result.replaceStrRep = names[replaceValue];
      }else{
        result.replaceStrRep = "Unsupported";
      }
    }

  }

 

}