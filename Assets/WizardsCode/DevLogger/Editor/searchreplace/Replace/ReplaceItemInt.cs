using UnityEngine;
using UnityEditor;
using System.IO;
using System.Reflection;

namespace sr
{
  [System.Serializable]
  public class ReplaceItemInt : ReplaceItem<DynamicTypeInt, int>
  {

    protected override int drawEditor()
    {
      return EditorGUILayout.IntField(Keys.Replace, replaceValue);
    }

    protected override void replace(SearchJob job, SerializedProperty prop, SearchResult result)
    {
      prop.intValue = replaceValue;
      result.replaceStrRep = replaceValue.ToString();
    }

    protected override void reimport(SearchJob job, SerializedProperty prop, SearchResult result)
    {
      ImportableSearchAssetData assetData = (ImportableSearchAssetData) job.assetData;
      AssetImporter importer = assetData.GetImporter();
      MethodInfo m = AssetImporterMethodUtil.GetMethodForProperty(importer, prop);
      if(m != null)
      {
        m.Invoke(importer, new object[]{replaceValue});
        importer.SaveAndReimport();
        result.replaceStrRep = replaceValue.ToString();
      }else{
        result.replaceStrRep = "Unsupported";
      }
    }
  }



}