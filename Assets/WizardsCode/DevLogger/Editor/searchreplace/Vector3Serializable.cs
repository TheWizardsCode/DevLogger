using UnityEngine;

namespace sr
{
  [System.Serializable]
  public struct Vector3Serializable
  {
    public float x;
    public float y;
    public float z;

    public Vector3Serializable(float x0, float y0, float z0)
    {
      x = x0;
      y = y0;
      z = z0;
    }

    public Vector3 ToVector3()
    {
      return new Vector3(x,y,z);
    }

    public static Vector3Serializable FromVector3(Vector3 v3)
    {
      return new Vector3Serializable(v3.x, v3.y, v3.z);
    }
 
  }
}