using System;
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
  public class PrefabSearchAssetData : SearchAssetData
  {
    /// <summary>
    /// Whether to load the prefab into its own scene, for more info check out SearchScope.
    /// </summary>
    public GameObject root = null;
#if UNITY_2018_3_OR_NEWER
    bool loadPrefab = false;
    bool shouldUnload = false;
#endif
    public PrefabSearchAssetData(string path) : base(path)
    {
      containsScripts = true;
    }
    public override void ProcessAsset(SearchJob job)
    {
      
#if UNITY_2018_3_OR_NEWER
      loadPrefab = job.scope.loadPrefab;
      // for the new prefab workflow we need to load the prefab into an
      // anonymous scene.
      
      if(roots == null)
      {
        if (loadPrefab)
        {
          root = PrefabUtility.LoadPrefabContents(assetPath);
        }
        else
        {
          root = (GameObject) AssetDatabase.LoadAssetAtPath(assetPath, typeof(UnityEngine.Object) );
        }
        if (root == null)
        {
          Debug.Log("Could not load " + assetPath + "!");
          return;
        }
        shouldUnload = true;
        roots = new UnityEngine.Object[] { root };
      }      
      job.OnAssetSearchBegin();
      job.searchGameObject(root, excludedObjects);
      job.OnAssetSearchEnd();

      if(assetIsDirty)
      {
        if (loadPrefab)
        {
          // Known issue: when calling SaveAsPrefabAsset, unity will overwrite the name of the game object.
          PrefabUtility.SaveAsPrefabAsset(root, assetPath);
        }
      }
#else
      root = (GameObject) AssetDatabase.LoadAssetAtPath(assetPath, typeof(UnityEngine.Object) );
      roots = new UnityEngine.Object[] { root };
      job.OnAssetSearchBegin();
      job.searchGameObject(root, excludedObjects);
      job.OnAssetSearchEnd();
      
#endif
    }

    public override void Unload()
    {
#if UNITY_2018_3_OR_NEWER
      if(shouldUnload && loadPrefab)
      {
        try
        {
          PrefabUtility.UnloadPrefabContents(root);
        }
        catch (Exception ex)
        {
          Debug.Log("An exception was thrown attempting to unload " + assetPath + " with root " + root);
          Debug.LogException(ex);
        }

      }
#endif
      root = null;
    }

    public override SearchAssetData Clone()
    {
      PrefabSearchAssetData assetData = new PrefabSearchAssetData(assetPath);
      assetData.root = root;
      assetData.roots = roots;
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
      if (go == root)
      {
        reason = "Cannot replace root of prefab.";
        return false;
      }

      return true;
    }


  }
}