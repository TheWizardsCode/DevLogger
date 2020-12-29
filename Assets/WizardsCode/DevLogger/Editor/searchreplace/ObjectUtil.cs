using UnityEngine;
using UnityEditor;
using System;
using System.IO;

namespace sr
{
  /**
   * Frequently we're given a UnityEngine.Object and we need to glean an object
   * type out of it somehow. These classes help with that.
   */
  public class ObjectUtil
  {
    public static UnityEngine.Object getScriptObjectFromObject(UnityEngine.Object obj)
    {
      if(obj == null)
      {
        return null;
      }
      if(obj is MonoBehaviour)
      {
        MonoBehaviour mb = (MonoBehaviour)obj;
        return MonoScript.FromMonoBehaviour(mb);
      }
      if(obj is ScriptableObject)
      {
        return MonoScript.FromScriptableObject((ScriptableObject)obj);
      }
      if(obj is Component)
      {
        return obj;
      }
      if(obj is MonoScript)
      {
        return obj;
      }
      return null;
    }

    public static Type getTypeFromObject(UnityEngine.Object obj)
    {
      if(obj == null)
      {
        return null;
      }
      if(obj is MonoScript)
      {
        MonoScript ms = (MonoScript)obj;
        return ms.GetClass();
      }
      if(obj is Component)
      {
        return obj.GetType();
      }
      return null;
    }

    public static bool IsDirectory(UnityEngine.Object obj)
    {
      string assetPath = AssetDatabase.GetAssetPath(obj);
      bool isDirectory = false;
      if(obj is UnityEditor.DefaultAsset)
      {
        // Let's not hit the file system unless ABSOLUTELY necessary.
        isDirectory = Directory.Exists(Application.dataPath.Replace("Assets", "") + assetPath);
      }else{
        isDirectory = false;
      }
      return isDirectory;
    }

    public static bool ValidateAndAssign(UnityEngine.Object newObj, ObjectID objID, PropertyPopupData searchProperty, ref InitializationContext initializationContext)
    {
      // First we need to attempt validation, or find the 'real' object we are 
      // interested in.
      UnityEngine.Object contextObj = newObj;
      bool objectValid = true;
      // If the object is a 'scene' object, let's get the prefab object if it
      // exists.
      if(newObj != null)
      {
        if(newObj.GetInstanceID() < 0)
        {
          // Debug.Log("[SearchItemProperty] finding prefab.");
#if UNITY_2018_2_OR_NEWER
          UnityEngine.Object prefab = PrefabUtility.GetCorrespondingObjectFromSource(newObj);
#else
          UnityEngine.Object prefab = PrefabUtility.GetPrefabParent(newObj);
#endif
          if(prefab != null)
          {
            newObj = prefab;
          }
        }
        PrefabTypes newObjPrefabType = PrefabUtil.GetPrefabType(newObj);
        if(newObjPrefabType == PrefabTypes.NotAPrefab)
        {
          if(newObj is Component)
          {
            if(newObj is MonoBehaviour)
            {
              MonoScript script = MonoScript.FromMonoBehaviour((MonoBehaviour)newObj);
              if(script != null) 
              {
                newObj = script;
              }
            }
          }
        }
      }

      if(newObj is MonoScript)
      {
        MonoScript m = (MonoScript) newObj;
        if(typeof(MonoBehaviour).IsAssignableFrom(m.GetClass()) || typeof(ScriptableObject).IsAssignableFrom(m.GetClass()))
        {
          // Debug.Log("[SearchItemProperty] Valid monobehaviour"+m.GetClass());
        }else{
          Debug.LogWarning  ("[Search And Replace] The object given does not contain a script.");
          objectValid = false;
        }
      }

      //Ok, validation is complete, let's process the new object.
      if(objectValid)
      {
        objID.SetObject(newObj);
        searchProperty.SetType(objID.obj);
        initializationContext = new InitializationContext(searchProperty.fieldData, contextObj);
        initializationContext.forceUpdate = true;
        SRWindow.Instance.PersistCurrentSearch();  
        return true;
      }
      return false;
      
    }


    // utility function to get a game object from this object if possible.
    public static GameObject GetGameObject(UnityEngine.Object obj)
    {
      if(obj is GameObject)
      {
        return (GameObject)obj;
      }
      if(obj is Component)
      {
        return ((Component)obj).gameObject;
      }
      return null;
    }


  }
}
