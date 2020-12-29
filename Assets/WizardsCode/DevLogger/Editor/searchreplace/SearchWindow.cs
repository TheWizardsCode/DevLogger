using UnityEngine;
using UnityEditor;
namespace sr
{
 
  /**
   * The base class for the Load Search and Save Search windows.
   */
  public class SearchWindow : EditorWindow {

    public SearchWindow()
    {
      titleContent = new GUIContent("Search");
    }

    public SRWindow parent;

    Vector2 scrollPosition;

    protected virtual void drawTop()
    {
    }

    protected virtual void drawItem(SavedSearch search, int index)
    {
    }

    protected virtual void cleanup()
    {
    }

    protected virtual void drawBottom()
    {
      if(GUILayout.Button("Close"))
      {
        Close();
      }
    }

    void OnGUI()
    {
      drawTop();

      scrollPosition = GUILayout.BeginScrollView(scrollPosition);
      
      for(int i=0;i<parent.savedSearches.searches.Count;i++)
      {
        SavedSearch search = parent.savedSearches.searches[i];
        drawItem(search, i);
      }

      GUILayout.EndScrollView();
      drawBottom();
      cleanup();
    }

    protected bool searchNameExists(string searchName)
    {
      foreach(SavedSearch search in parent.savedSearches.searches)
      {
        if(search.name == searchName){
          return true;
        }
      }
      return false;
    }

  }

}