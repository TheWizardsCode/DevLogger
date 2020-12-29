using UnityEngine;
using UnityEngine.Rendering;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Reflection;
using System.Linq;

namespace sr
{
  /***
   * Provides a somewhat hacky but workable way to manage search/replace parameters
   * for replacable properties.
   */
  public class AssetImporterMethodUtil
  {

    static Dictionary<string, string> propertyMethodHash = new Dictionary<string, string>()
    {
      {"m_TextureSettings.m_FilterMode" , "set_filterMode"},
      {"m_TextureSettings.m_WrapMode" , "set_wrapMode"},
      {"m_IsReadable" , "set_isReadable"},
      {"m_LoadInBackground" , "set_loadInBackground"},
      {"m_PreloadAudioData" , "set_preloadAudioData"},

    };

    public static bool HasImporterMethod(string propertyPath)
    {
      return propertyMethodHash.ContainsKey(propertyPath);
    }

    public static MethodInfo GetMethodForProperty(AssetImporter importer, SerializedProperty prop)
    {
      Type t = importer.GetType();
      string methodName = null;
      if(propertyMethodHash.TryGetValue(prop.propertyPath, out methodName))
      {
        MethodInfo m = t.GetMethod(methodName);
        return m;
      }
      return null;
    }
  }
}
