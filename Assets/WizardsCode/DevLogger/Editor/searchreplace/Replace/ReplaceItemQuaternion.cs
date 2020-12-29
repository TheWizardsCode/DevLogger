using UnityEngine;
using UnityEditor;
using System.IO;

namespace sr
{
  [System.Serializable]
  public class ReplaceItemQuaternion : ReplaceItem<DynamicTypeQuaternion, Vector4Serializable>
  {
    public OptionalFloatField xField;
    public OptionalFloatField yField;
    public OptionalFloatField zField;
    public OptionalFloatField wField;

    public override void Draw()
    {
      GUILayout.BeginHorizontal();
      drawHeader();
      SRWindow.Instance.CVS();

      if(!SRWindow.Instance.Compact())
      {
        GUILayout.BeginVertical();
        GUILayout.BeginHorizontal();
      }

      xField.Draw(showMoreOptions);
      yField.Draw(showMoreOptions);

      if(!SRWindow.Instance.Compact())
      {
        GUILayout.EndHorizontal();
        GUILayout.BeginHorizontal();
      }

      zField.Draw(showMoreOptions);
      wField.Draw(showMoreOptions);

      if(!SRWindow.Instance.Compact())
      {
        GUILayout.EndHorizontal();
        GUILayout.EndVertical();
      }

      SRWindow.Instance.CVE();

      if(xField.updated || yField.updated || zField.updated || wField.updated)
      {
        Vector4 updatedValue = new Vector4(xField.fieldValue, yField.fieldValue, zField.fieldValue, wField.fieldValue);
        replaceValue = Vector4Serializable.FromVector4(updatedValue);
        SRWindow.Instance.PersistCurrentSearch();
      }
      drawSwap();
      GUILayout.EndHorizontal();
    }

    public override void OnDeserialization()
    {
      if(xField == null)
      {
        xField = new OptionalFloatField("X", replaceValue.x);
      }
      if(yField == null )
      {
        yField = new OptionalFloatField("Y", replaceValue.y);
      }
      if(zField == null )
      {
        zField = new OptionalFloatField("Z", replaceValue.z);
      }
      if(wField == null )
      {
        wField = new OptionalFloatField("W", replaceValue.w);
      }
    }

    protected override void Swap()
    { 
      xField.Swap(parent.xField);
      yField.Swap(parent.yField);
      zField.Swap(parent.zField);
      wField.Swap(parent.wField);
      base.Swap();
    }

    protected override void replace(SearchJob job, SerializedProperty prop, SearchResult result)
    {
      Quaternion q2 = prop.quaternionValue;
      q2.x = xField.Replace(q2.x);
      q2.y = yField.Replace(q2.y);
      q2.z = zField.Replace(q2.z);
      q2.w = wField.Replace(q2.w);
      prop.quaternionValue = q2;
      result.replaceStrRep = q2.ToString();
    }
  }
 


}