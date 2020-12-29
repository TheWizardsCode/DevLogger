using UnityEngine;
using UnityEditor;
using System.IO;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
#if UNITY_2018_3_OR_NEWER
using UnityEditor.Experimental.SceneManagement;
#endif
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

namespace sr
{
  /**
   * This class attempts to create a serialized representation of an 'object'.
   * An object could exist within a scene, be a sub-object in a prefab, a 
   * top-level object in a prefab,a MonoScript, ScriptableObject, material,
   *  texture, etc. Because Unity doesn't provide a global method for defining how to 
   * serialize and re-gain access to an object we do our best to get around it
   * with this class.
   *
   * This class also does its best to *not load* the object unless necessary, in
   * order to keep memory usage manageable with large search results. The 
   * unfortunately complicated logic does its best to keep cpu and memory low with
   * a number of optimizations.
   */
  [System.Serializable]
  public class ObjectID
  {
    static PropertyInfo debugModeInspectorThing;

    public long localID;
    // If this is a scene object, then this 'guid' will be the scene path!
    // If it is an unsaved scene then this will be an empty string.
    public string guid;

    public PathData localPath;

    [System.NonSerialized]
    public bool isSceneObject = false;
    
    [System.NonSerialized]
    public bool isPrefabStageObject = false;
    
    [System.NonSerialized]
    public bool isDirectory = false;

    [System.NonSerialized]
    public UnityEngine.Object obj;

    public static ObjectID none = new ObjectID();

    [System.NonSerialized]
    public string assetPath = "";

    //Whether or not we need a specific AssetImporter to import this object.
    [System.NonSerialized]
    public bool assetRequiresImporter = false;

    public ObjectID()
    {
      localID = 0;
      guid = "";
    }

    // NOTE: this will set obj to NULL immediately for memory reasons.
    public ObjectID(SerializedProperty prop)
    {
      SetObject(prop.serializedObject.targetObject);
      obj = null;
    }

    public ObjectID(UnityEngine.Object o)
    {
      SetObject(o);
    }

    public ObjectID Copy()
    {
      ObjectID retVal = new ObjectID(obj);
      return retVal;
    }

    public void OnDeserialization()
    {
      // Debug.Log("[ObjectID] guid: "+guid+" ---  id:"+localID);
      GetObject();
      parseIfSceneObject();
      parseIfDirectory();
    }

    public void Clear()
    {
      obj = null;
    }

    void setRequiresImporter()
    {
      if(obj == null)
      {
        assetRequiresImporter = false;
        return;
      }
      assetRequiresImporter = obj is Texture || obj is AudioClip;
    }

    public void SetObject(UnityEngine.Object o)
    {
      obj = o;
      setRequiresImporter();
      if(obj == null)
      {
        localID = 0;
        guid = string.Empty;
        localPath = null;

        return;
      }
      initDebugMode();
      // Debug.Log("[ObjectID] Object:"+o.GetType());
      SerializedObject so = new SerializedObject(o);
      debugModeInspectorThing.SetValue(so, InspectorMode.Debug, null);
      SerializedProperty serializedProperty = so.FindProperty("m_LocalIdentfierInFile");
      if(serializedProperty != null)
      {
        localID = serializedProperty.longValue;
        // this will be an empty string for scene objects, including disconnected prefab instances.
        assetPath = AssetDatabase.GetAssetPath(obj);
        guid = AssetDatabase.AssetPathToGUID(assetPath); // guid will be empty for scene objects as well.
        // Debug.Log("[ObjectID] path for object:"+ obj + " '" + assetPath + "'", obj);
        // Debug.Log("[ObjectID] guid:"+guid);
        parseIfSceneObject();
        parseIfDirectory();
        // Debug.Log("[ObjectID] "+ o.name+" asset:"+obj+" :"+guid+" ---  "+localID);
      }else{
        Debug.LogWarning("Could not get m_LocalIdentfierInFile on " + o, o);

      }

    }

    void parseIfSceneObject()
    {
      if(obj != null)
      {
        // Debug.Log("[ObjectID] guid: " + guid + " for  " + obj);
        if(guid == "")
        {
          GameObject go = null;
          if(obj is Component)
          {
            Component c = (Component)obj;
            go = c.gameObject;
          }else{
           go = (GameObject)obj;
          }
          isSceneObject = go.scene.path.EndsWith(".unity");
          if(isSceneObject)
          {
            guid = go.scene.path;
          }else{
            guid = AssetDatabase.AssetPathToGUID(go.scene.path);
#if UNITY_2018_3_OR_NEWER
            PrefabStage stage = PrefabStageUtility.GetPrefabStage(go);
            if(stage != null)
            {
              isPrefabStageObject = true;
            }
#endif
          }
          localPath = PathData.ToPathData(obj);
        }else
        if(guid == SceneUtil.GuidPathForActiveScene())
        {
          isSceneObject = true;
          localPath = PathData.ToPathData(obj);
        }else{
          isSceneObject = false;
          localPath = null;
        }
      }else{
        isSceneObject = false;
        localPath = null;
      }
    }

