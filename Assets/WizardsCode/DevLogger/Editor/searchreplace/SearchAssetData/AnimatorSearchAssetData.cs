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
  public class AnimatorSearchAssetData : SearchAssetData
  {

    public AnimatorSearchAssetData(string path) : base(path)
    {
    }
    public override void ProcessAsset(SearchJob job)
    {
      AnimatorController root = (AnimatorController) AssetDatabase.LoadAssetAtPath(assetPath, typeof(AnimatorController) );
      if(root != null)
      {
        roots = new Object[]{ root };
        job.OnAssetSearchBegin();
        job.searchAnimatorController(root); // todo: cache for subjobs?
        job.OnAssetSearchEnd();
        // yield return job.searchDependencies(root);
        
      }

    }

  }
}