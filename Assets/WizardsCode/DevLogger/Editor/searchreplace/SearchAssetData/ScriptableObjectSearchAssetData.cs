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
  public class ScriptableObjectSearchAssetData : SearchAssetData
  {

    public ScriptableObjectSearchAssetData(string path) : base(path)
    {
        containsScripts = true;
    }
    public override void ProcessAsset(SearchJob job)
    {
      ScriptableObject root = (ScriptableObject) AssetDatabase.LoadAssetAtPath(assetPath, typeof(ScriptableObject) );
      if(root != null)
      {
        job.OnAssetSearchBegin();
        job.searchObject(root); // todo: cache for subjobs?
        job.OnAssetSearchEnd();
      }

    }

  }
}