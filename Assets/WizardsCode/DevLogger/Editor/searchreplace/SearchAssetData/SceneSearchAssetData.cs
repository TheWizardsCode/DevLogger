using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;
#if UNITY_2018_3_OR_NEWER
using UnityEditor.Experimental.SceneManagement;
#endif
using UnityEngine.SceneManagement;
using UnityEditor.SceneManagement;
using System.Linq;

namespace sr
{
  /**
   * Data model of a currently open scene or prefab stage that is being searched.
   */
  public class SceneSearchAssetData : SearchAssetData
  {
    Scene scene;

    private bool shouldUnload = false;
    public SceneSearchAssetData(string path) : base(path)
    {
        containsScripts = true;
    }
    public override void ProcessAsset(SearchJob job)
    {
       scene = SceneUtil.LoadScene(assetPath, OpenSceneMode.Single).Value;
       if(roots == null)
       {
         roots = scene.GetRootGameObjects();
         shouldUnload = true;
       }       
       
       job.OnAssetSearchBegin();
       foreach(GameObject root in roots)
       {
        job.searchGameObject(root, excludedObjects);
       }
       job.OnAssetSearchEnd();
       
       if(assetIsDirty)
       {
         EditorSceneManager.SaveScene(scene);
       }
    }
    
    public override SearchAssetData Clone()
    {
      SceneSearchAssetData assetData = new SceneSearchAssetData(assetPath);
      assetData.roots = roots;
      return assetData;
    }
    
    public override void Unload()
    {
      if(shouldUnload)
      {
        roots = null;
      }
    }



  }
}