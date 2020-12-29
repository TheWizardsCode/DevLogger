using UnityEngine;
using UnityEditor;
using System;
using System.Text;

namespace sr
{
  /**
   * Subclass helper class for a search result that encapsulates a search result.
   * On the chopping block!
   */
  public class SearchResultProperty : SearchResult
  {

    public SearchResultProperty(SerializedProperty p, string sRep, SearchJob job)
    {
      strRep = sRep;
      pathInfo = PathInfo.GetPathInfo(p, job.assetData);
    }
  }
}