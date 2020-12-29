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
  public abstract class SearchAssetData
  {
    public bool containsScripts = false;
    public bool hasBeenSearched = false;
    //The path of the asset relative to the project folder or "scene object" if this is a scene object.
    public string assetPath = "";
    public string assetName = "";
    public string assetExtension = "";
    // If this asset is internal to another asset (for example, a StateMachine internal to an Animator)
    // then this will provide a path that can be inserted into a path Info object.
    // This should be used only for 'internal' assets!
    // Formatted with backslashes, and should have backslash on the end if this is set.
    public string internalAssetPath = "";
    public List<SearchAssetData> dependencies;

    //Whether search and replace has modified the current asset.
    public bool assetIsDirty = false;

    //Whether or not we need a specific AssetImporter to import this object.
    public bool assetRequiresImporter = false;

    // If a SearchItem has executed a search on this asset, then this asset was
    // searched.
    public bool searchExecuted = false;

    //The scope this asset was created in, or None if its a dependency.
    public AssetScope assetScope = AssetScope.None;

    // Provides a mechanism to exclude certain objects from a search.
    protected HashSet<UnityEngine.Object> excludedObjects = null;

    public UnityEngine.Object[] Roots
    {
      get{
        return roots;
      }
    }

    protected UnityEngine.Object[] roots = null;

    // Helper function to set the internal asset path of an asset. This will
    // add the appropriate slashes, and if the list is empty, set to string.Empty.
    public void SetInternalAssetPath(List<string> internalPath)
    {
      if(internalPath.Count > 0)
      {
        internalAssetPath = "/" + string.Join("/", internalPath.ToArray());
      }else{
        internalAssetPath = string.Empty;
      }
    }

    public SearchAssetData(string path)
    {
      assetPath = path;
      assetName = Path.GetFileName(path);
      assetExtension = Path.GetExtension(path);
    }

    public void addDependency(SearchAssetData dependency)
    {
      if(dependencies == null)
      {
        dependencies = new List<SearchAssetData>();
      }
      dependencies.Add(dependency);
    }

    public virtual bool CanReplaceObject(UnityEngine.Object obj, out string reason)
    {
      reason = "";
      return true;
    }

    public void ExcludeObject(UnityEngine.Object obj)
    {
      if (excludedObjects == null)
      {
        excludedObjects = new HashSet<Object>();
      }
      excludedObjects.Add(obj);
    }
    
    public virtual void ProcessAsset(SearchJob job)
    {
    }

    public virtual void Unload()
    {
      roots = null;
    }

    public virtual SearchAssetData Clone()
    {
      return null;
    }

  }
}