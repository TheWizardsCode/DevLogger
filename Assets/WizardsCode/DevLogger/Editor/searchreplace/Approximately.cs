using UnityEngine;

namespace sr
{
  
  public static class Approximately
  {
    public static bool Quat(Quaternion q1, Quaternion q2)
    {
      return 
      Mathf.Approximately(q1.x, q2.x) &&
      Mathf.Approximately(q1.y, q2.y) &&
      Mathf.Approximately(q1.z, q2.z) &&
      Mathf.Approximately(q1.w, q2.w);
    }

  }
}