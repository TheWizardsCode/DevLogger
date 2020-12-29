using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace sr
{
    public static class SearchAssetFactory
    {
        static Dictionary<string, Func<string, SearchAssetData>> methods = new Dictionary<string, Func<string, SearchAssetData>>();

        static SearchAssetFactory()
        {
            methods[".prefab"] = (_) => new PrefabSearchAssetData(_);
            methods[".unity"] = (_) => new SceneSearchAssetData(_);
            methods[".asset"] = (_) => new ScriptableObjectSearchAssetData(_);
            methods[".mat"] = (_) => new UnityObjectSearchAssetData(_);
            methods[".anim"] = (_) => new UnityObjectSearchAssetData(_);
            methods[".controller"] = (_) => new AnimatorSearchAssetData(_);
            methods[".wav"] = (_) => new ImportableSearchAssetData(_);
            methods[".mp3"] = (_) => new ImportableSearchAssetData(_);
            methods[".png"] = (_) => new ImportableSearchAssetData(_);
            methods[".psd"] = (_) => new ImportableSearchAssetData(_);
            methods[".tiff"] = (_) => new ImportableSearchAssetData(_);
            methods[".tif"] = (_) => new ImportableSearchAssetData(_);
            methods[".tga"] = (_) => new ImportableSearchAssetData(_);
            methods[".gif"] = (_) => new ImportableSearchAssetData(_);
            methods[".bmp"] = (_) => new ImportableSearchAssetData(_);
            methods[".jpg"] = (_) => new ImportableSearchAssetData(_);
            methods[".jpeg"] = (_) => new ImportableSearchAssetData(_);
            methods[".iff"] = (_) => new ImportableSearchAssetData(_);
            methods[".pict"] = (_) => new ImportableSearchAssetData(_);
        }

        public static SearchAssetData AssetForPath(string assetPath)
        {
            string extension = Path.GetExtension(assetPath);
            Func<string, SearchAssetData> func;
            if(methods.TryGetValue(extension, out func))
            {
                return func(assetPath);
            };
            return null;
        }

        static void AddAsset(string assetPath, List<SearchAssetData> assets)
        {
            SearchAssetData assetData = AssetForPath(assetPath);
            if(assetData != null)
            {
                assets.Add(assetData);
            }
        }

        public static List<SearchAssetData> GetAssets(SearchScopeData scope)
        {
            List<SearchAssetData> retVal = new List<SearchAssetData>();
            if(scope.projectScope == ProjectScope.EntireProject)
            {
                //If the asset scope is set and has this value OR the scope is not set at all (everything)
                HashSet<string> suffixes = scope.GetSuffixesForScope();
                string[] allAssets = AssetDatabase.GetAllAssetPaths();
                IEnumerable<string> filteredPaths = allAssets.Where( asset => asset.StartsWith("Assets/")).Where( asset => suffixes.Contains(Path.GetExtension(asset).ToLowerInvariant()));

                foreach(string asset in filteredPaths)
                {
                    AddAsset(asset, retVal);
                }
            }else if(scope.projectScope == ProjectScope.SceneView)
            {
                SceneViewSearchAssetData sceneView = new SceneViewSearchAssetData(SceneUtil.GuidPathForActiveScene());
                retVal.Add(sceneView);
                
            }else if(scope.projectScope == ProjectScope.SpecificLocation)
            {
                string[] allAssets = null;
                HashSet<string> suffixes = scope.GetSuffixesForScope();
                addObjectsInLocation(scope.scopeObj, ref allAssets, suffixes, retVal);
            }else if(scope.projectScope == ProjectScope.CurrentSelection)
            {
                string[] allAssets = null;
                SceneObjectSearchAssetData sceneObject = null;
                foreach(UnityEngine.Object obj in Selection.objects)
                {
                    HashSet<string> suffixes = scope.GetSuffixesForScope(SearchScope.allAssets);
                    ObjectID objID = new ObjectID(obj);
                    objID.Clear();
                    if(objID.isSceneObject)
                    {
                        GameObject go = (GameObject) obj;
                        if(sceneObject == null)
                        {
                            sceneObject = new SceneObjectSearchAssetData(go.scene.path);
                            retVal.Add(sceneObject);
                        }
                        sceneObject.AddObject(go);
                    }else if(objID.isPrefabStageObject){
                        if(sceneObject == null)
                        {
                            sceneObject = new SceneObjectSearchAssetData(SceneUtil.GuidPathForActiveScene());
                            retVal.Add(sceneObject);
                        }
                        sceneObject.AddObject((GameObject)obj);

                    }else{
                        
                        //Ignore asset scope for current selection.
                        addObjectsInLocation(objID, ref allAssets, suffixes, retVal);
                    }
                }
            }
            return retVal;
        }

        static void addObjectsInLocation(ObjectID scopeObj, ref string[] allAssets,  HashSet<string> suffixes, List<SearchAssetData> retVal)
        {
            if(scopeObj.isDirectory)
            {
                if(allAssets == null)
                {
                    allAssets = AssetDatabase.GetAllAssetPaths();
                }
                string scopePath = scopeObj.assetPath;
                // this is essentially a recursive search.
                IEnumerable<string> filteredPaths = allAssets.Where( asset => asset.StartsWith(scopePath)).Where( asset => suffixes.Contains(Path.GetExtension(asset).ToLowerInvariant()));
                foreach(string asset in filteredPaths)
                {    
                    AddAsset(asset, retVal);
                }
            }else{
                if(suffixes.Contains( Path.GetExtension(scopeObj.assetPath) ))
                {
                    AddAsset(scopeObj.assetPath, retVal);
                }
            }
        }
    }


}