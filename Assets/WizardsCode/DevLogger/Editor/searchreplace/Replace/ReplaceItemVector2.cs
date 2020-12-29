using UnityEngine;
using UnityEditor;
using System.IO;

namespace sr
{
  [System.Serializable]
  public class ReplaceItemVector2 : ReplaceItem<DynamicTypeVector2, Vector2Serializable>
  {
    public OptionalFloatField xField;
    public OptionalFloatField yField;

    public override void Draw()
    {
      GUILayout.BeginHorizontal();
      drawHeader();
      SRWindow.Instance.CVS();
      xField.Draw(showMoreOptions);
      yField.Draw(showMoreOptions);
      SRWindow.Instance.CVE();

      if(xField.updated || yField.updated)
      {
        Vector2 updatedValue = new Vector2(xField.fieldValue, yField.fieldValue);
        replaceValue = Vector2Serializable.FromVector2(updatedValue);
        SRWindow.Instance.PersistCurrentSearch();
      }
      if(!SRWindow.Instance.Compact())
      {
        GUILayout.Space(SRWindow.approxWidth); //approximate.
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
    }

    protected override void replace(SearchJob job, SerializedProperty prop, SearchResult result)
    {
      Vector2 v2 = prop.vector2Value;
      v2.x = xField.Replace(v2.x);
      v2.y = yField.Replace(v2.y);
      prop.vector2Value = v2;
      result.replaceStrRep = v2.ToString();
    }

    protected override void Swap()
    { 
      xField.Swap(parent.xField);
      yField.Swap(parent.yField);
      base.Swap();
    }


  }
 


}