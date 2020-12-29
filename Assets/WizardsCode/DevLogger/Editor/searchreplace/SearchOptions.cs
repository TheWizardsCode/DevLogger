using UnityEngine;
namespace sr
{
  
  /**
   * Unlike the search item set, search options are configurable options
   * that the user may choose based upon context. IE: The user may request
   * case senstive, search vs. replace or other items.
   */
  [System.Serializable]
  public class SearchOptions
  {
    public SearchType searchType;

    public SearchOptions Copy()
    {
      SearchOptions retVal = new SearchOptions();
      retVal.searchType = searchType;
      return retVal;
    }

    public static SearchOptions PopulateFromDisk()
    {
      string path = Application.persistentDataPath+"/searchreplace/currentSearchOptions";
      SearchOptions sis = (SearchOptions)SerializationUtil.Deserialize(path);
      if(sis == null) 
      {
        return new SearchOptions();
      }else{
        return sis;
      }
    }
  }

}