    void parseIfDirectory()
    {
      if(isSceneObject)
      {
        isDirectory = false;
      }else{
        if(obj == null)
        {
          isDirectory = false;
        }else{
          if(obj is UnityEditor.DefaultAsset)
          {
            // Let's not hit the file system unless ABSOLUTELY necessary.
            isDirectory = Directory.Exists(Application.dataPath.Replace("Assets", "") + assetPath);
          }else{
            isDirectory = false;
          }
        }
      }
    }


    /*
     * Called upon deserialization to initialize the object.
     */
    void GetObject()
    {

      if(obj == null)
      {
        //At this point we don't know if this is a scene object,
        bool guidIsScene = guid == SceneUtil.GuidPathForActiveScene();
        if(guidIsScene)
        {
          // Debug.Log("[ObjectID] looking in scene:"+guid);
          obj = searchForObjectInScene();
        } else{
          // Debug.Log("[ObjectID] not a scene object");
        }
        UnityEngine.Object o = null;
        // attempt to retrieve the object.
        assetPath = AssetDatabase.GUIDToAssetPath(guid);
        // Debug.Log("[ObjectID] looking at : "+assetPath);
        if(assetPath == "Resources/unity_builtin_extra")
        { 
          // Debug.Log("[ObjectID] internal resource;"+guid+" : " + localID);
          o = EditorUtility.InstanceIDToObject((int)localID);
          // Debug.Log("[ObjectID] we found internally:"+o);
        }else{
          o = AssetDatabase.LoadAssetAtPath(assetPath, typeof(UnityEngine.Object));
        }
        //search for local object.
        if(o == null)
        {
          return; //aka 'null'
        }
        obj = searchForLocalObject(o);
        if(obj == null)
        {
          // Check our monoscript cache.
          // Debug.Log("[ObjectID] couldn't find object....checking monoscript cache");
          obj = MonoScriptCache.getScript(localID);
        }
          
        // Debug.Log("[ObjectID] obj:"+obj);
        if(obj == null)
        {
          localID = 0;
          guid = "";
          // wipe it.
        }
        setRequiresImporter();

      }
    }
    public UnityEngine.Object searchForObjectInScene(Scene scene)
    {
      if(!scene.IsValid())
      {
        Debug.Log("[ObjectID] WARN: Scene to search:" + scene.path + " is invalid!");
        return null;
      }
      return searchRootObjects(scene.GetRootGameObjects());
    }

    public UnityEngine.Object searchForObjectInScene()
    {
      Scene scene;
#if UNITY_2018_3_OR_NEWER
      if (SceneUtil.IsSceneStage())
      {
        scene = PrefabStageUtility.GetCurrentPrefabStage().scene;
        return searchForObjectInScene(scene);
      }
#endif
      scene = EditorSceneManager.GetActiveScene();
      return searchForObjectInScene(scene);
    }

    UnityEngine.Object searchRootObjects(IEnumerable rootObjects)
    {
      //search using pathData.
      if(localPath != null)
      {
        List<Transform> rootTransforms = new List<Transform>();
        foreach(GameObject go in rootObjects)
        {
          rootTransforms.Add(go.transform);
        }
        return localPath.GetObject(rootTransforms);
      }

      foreach(GameObject go in rootObjects)
      {
        UnityEngine.Object o = searchForLocalObject(go);
        if(o != null)
        {
          return o;
        }
      }
      return null;
    }

    public UnityEngine.Object searchForLocalObject(UnityEngine.Object o)
    {
      if(localID == 0)
      {
        // Debug.Log("[ObjectID] local id == 0:"+o);
        return null;
      }
      // Debug.Log("[ObjectID] localID:"+localID);
      initDebugMode();
      SerializedObject so = new SerializedObject(o);
      debugModeInspectorThing.SetValue(so, InspectorMode.Debug, null);
      SerializedProperty serializedProperty = so.FindProperty("m_LocalIdentfierInFile");
      // Debug.Log("[ObjectID] looking at:"+serializedProperty.longValue);
      if(serializedProperty.longValue == localID)
      {
        return so.targetObject;
      }
      if(o is GameObject)
      {
        GameObject go = (GameObject) o;
        Component[] components = go.GetComponents<Component>();
        foreach(Component c in components)
        {
          if(c != null)
          {
            so = new SerializedObject(c);
            // Debug.Log("[ObjectID] looking at component:"+c);
            debugModeInspectorThing.SetValue(so, InspectorMode.Debug, null);
            serializedProperty = so.FindProperty("m_LocalIdentfierInFile");
            if(serializedProperty.longValue == localID)
            {
              // Debug.Log("[ObjectID] found thing!");
              return so.targetObject;
            }
          }
        }
        foreach(Transform child in go.transform)
        {
          UnityEngine.Object childObj = searchForLocalObject(child.gameObject);
          if(childObj != null)
          {
            return childObj;
          }
        }
      }
      // Debug.Log("[ObjectID] bottomed out...");
      return null;
    }
 
    void initDebugMode()
    {
      if(debugModeInspectorThing == null)
      {
        debugModeInspectorThing = typeof(SerializedObject).GetProperty("inspectorMode", BindingFlags.NonPublic | BindingFlags.Instance);
      }
    }

    // utility function to get a game object from this object if possible.
    public GameObject GetGameObject()
    {
      return ObjectUtil.GetGameObject(obj);
    }

  }
}