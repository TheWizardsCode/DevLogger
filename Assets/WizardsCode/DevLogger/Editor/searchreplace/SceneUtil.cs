using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
#if UNITY_2018_3_OR_NEWER
using UnityEditor.Experimental.SceneManagement;
#endif
using UnityEngine.SceneManagement;
using UnityEditor.SceneManagement;

namespace sr
{
  /**
   * Various utilities provides to simplify scene-related actions. Used to be
   * a wrapper for functionality before the 'new' Scene Management API.
   */ 
  public class SceneUtil
  {

    public static Scene? LoadScene(string assetPath, OpenSceneMode mode)
    {
      // Debug.Log("[SceneSubJob] assetPath:"+assetPath);
      UnityEngine.Object sceneObj = AssetDatabase.LoadMainAssetAtPath(assetPath);
      if(sceneObj == null)
      {
        // this probably means we are in a new scene.
        // Debug.Log("[SceneUtil] path is missing for " + assetPath);
        return EditorSceneManager.GetActiveScene();
        // return null;
      }
      // Debug.Log("[SceneUtil] " + assetPath + " sceneObj:"+sceneObj, sceneObj);
      Scene s = EditorSceneManager.GetSceneByPath(assetPath);
      if(!s.IsValid())
      {
        // Debug.Log("[SceneUtil] attempting to open scene");
        s = EditorSceneManager.OpenScene(assetPath, mode);
      }
      EditorSceneManager.SetActiveScene(s); // PathInfo/ObjectID uses GetActiveScene(). Importanté!
      return s;
    }

    public static bool IsSceneStage()
    {
#if UNITY_2018_3_OR_NEWER
     return PrefabStageUtility.GetCurrentPrefabStage() != null;
#else
      return false;
#endif
    }
    public static string GuidPathForActiveScene()
    {
#if UNITY_2018_3_OR_NEWER
      if (IsSceneStage())
      {
#if UNITY_2020_1_OR_NEWER
        return PrefabStageUtility.GetCurrentPrefabStage().assetPath;
#else
        return PrefabStageUtility.GetCurrentPrefabStage().prefabAssetPath;
#endif
      }
#endif
      return EditorSceneManager.GetActiveScene().path;
    }

    public static bool SaveDirtyScenes()
    {
      return EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();
    }


  }
}