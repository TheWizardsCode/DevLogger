using UnityEngine;

namespace sr
{
  [System.Serializable]
  public class ColorSerializable
  {
    public float r = 1.0f;
    public float g = 1.0f;
    public float b = 1.0f;
    public float a = 1.0f;

    public ColorSerializable(float r0, float g0, float b0, float a0)
    {
      r = r0;
      g = g0;
      b = b0;
      a = a0;
    }

    public ColorSerializable(ColorSerializable other)
    {
      r = other.r;
      g = other.g;
      b = other.b;
      a = other.a;
    }

    public Color ToColor()
    {
      return new Color(r,g,b,a);
    }

    public static ColorSerializable FromColor(Color color)
    {
      return new ColorSerializable(color.r, color.g, color.b, color.a);
    }
 
  }
}