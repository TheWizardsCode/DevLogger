using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

namespace sr
{
 
  /**
   * Provides a UI window for loading a saved search.
   */
  public class LoadSearchWindow : SearchWindow {

    public LoadSearchWindow()
    {
      titleContent = new GUIContent("Load Search");
    }



    List<SavedSearch>deletedSearches = new List<SavedSearch>();

    SavedSearch editingSearch = null;
    bool isRenaming = false;
    string newName = "";
    SavedSearch moveSearch = null;
    int moveSearchAmount = 0;
    bool searchesDirty = false;

    protected override void drawTop()
    {
      GUILayout.Space(10.0f);
      string labelStr = "";
      if(parent.savedSearches.searches.Count == 0)
      {
        labelStr = "You can save searches and use them again later. When you save searches, they will show up in the search dropdown. All search parameters will be saved.";
      }else{
        labelStr = "Select a search to edit. Searches remember all settings, including the scope and whether to search and replace.";
      }
      GUIContent content = new GUIContent(labelStr);
      float height = SRWindow.richTextStyle.CalcHeight(content, position.width);
      GUILayout.Label(content, SRWindow.richTextStyle, GUILayout.Height(height));

      GUILayout.Space(10.0f); 
      SRWindow.Divider();
    }

    protected override void drawItem(SavedSearch search, int index)
    {
      GUIStyle resultStyle = index % 2 == 0 ? SRWindow.resultStyle1 : SRWindow.resultStyle2;
      GUILayout.BeginHorizontal(resultStyle, GUILayout.Height(25.0f));
      // bool loadSearch = GUILayout.Button(new GUIContent("", "Load"), SRWindow.load);
      // if(loadSearch)
      // {
      //   parent.LoadSearch(search);
      //   Close();
      // }
      if(editingSearch == search)
      {

        if(isRenaming)
        {
          newName = GUILayout.TextField(newName);
          if(GUILayout.Button("OK", GUILayout.Width(50.0f)))
          {
            if(newName == "")
            {
              EditorUtility.DisplayDialog("Blank Name", "The name cannot be left blank.", "OK");
            }
            else if(search.name == newName)
            {
              //same name....cancel.
              isRenaming = false;
            }
            else if(searchNameExists(newName))
            {
              EditorUtility.DisplayDialog("Existing Search", "There is already a search named '"+newName+"'.", "OK");
            }
            else
            {
              editingSearch = null;
              search.name = newName;
              isRenaming = false;
              searchesDirty = true;
            }
          }
          if(GUILayout.Button("Cancel", GUILayout.Width(50.0f)))
          {
            isRenaming = false;
          }
        }else{
          GUILayout.Label(search.name, GUILayout.MaxWidth(400.0f));

          if(GUILayout.Button(new GUIContent("", "Rename"), SRWindow.rename))
          { 
            newName = search.name;
            isRenaming = true;
          }
          GUI.enabled = index != 0;
          if(GUILayout.Button(new GUIContent("", "Move Up"), SRWindow.upArrow))
          {
            moveSearch = search;
            moveSearchAmount = -1;
          }
          GUI.enabled = true;

          GUI.enabled = index != parent.savedSearches.searches.Count - 1;
          if(GUILayout.Button(new GUIContent("", "Move Down"), SRWindow.downArrow))
          {
            moveSearch = search;
            moveSearchAmount = 1;
          }
          GUI.enabled = true;
          if(GUILayout.Button(new GUIContent("", "Delete Search"), SRWindow.olMinusMinus))
          {
            bool confirm = EditorUtility.DisplayDialog("Delete?", "Are you sure you would like to delete the search '"+ search.name+"'?", "Delete", "Cancel");
            if(confirm)
            {
              deletedSearches.Add(search);
            }
          }
          if(GUILayout.Button("Done", GUILayout.Width(40.0f)))
          {
            editingSearch = null;
          }
        }
      }else{
        GUILayout.Label(search.name);
        if(GUILayout.Button(new GUIContent("", "More Options"), SRWindow.edit))
        {
          editingSearch = search;
        }
      }
      GUILayout.Width(5.0f);
      GUILayout.EndHorizontal();
    }

    protected override void cleanup()
    {
      if(deletedSearches.Count > 0)
      {
        foreach(SavedSearch search in deletedSearches)
        {
          parent.RemoveSearch(search);
        }
        deletedSearches.Clear();
        searchesDirty = true;
      }

      if(moveSearch != null)
      {
        parent.MoveSearch(moveSearch, moveSearchAmount);
        moveSearch = null;
        moveSearchAmount = 0;
        searchesDirty = true;
      }

      if(searchesDirty)
      {
        searchesDirty = false;
        parent.savedSearches.Persist();
      }

    }

  }

}