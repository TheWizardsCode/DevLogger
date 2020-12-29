using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace sr
{
  /**
   * Provides a serializable method to find a path to a game object. Many objects
   * are easy to find, others are more complicated to find, such as game objects
   * in a scene. This uses some logic and some brute force tactics to glean
   * the path of an object.
   */
  [System.Serializable]
  public class PathData
  {
    List<string> names = new List<string>();
    List<int> indices = new List<int>();

  public void Add(int index, string name)
  {
    names.Insert(0, name);
    indices.Insert(0, index);
  }

  public override string ToString()
  {
    StringBuilder sb = new StringBuilder();
    sb.Append("[PathData ");
    for(int i=0;i<indices.Count;i++)
    {
      sb.Append(names[i]);
      sb.Append("#"+indices[i]);
      if(i != indices.Count - 1)
      {
        sb.Append(",");
      }
    }
    sb.Append("]");
    return sb.ToString();
  }


  public GameObject GetObject(GameObject root)
  {
    return GetObject(new List<Transform>(){ root.transform });
  }

  public GameObject GetObject(List<Transform> transforms)
  {

    if(indices.Count == 0)
    {
      return null;
    }
    List<Transform> siblings = transforms;
    Transform t = null;
    for(int i = 0; i < indices.Count; i++)
    {
      int index = indices[i];
      string name = names[i];
      t = getTransform(siblings, index, name);
      if(t == null)
      {
        //something bad happened!
        return null;
      }
      //get children as list.
      siblings = new List<Transform>();
      foreach(Transform childTransform in t)
      {
        siblings.Add(childTransform);
      }
    }
    return t.gameObject;
  }

  Transform getTransform(List<Transform> transforms, int index, string name)
  {
      
    for(int i = 0; i < transforms.Count; i++)
    {
      Transform t = transforms[i];
      int siblingIndex = t.GetSiblingIndex();
      if(t.name == name && siblingIndex == index)
      {
        return t;
      }
    }
    return null;
  }


  //Provides a path in gameobject.find() syntax
  public static PathData ToPathData(UnityEngine.Object o)
  {
    GameObject go = null;
    if(o is Component)
    {
      go = ((Component)o).gameObject;
    }
    if(o is GameObject)
    {
      go = (GameObject) o;
    }
    PathData retVal = new PathData();

    // In the case that someone has created a ScriptableObject dynamically in a 
    // scene. Yes this can happen. ScriptableOFactory does this in order for us
    // to test against it.
    if(o is ScriptableObject)
    {
      return retVal;
    }

    Transform t = go.transform;
    while(t != null)
    {
      retVal.Add(t.GetSiblingIndex(), t.gameObject.name);
      t = t.parent;
    }
    return retVal;
  }

  }


}