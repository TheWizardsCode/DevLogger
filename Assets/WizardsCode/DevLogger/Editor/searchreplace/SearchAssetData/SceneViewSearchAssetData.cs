using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;
#if UNITY_2018_3_OR_NEWER
using UnityEditor.Experimental.SceneManagement;
#endif
using UnityEngine.SceneManagement;
using UnityEditor.SceneManagement;

namespace sr
{
  /**
   * Data model of a currently open scene or prefab stage that is being searched.
   */
  public class SceneViewSearchAssetData : SearchAssetData
  {
    Scene scene;
    public SceneViewSearchAssetData(string path) : base(path)
    {
      containsScripts = true;
#if UNITY_2018_3_OR_NEWER
      if (SceneUtil.IsSceneStage())
      {
        PrefabStage prefabStage = PrefabStageUtility.GetCurrentPrefabStage();
        scene = prefabStage.scene;
#if UNITY_2020_1_OR_NEWER
        assetPath = prefabStage.assetPath;
#else
        assetPath = prefabStage.prefabAssetPath;
#endif
        roots = new UnityEngine.Object[] { prefabStage.prefabContentsRoot };
      }else{
        addActiveScene();
      }
#else
      addActiveScene();
#endif
    }

    private void addActiveScene()
    {
      scene = EditorSceneManager.GetActiveScene();
      roots = scene.GetRootGameObjects();
    }

    public override void ProcessAsset(SearchJob job)
    {
       foreach(GameObject root in roots)
       {
        job.searchGameObject(root, excludedObjects);
       }
       if(assetIsDirty)
       {
         EditorSceneManager.MarkSceneDirty(scene);
       }
    }
    
    public override SearchAssetData Clone()
    {
      SceneViewSearchAssetData assetData = new SceneViewSearchAssetData(assetPath);
      return assetData;
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
      if (SceneUtil.IsSceneStage() && roots.Contains(go))
      {
        reason = "Cannot replace root of prefab.";
        return false;
      }

      return true;
    }


  }
}