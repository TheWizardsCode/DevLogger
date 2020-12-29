using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;

namespace sr
{
  /**
   * Data model of an object that is searched. Has it already been searched?
   * etc.
   */
  public class ImportableSearchAssetData : SearchAssetData
  {

    protected AssetImporter importer;
    public ImportableSearchAssetData(string path) : base(path)
    {
      assetRequiresImporter = true;
    }
    public virtual AssetImporter GetImporter() 
    {
      if(importer == null)
      {
        importer = AssetImporter.GetAtPath(assetPath);
      }
      return importer;
    }

    public override void ProcessAsset(SearchJob job)
    {
      UnityEngine.Object root = (UnityEngine.Object) AssetDatabase.LoadAssetAtPath(assetPath, typeof(UnityEngine.Object) );
      if(root != null)
      {
        roots = new Object[]{ root };
        job.OnAssetSearchBegin();
        job.searchObject(root); // todo: cache for subjobs?
        if(importer != null)
        {
          importer.SaveAndReimport();
        }
        job.OnAssetSearchEnd();
        // yield return job.searchDependencies(root);
      }
    }


  }
}