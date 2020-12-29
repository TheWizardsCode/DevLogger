using UnityEngine;

namespace sr
{
  [System.Serializable]
  public struct Vector2Serializable
  {
    public float x;
    public float y;

    public Vector2Serializable(float x0, float y0)
    {
      x = x0;
      y = y0;
    }

    public Vector2 ToVector2()
    {
      return new Vector2(x,y);
    }

    public static Vector2Serializable FromVector2(Vector2 v2)
    {
      return new Vector2Serializable(v2.x, v2.y);
    }
 
  }
}