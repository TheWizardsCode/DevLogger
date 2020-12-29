using UnityEngine;
using UnityEditor;
using System.IO;

namespace sr
{
  [System.Serializable]
  public class ReplaceItemVector3 : ReplaceItem<DynamicTypeVector3, Vector3Serializable>
  {
    public OptionalFloatField xField;
    public OptionalFloatField yField;
    public OptionalFloatField zField;

    public override void Draw()
    {
      GUILayout.BeginHorizontal();
      drawHeader();
      SRWindow.Instance.CVS();
      xField.Draw(showMoreOptions);
      yField.Draw(showMoreOptions);
      zField.Draw(showMoreOptions);
      SRWindow.Instance.CVE();

      if(xField.updated || yField.updated || zField.updated)
      {
        Vector3 updatedValue = new Vector3(xField.fieldValue, yField.fieldValue, zField.fieldValue);
        replaceValue = Vector3Serializable.FromVector3(updatedValue);
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

    }
  

    protected override void replace(SearchJob job, SerializedProperty prop, SearchResult result)
    {
      Vector3 v3 = prop.vector3Value;
      v3.x = xField.Replace(v3.x);
      v3.y = yField.Replace(v3.y);
      v3.z = zField.Replace(v3.z);
      prop.vector3Value = v3;
      result.replaceStrRep = v3.ToString();
    }

    protected override void Swap()
    { 
      xField.Swap(parent.xField);
      yField.Swap(parent.yField);
      zField.Swap(parent.zField);
      base.Swap();
    }

  }
 


}