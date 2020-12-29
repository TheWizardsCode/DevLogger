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
  public class UnityObjectSearchAssetData : SearchAssetData
  {

    public UnityObjectSearchAssetData(string path) : base(path)
    {
    }
    public override void ProcessAsset(SearchJob job)
    {
      UnityEngine.Object root = (UnityEngine.Object) AssetDatabase.LoadAssetAtPath(assetPath, typeof(UnityEngine.Object) );
      if(root != null)
      {
        roots = new Object[]{ root };
        job.OnAssetSearchBegin();
        job.searchObject(root);
        job.OnAssetSearchEnd();
      }

    }

  }
}