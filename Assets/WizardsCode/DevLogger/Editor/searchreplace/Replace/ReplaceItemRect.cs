using UnityEngine;
using UnityEditor;
using System.IO;

namespace sr
{
  [System.Serializable]
  public class ReplaceItemRect : ReplaceItem<DynamicTypeRect, RectSerializable>
  {

    public OptionalFloatField xField;
    public OptionalFloatField yField;
    public OptionalFloatField wField;
    public OptionalFloatField hField;

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

      wField.Draw(showMoreOptions);
      hField.Draw(showMoreOptions);

      if(!SRWindow.Instance.Compact())
      {
        GUILayout.EndHorizontal();
        GUILayout.EndVertical();
      }

      SRWindow.Instance.CVE();

      if(xField.updated || yField.updated || wField.updated || hField.updated)
      {
        Rect updatedValue = new Rect(xField.fieldValue, yField.fieldValue, wField.fieldValue, hField.fieldValue);
        replaceValue = RectSerializable.FromRect(updatedValue);
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
      if(wField == null )
      {
        wField = new OptionalFloatField("W", replaceValue.width);
      }
      if(hField == null )
      {
        hField = new OptionalFloatField("H", replaceValue.height);
      }

    }

    protected override void Swap()
    { 
      xField.Swap(parent.xField);
      yField.Swap(parent.yField);
      wField.Swap(parent.wField);
      hField.Swap(parent.hField);
      base.Swap();
    }

    protected override void replace(SearchJob job, SerializedProperty prop, SearchResult result)
    {
      Rect rect = prop.rectValue;
      rect.x = xField.Replace(rect.x);
      rect.y = yField.Replace(rect.y);
      rect.width = wField.Replace(rect.width);
      rect.height = hField.Replace(rect.height);
      prop.rectValue = rect;
      result.replaceStrRep = rect.ToString();
    }
  }
 


}