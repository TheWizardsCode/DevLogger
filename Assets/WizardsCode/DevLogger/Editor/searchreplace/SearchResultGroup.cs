using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Text;

namespace sr
{
  /**
   * Each search item has its own separate grouping of search results.
   */
  public class SearchResultGroup
  {
    public List<SearchResult> results = new List<SearchResult>(); //TODO: make it possible to search for > 100 things?

    public int startIndex = 0;
    public int endIndex = 0;

    public string groupName;

    public SearchResultGroup(SearchItem si)
    {
      //don't keep a ref to search item! Copy the data we are interested in to this object.

      groupName = si.GetDescription();
    }

    public void CopyToClipboard(StringBuilder sb)
    {
      sb.Append("Found "+results.Count+" for "+groupName + "\n");
      foreach(SearchResult result in results)
      {
        result.CopyToClipboard(sb);
      }
    }

    public void AddResult(SearchResult result)
    {
      results.Add(result);
      result.resultGroup = this;
      result.alternate = results.Count % 2 == 0;
    }

    public void Clear()
    {
      results.Clear();
    }

    public void Draw(int startIndex, int items)
    {
      // Debug.Log("[SearchResultGroup] "+groupName+ " startIndex:"+startIndex+" items:"+items + " results:"+results.Count);
      GUILayout.BeginVertical(SRWindow.resultsBox);
      EditorGUILayout.SelectableLabel("Found <b>"+results.Count+"</b> results for <b>"+groupName +"</b>.", SRWindow.richTextStyle);
      for(int i = startIndex; i < startIndex + items; i++)
      {
        SearchResult result = results[i];
        result.Draw();
      }

      GUILayout.EndVertical();
    }
  }
}