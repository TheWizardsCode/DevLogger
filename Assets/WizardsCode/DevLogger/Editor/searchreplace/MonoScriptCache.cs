using UnityEngine;
using UnityEditor;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;

namespace sr
{
  /**
   * There is no method to get all monoscripts except via
   * FindObjectsOfTypeAll. We cache this information into a 
   * table and look it up later.
   * This is only called when we need it (aka we can't find something) but we'll
   * probably need it frequently.
   */
  public static class MonoScriptCache
  {
    private static Dictionary<long, MonoScript> scriptHash = null;


    public static MonoScript getScript(long id)
    {
      if(scriptHash == null)
      {
        // Debug.Log("[MonoScriptCache] making hash of monoscripts to find monoscript ref.");
        scriptHash = new Dictionary<long, MonoScript>();
        PropertyInfo debugModeInspectorThing = typeof(SerializedObject).GetProperty("inspectorMode", BindingFlags.NonPublic | BindingFlags.Instance);
        MonoScript[] scripts = (MonoScript[])Resources.FindObjectsOfTypeAll( typeof( MonoScript ) );
        foreach(MonoScript script in scripts)
        {
          System.Type c = script.GetClass();
          if(c != null)
          { 
            // This simple string search reduces search time by 4x.
            if(c.ToString().StartsWith("UnityEditor"))
            {
              continue;
            }
            SerializedObject so = new SerializedObject(script);
            debugModeInspectorThing.SetValue(so, InspectorMode.Debug, null);
            SerializedProperty serializedProperty = so.FindProperty("m_LocalIdentfierInFile");
            scriptHash[serializedProperty.longValue] = script;
            // Debug.Log("[ObjectID] script:"+script.GetClass());
          }
        }
      }

      MonoScript retVal = null;
      scriptHash.TryGetValue(id, out retVal);
      return retVal;
    }
  }
}