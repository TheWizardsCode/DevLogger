using UnityEngine;

namespace sr
{
  [System.Serializable]
  public struct RectSerializable
  {
    public float x;
    public float y;
    public float width;
    public float height;

    public RectSerializable(float x0, float y0, float w0, float h0)
    {
      x = x0;
      y = y0;
      width = w0;
      height = h0;
    }

    public Rect ToRect()
    {
      return new Rect(x,y,width,height);
    }

    public static RectSerializable FromRect(Rect rect)
    {
      return new RectSerializable(rect.x, rect.y, rect.width, rect.height);
    }
 
  }
}