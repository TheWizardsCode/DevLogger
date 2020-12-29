using UnityEngine;
using UnityEditor;
#if UNITY_2018_3_OR_NEWER
using UnityEditor.Experimental.SceneManagement;
#endif
using UnityEditor.SceneManagement;

namespace sr
{
  /**
   * PathInfo is the data object behind a search result. It provides data that
   * can be displayed to the user without requiring that the (potentially large)
   * object is loaded into memory.
   */
  public class PathInfo
  {
    // The full path to the asset on disk.
    public string assetPath;
    
    // If this path info goes into an object hierarchy, this will contain that 
    // information in the format object:subobject->property
    public string objectPath;

    // An object path that is shorter for tight UI constraints.
    public string compactObjectPath;

    // The instance id of the game object that contains the search result.
    public int gameObjectID;

    // The instance id of the search result itself.
    public int objectID;

    // What's the prefab type?
    public PrefabTypes prefabType;

    // An objectID object for even more information!
    public ObjectID objID;

    public string FullPath()
    {
      return assetPath + objectPath;
    }

    /**
     * Provides functionality to create path information to an object: Folders->Objects->Monobehaviour->Property
     */
    public static PathInfo GetPathInfo(SerializedProperty prop, SearchAssetData assetData)
    {
      UnityEngine.Object obj = prop.serializedObject.targetObject;
      PathInfo pi = null;
      if(obj is Component)
      {
        Component c = (Component)obj;
        pi = InitWithComponent(c, c.gameObject, assetData);

        if(c is MonoBehaviour)
        {
          //Get the class information and attach it, then the property path
          MonoBehaviour m = (MonoBehaviour)c;
          MonoScript ms = MonoScript.FromMonoBehaviour(m);
          pi.objectPath = ToPath(m.gameObject, assetData) + "->" + ms.GetClass().ToString() + "."+prop.propertyPath;
        }else{
          // Just a component! GetType will do.
          pi.objectPath = ToPath(c.gameObject, assetData) + "->" + c.GetType().ToString() + "."+prop.propertyPath;
        }
        pi.compactObjectPath = c.gameObject.name+" ("+c.GetType().Name+")."+prop.propertyPath;

      }else if(obj is GameObject)
      {
        pi = GetPathInfo((GameObject)obj, assetData);
        pi.objectPath += "." + prop.propertyPath;
      }else if(obj is Material)
      {
        pi = GetPathInfo(obj, assetData);
        pi.objectPath = demanglePropertyPathForMaterial(prop);
        pi.compactObjectPath = obj.name + demanglePropertyPathForMaterial(prop);
      }else if(obj is UnityEngine.Object)
      {
        pi = GetPathInfo(obj, assetData);
        pi.objectPath = assetData.internalAssetPath + "." + prop.propertyPath;

      }else{
        Debug.Log("[PathInfo] Cannot guess object type:"+obj);
      }
      return pi;
    }

    // Currently used by DynamicTypeClass when matching classes, and also internally.
    // parent is passed in because its possible component can be null!
    public static PathInfo InitWithComponent(Component c, GameObject parent, SearchAssetData assetData)
    {
      PathInfo pi = new PathInfo();
      if(c != null)
      {
        pi.objID = new ObjectID(c);
        pi.objectID = c.GetInstanceID();
      }else{
        pi.objID = new ObjectID(parent);
      }
      pi.objID.obj = null;
      
      pi.gameObjectID = parent.GetInstanceID();
      pi.prefabType = PrefabUtil.GetPrefabType(parent);
      pi.assetPath = assetData.assetPath;
      pi.objectPath = ToPath(parent, assetData);
      pi.compactObjectPath = parent.name;

      return pi;
    }

    public static PathInfo GetPathInfo(GameObject go, SearchAssetData assetData)
    {
      PathInfo pi = new PathInfo();
      pi.objectID = go.GetInstanceID();
      pi.objID = new ObjectID(go);
      pi.objID.obj = null;


      pi.gameObjectID = pi.objectID;
      pi.assetPath = assetData.assetPath;
      pi.prefabType = PrefabUtil.GetPrefabType(go);
      pi.objectPath = ToPath(go, assetData);
      pi.compactObjectPath = go.name;
      return pi;
    }

    public static PathInfo GetPathInfo(UnityEngine.Object obj, SearchAssetData assetData)
    {
      if (!obj)
      {
        return null;
      }
      PathInfo pi = new PathInfo();
      pi.objectID = obj.GetInstanceID();
      pi.objID = new ObjectID(obj);
      pi.objID.obj = null;

      pi.gameObjectID = pi.objectID;
      pi.assetPath = assetData.assetPath;
      pi.prefabType = PrefabTypes.NotAPrefab;
      if(obj.name == string.Empty)
      {
        pi.objectPath = obj.GetType().ToString();
        pi.compactObjectPath = pi.objectPath;
      }else{
        pi.objectPath = obj.name;
        pi.compactObjectPath = pi.objectPath;
      }
      return pi;
    }

    public static PathInfo InitWithScriptableObject(ScriptableObject so, SearchAssetData assetData)
    {
      PathInfo pi = GetPathInfo(so, assetData);
      pi.objectPath = "";
      return pi;
    }

    // The paths for materials are kind of intense. Let's make this make 
    // sense!
    public static string demanglePropertyPathForMaterial(SerializedProperty prop)
    {
      //The format of items follows something like this:
      // m_SavedProperties.m_TexEnvs.Array.data[0].second.m_Texture
      // m_SavedProperties.m_TexEnvs.Array.data[0].first.name
      // So we can do some simple string manipulation and get rid of the second.blah and replace it with first.name, find that property, and use that value instead of 
      // the property path.
      string propPath = prop.propertyPath;
      int secondIndex = propPath.LastIndexOf("second");
      if(secondIndex > -1)
      {
        propPath = propPath.Substring(0, secondIndex);
        //Get the name.
        propPath += "first.name";
        SerializedProperty firstProp = prop.serializedObject.FindProperty(propPath);
        if(firstProp != null)
        {
          return "."+firstProp.stringValue;
        }else{
          return "."+prop.propertyPath;
        }
      }
      return "."+prop.propertyPath; //fallback.
    }

    protected static string ToPath(GameObject go, SearchAssetData assetData)
    {
      string retVal = "";
      Transform t = go.transform;
      while(t != null)
      {
        retVal = t.gameObject.name + retVal;
        t = t.parent;
        if(t != null)
        {
          retVal = "::" + retVal;
        }
      }
      return  "/" + retVal + assetData.internalAssetPath;
    }

    protected static string ToPath(UnityEngine.Object o, SearchAssetData assetData)
    {
      if(o is Component)
      {
        Component c = (Component) o;
        return ToPath(c.gameObject, assetData)+"."+c.GetType().Name;
      }
      if(o is GameObject)
      {
        GameObject go = (GameObject) o;
        return ToPath(go, assetData);
      }
      return "";
    }

  }
}