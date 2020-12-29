using System.Collections.Generic;

namespace sr
{
  
  public class SearchScopeData
  {
    public ProjectScope projectScope;
    public AssetScope assetScope;
    public ObjectID scopeObj;

    public bool searchDependencies;
    public bool loadPrefab;

    public SearchScopeData(ProjectScope ep, AssetScope s, ObjectID oid, bool sd, bool lp)
    {
      projectScope = ep;
      assetScope = s;
      scopeObj = oid;
      searchDependencies = sd;
      loadPrefab = lp;
    }

    static Dictionary<AssetScope, string[]> suffixesForScope = new Dictionary<AssetScope, string[]>
    {
      { AssetScope.Prefabs, new string[]{ ".prefab" } },
      { AssetScope.Scenes, new string[]{ ".unity" } },
      { AssetScope.ScriptableObjects, new string[]{ ".asset" } },
      { AssetScope.Materials, new string[]{ ".mat" } },
      { AssetScope.Animations, new string[]{ ".anim" } },
      { AssetScope.Animators, new string[]{ ".controller" } },
      { AssetScope.AudioClips, new string[]{ ".wav", ".mp3" } },
      { AssetScope.Textures, new string[]{ ".png", ".psd", ".tiff", ".tif",".tga",".gif",".bmp",".jpg",".jpeg",".iff",".pict" } },
    };

    public HashSet<string> GetSuffixesForScope()
    {
      return GetSuffixesForScope(assetScope);
    }

    public HashSet<string> GetSuffixesForScope(AssetScope scope)
    {
      HashSet<string> retVal = new HashSet<string>();
      foreach(var kvp in suffixesForScope)
      {
        if((scope & kvp.Key) == kvp.Key)
        {
          foreach(string suffix in kvp.Value)
          {
            retVal.Add(suffix);
          }
        }
      }
      return retVal;      
    }
  }
}