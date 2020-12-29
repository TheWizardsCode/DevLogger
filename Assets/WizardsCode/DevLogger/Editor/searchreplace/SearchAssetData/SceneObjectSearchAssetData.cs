using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;
#if UNITY_2018_3_OR_NEWER
using UnityEditor.Experimental.SceneManagement;
#endif
using UnityEditor.SceneManagement;

namespace sr
{
  /**
   * Data model of an object that is searched. Has it already been searched?
   * etc.
   */
  public class SceneObjectSearchAssetData : SearchAssetData
  {
    public List<GameObject> sceneObjects = new List<GameObject>();

    private bool isInPrefabStage = false;
    public SceneObjectSearchAssetData(string path) : base(path)
    {
    }

    public void AddObject(GameObject gameObject)
    {
      sceneObjects.Add(gameObject);
    }
    public override void ProcessAsset(SearchJob job)
    {
#if UNITY_2018_3_OR_NEWER
      PrefabStage stage = PrefabStageUtility.GetPrefabStage(sceneObjects[0]);
      if(stage != null)
      {
        isInPrefabStage = true;
      }
#endif

      roots = sceneObjects.ToArray();
      
      job.OnAssetSearchBegin();
      foreach(GameObject root in roots)
      {
        job.searchGameObject(root, excludedObjects);
      }

#if UNITY_2018_3_OR_NEWER
      if (assetIsDirty && isInPrefabStage)
      {
        EditorSceneManager.MarkSceneDirty(stage.scene);
      }
#endif    
    }
    public override bool CanReplaceObject(UnityEngine.Object obj, out string reason)
    {
      reason = "";
      if (!(obj is GameObject))
      {
        reason = "Not a game object.";
        return false;
      }

      GameObject go = (GameObject) obj;
      if (isInPrefabStage && go.transform.root == go.transform)
      {
        reason = "Cannot replace root of prefab.";
        return false;
      }

      return true;
    }
    
    public override SearchAssetData Clone()
    {
      SceneObjectSearchAssetData assetData = new SceneObjectSearchAssetData(assetPath);
      assetData.sceneObjects = sceneObjects;
      return assetData;
    }



  }
}