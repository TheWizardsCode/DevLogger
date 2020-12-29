using UnityEngine;

namespace sr
{
  [System.Serializable]
  public struct Vector4Serializable
  {
    public float x;
    public float y;
    public float z;
    public float w;

    public Vector4Serializable(float x0, float y0, float z0, float w0)
    {
      x = x0;
      y = y0;
      z = z0;
      w = w0;
    }

    public Vector4 ToVector4()
    {
      return new Vector4(x,y,z,w);
    }

    public Quaternion ToQuaternion()
    {
      return new Quaternion(x,y,z,w);
    }

    public static Vector4Serializable FromVector4(Vector4 v4)
    {
      return new Vector4Serializable(v4.x, v4.y, v4.z, v4.w);
    }

  }
}