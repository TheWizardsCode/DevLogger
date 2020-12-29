using UnityEngine;
using UnityEditor;
namespace sr
{
 
  /**
   * A UI for saving a user's search.
   */
  public class SaveSearchWindow : SearchWindow {

    public SaveSearchWindow()
    {
      titleContent = new GUIContent("Save Search");
    }

    public string searchName = "";
    string errorText = "";

    protected override void drawTop()
    {
      GUILayout.Space(10.0f);
      string labelStr = "Name your search below and it will show up in the dropdown. Searches remember all settings, including the scope and whether to search and replace.";
      GUIContent content = new GUIContent(labelStr);
      float height = SRWindow.richTextStyle.CalcHeight(content, position.width);
      GUILayout.Label(content, SRWindow.richTextStyle, GUILayout.Height(height));

      string newName = EditorGUILayout.TextField("Save New Search", searchName);
      if(errorText != "")
      {
        GUILayout.Label(errorText);
      }
      if(newName != searchName)
      {
        searchName = newName;
      }
      if(GUILayout.Button("Save"))
      {
        if(searchName != "")
        {
          if(searchNameExists(searchName))
          {
            bool confirm = EditorUtility.DisplayDialog("Overwrite?", "Instead of saving a new search, this will overwrite the search '"+ searchName+"'. Are you sure?", "Overwrite", "Cancel");
            if(confirm)
            {
              saveAndClose(searchName, parent.currentSearch, parent.searchOptions);
            }
          }else{
            saveAndClose(searchName, parent.currentSearch, parent.searchOptions);
          }
        }else{
          errorText = "You must input a valid name.";
        }
      }
      GUILayout.Space(10.0f);
      SRWindow.Divider();
      GUILayout.Label("Overwrite Existing Search");
    }

    void saveAndClose(string name, SearchItemSet set, SearchOptions options)
    {
      SavedSearch ss = new SavedSearch(name, parent.currentSearch.Clone(), parent.searchOptions.Copy());
      parent.SaveSearch(ss);
      Close();
    }

    SavedSearch searchToOverwrite = null;
    protected override void drawItem(SavedSearch search, int index)
    {
      GUIStyle resultStyle = index % 2 == 0 ? SRWindow.resultStyle1 : SRWindow.resultStyle2;
      GUILayout.BeginHorizontal(resultStyle, GUILayout.Height(25.0f));
      if(GUILayout.Button("Overwrite", GUILayout.Width(100.0f)))
      {
        bool confirm = EditorUtility.DisplayDialog("Overwrite?", "Instead of saving a new search, this will overwrite the search '"+ search.name+"'. Are you sure?", "Overwrite", "Cancel");
        if(confirm)
        {
          searchToOverwrite = search;
        }
      }
      GUILayout.Label(search.name);
      GUILayout.EndHorizontal();
    }

    protected override void cleanup()
    {
      if(searchToOverwrite != null)
      {
        parent.savedSearches.searches.Remove(searchToOverwrite);
        saveAndClose(searchToOverwrite.name, parent.currentSearch, parent.searchOptions);
        searchToOverwrite = null;
      }
      
    }
  }  
}