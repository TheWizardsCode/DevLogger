
namespace sr
{
  /**
   * Provides a serializable representation of a search.
   */

  [System.Serializable]
  public class SavedSearch
  {
    public string name;
    public SearchItemSet search;
    public SearchOptions options;

    public SavedSearch(string n, SearchItemSet s, SearchOptions o)
    {
      name = n;
      search = s;
      options = o;
    }

    public SavedSearch Clone()
    {
      SearchItemSet s = search.Clone();
      SearchOptions o = options.Copy();
      SavedSearch ss = new SavedSearch(name, s, o);
      return ss;
    }
  }


  
}