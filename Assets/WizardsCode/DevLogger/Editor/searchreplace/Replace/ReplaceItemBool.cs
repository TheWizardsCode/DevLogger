using UnityEngine;
using UnityEditor;
using System.IO;
using System.Reflection;

namespace sr
{
  [System.Serializable]
  public class ReplaceItemBool : ReplaceItem<DynamicTypeBool, bool>
  {

    protected override bool drawEditor()
    {
      return EditorGUILayout.Toggle(Keys.Replace, replaceValue);
    }

    protected override void replace(SearchJob job, SerializedProperty prop, SearchResult result)
    {
      prop.boolValue = replaceValue;
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
        result.replaceStrRep = replaceValue.ToString();
      }else{
        result.replaceStrRep = "Unsupported";
      }
    }
  }

}