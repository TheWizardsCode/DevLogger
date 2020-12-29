using UnityEngine;
using UnityEditor;
using System.IO;

namespace sr
{
  [System.Serializable]
  public class ReplaceItemVector4 : ReplaceItem<DynamicTypeVector4, Vector4Serializable>
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
  

    protected override void replace(SearchJob job, SerializedProperty prop, SearchResult result)
    {
      Vector4 v4 = prop.vector4Value;
      v4.x = xField.Replace(v4.x);
      v4.y = yField.Replace(v4.y);
      v4.z = zField.Replace(v4.z);
      v4.w = wField.Replace(v4.w);
      prop.vector4Value = v4;
      result.replaceStrRep = v4.ToString();
    }

    protected override void Swap()
    { 
      xField.Swap(parent.xField);
      yField.Swap(parent.yField);
      zField.Swap(parent.zField);
      wField.Swap(parent.wField);
      base.Swap();
    }

  }
 


}