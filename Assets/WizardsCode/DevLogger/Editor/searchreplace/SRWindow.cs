using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace sr
{
 
  /**
   * The Project Search & Replace editor window. Works as a singleton to provide
   * access to useful functions and styles.
   */
  public class SRWindow : EditorWindow {
    [MenuItem ("Window/Search + Replace")]
    static void Init () 
    {
      // Get existing open window or if none, make a new one:
      SRWindow window = (SRWindow)EditorWindow.GetWindow (typeof (SRWindow)); 
      Texture2D icon = (Texture2D) findObject("com.eh.searchandreplace.icon");
      window.titleContent = new GUIContent("Search", icon );
      window.Show();
    }

    //Views
    PopupData searchType;
    bool replaceMode = false;
    int searchForIndex = 0;

    SearchWindow childWindow;

    public static GUIStyle richTextStyle;
    public static GUIStyle searchBox;
    public static GUIStyle searchBoxDragHighlight;
    public static GUIStyle searchInnerDepthBox;
    public static GUIStyle resultsBox;
    public static GUIStyle divider;
    public static GUIStyle dragDivider;
    public static Texture2D dragDividerNormal;
    public static GUIStyle plusButton;
    public static GUIStyle resultStyle1;
    public static GUIStyle resultStyle2;
    public static GUIStyle resultStyle3;
    public static GUIStyle selectedStyle;
    public static GUIStyle toolbarButton;
    public static GUIStyle optionsToggle;
    public static GUIStyle swapToggle;
    public static GUIStyle errorStyle;
    public static GUIStyle olPlusPlus;
    public static GUIStyle olMinusMinus;
    public static Texture2D prefabIcon;
    public static Texture2D scriptIcon;
    public static Texture2D goIcon;
    public static Texture2D materialIcon;
    public static Texture2D sceneAssetIcon;
    public static Texture2D viewToolZoomIcon;
    public static GUIStyle downArrow;
    public static GUIStyle upArrow;
    public static GUIStyle edit;
    public static GUIStyle save;
    public static GUIStyle rename;
    public static GUIStyle load;
    public static GUIStyle loadSearch;
    public static GUIContent thumb;
    public static float compactLabelWidth = 60;
    public static float compactLabelWidthEP = 64;//extra padding, for advanced options
    public static float compactLabelWidthNP = 56;//no padding
    public static float approxWidth = 100;
    public static float approxLabelWidth = 85;
    public static float boxPad = 30;


      public static string productName = "Project Search && Replace";
    public static string fullVersionURL = "https://www.assetstore.unity3d.com/#!/content/55680";
    
    public static string supportURL = "http://support.enemyhideout.com/searchandreplace/";

    const float compactWidth = 400;

    //View + Data
    public SearchItemSet currentSearch;
    List<SearchItemSet> searches;
    public SavedSearches savedSearches;
    HashSet<SearchItemSet> searchesToPersist;

    // Data
    public SearchOptions searchOptions;

    //Last run search.
    public SearchJob searchJob = null;

    //Where we are in the top divider view.
    Vector2 scrollPosition;

    //Whether or now we are currently resizing the divider.
    bool resizing = false;

    //The initial divider location
    float dividePercentY = 0.75f;

    bool _initialized = false;

    public static SRWindow Instance;

    string[] searchForOptionLabels = new string[]{Keys.GlobalLabel, Keys.PropertyLabel, Keys.InstancesLabel };
    string[] searchForOptions = new string[]{Keys.Global, Keys.Property, Keys.Instances };

    // Dirty flag that controls whether or not we want to persist to disk.
    // There are a couple reasons we do this. First it stops us from potentially
    // serializing multiple times a tick. Also there are certain operations that
    // unity doesn't like to do in ctors etc and so we must delay these to a 
    // point where we are 'safe' (not thread-safe, just....safe i guess).
    bool persist = false;

    public SRWindow()
    {
      _initialized = false;
    }

    void OnEnable()
    {
      _initialized = false;
    }

    void init()
    {
      if(searchOptions == null)
      {
        _initialized = false;
      }
      if(!_initialized)
      {
        _initialized = true;
        Instance = this;
        searchType = new PopupData(0, new string[]{Keys.search, Keys.searchAndReplace}, Keys.prefSearchType, "");
        searchType.boxPad = 10;


        int replacePref = EditorPrefs.GetInt(Keys.prefSearchType, 0);
        replaceMode = replacePref == 0 ? false : true;

        searchForIndex = EditorPrefs.GetInt(Keys.prefSearchFor, 0);

        //This throws errors if done in Ctor! yay.
        searches = new List<SearchItemSet>();
        searchesToPersist = new HashSet<SearchItemSet>();
        foreach(string searchKey in searchForOptions)
        {
          SearchItemSet s = SearchItemSet.PopulateFromDisk(searchKey);
          searches.Add(s);
        }
        SearchItemSet savedSearch = SearchItemSet.PopulateFromDisk(Keys.SavedSearch);
        searches.Add(savedSearch);

        searchOptions = SearchOptions.PopulateFromDisk();
        searchOptions.searchType = replaceMode ? SearchType.SearchAndReplace : SearchType.Search;
        savedSearches = SavedSearches.PopulateFromDisk();

        // now that we've loaded all potential searches, which one has the user
        // selected?
        if(searchForIndex < 3)
        {
          currentSearch = searches[searchForIndex];
        }else{
          // If the index is above, then we have a saved search selected. The fourth
          // sis is the currently loaded saved search.
          currentSearch = searches[3];
        }

        //EditorStyles does not exist in Constructor??
        richTextStyle = new GUIStyle(EditorStyles.label);
        richTextStyle.richText = true;
        richTextStyle.wordWrap = true;

        searchBox = new GUIStyle(EditorStyles.helpBox);
        searchBox.padding = new RectOffset(10, 10, 10, 10 );

        searchBoxDragHighlight = new GUIStyle(EditorStyles.helpBox);
        searchBoxDragHighlight.normal.background = EditorGUIUtility.FindTexture( "BoxCollider2D Icon" );
        searchBoxDragHighlight.border = new RectOffset(10,10,10,10);
        searchBoxDragHighlight.padding = new RectOffset(10, 10, 10, 10 );

        searchInnerDepthBox = new GUIStyle(EditorStyles.inspectorFullWidthMargins);
        searchInnerDepthBox.padding = new RectOffset(0,0,0,0);

        resultsBox = new GUIStyle(EditorStyles.helpBox);
        resultsBox.padding = new RectOffset(0, 0, 10, 10 );

        divider = new GUIStyle("box");
        divider.border.top = divider.border.bottom = 1;
        divider.margin.top = 10;

        dragDivider = new GUIStyle();
        dragDivider.border = new RectOffset(1,1,1,1);
        dragDivider.margin = new RectOffset(0,0,0,0);
        dragDivider.alignment = TextAnchor.MiddleCenter;
        Texture2D thumbbg = (Texture2D)findObject(EditorGUIUtility.isProSkin ? "com.eh.searchandreplace.thumbbg" : "com.eh.searchandreplace.thumbbglight");
        dragDivider.normal.background = thumbbg;
        Texture2D thumbTex = (Texture2D)findObject("com.eh.searchandreplace.thumb");
        thumb = new GUIContent(thumbTex);

        dragDividerNormal = dragDivider.normal.background;

        plusButton = new GUIStyle(GUI.skin.button);
        plusButton.margin.top = 2;

        toolbarButton = new GUIStyle(EditorStyles.toolbarButton);
        toolbarButton.fixedHeight = 20;
        toolbarButton.padding = new RectOffset(0, 0, 2, 2 );

        resultStyle1 = new GUIStyle();
        resultStyle1.margin = new RectOffset(1,1,0,0);
        float lightgray = EditorGUIUtility.isProSkin ? 0.22f : 0.8f;
        resultStyle1.normal.background = MakeTex(1,1, new Color(lightgray, lightgray, lightgray));
        resultStyle2 = new GUIStyle();
        float gray = EditorGUIUtility.isProSkin ? 0.26f : 0.77f;
        resultStyle2.normal.background = MakeTex(1,1, new Color(gray, gray, gray));
        resultStyle2.margin = new RectOffset(1,1,0,0);

        selectedStyle = new GUIStyle();
        selectedStyle.margin = new RectOffset(1,1,0,0);
        float selectGray = EditorGUIUtility.isProSkin ? 0.22f : 0.8f;
        float selectBlue = EditorGUIUtility.isProSkin ? 0.4f : 0.9f;
        selectedStyle.normal.background = MakeTex(1,1, new Color(selectGray, selectBlue, selectGray));

        swapToggle = new GUIStyle();
        swapToggle.fixedWidth = 15;
        swapToggle.fixedHeight = 15;
        swapToggle.normal.background = (Texture2D)findObject( "com.eh.swap" );
        swapToggle.hover.background = (Texture2D)findObject( "com.eh.swap.hover" );
        swapToggle.margin = new RectOffset(0,4,2,0);

        errorStyle = new GUIStyle();
        errorStyle.normal.background = EditorGUIUtility.FindTexture( "d_console.erroricon.sml" );
        errorStyle.fixedWidth = 17;
        errorStyle.fixedHeight = 15;
        errorStyle.margin = new RectOffset(0,0,0,0);
        errorStyle.padding = new RectOffset(0,0,0,0);

        olPlusPlus = new GUIStyle((GUIStyle)"OL Plus");
        olPlusPlus.margin = new RectOffset(0,0,2,0);
        olPlusPlus.fixedWidth = 15;
        olPlusPlus.fixedHeight = 15;

        olMinusMinus = new GUIStyle((GUIStyle)"OL Minus");
        olMinusMinus.margin = new RectOffset(0,0,2,0);
        olMinusMinus.fixedWidth = 15;
        olMinusMinus.fixedHeight = 15;

        optionsToggle = new GUIStyle(); 
        optionsToggle.normal.background = (Texture2D)findObject("com.eh.options.normal");
        optionsToggle.onNormal.background = (Texture2D)findObject("com.eh.options.onNormal");
        optionsToggle.fixedWidth = 15;
        optionsToggle.fixedHeight = 15;

#if UNITY_2018_3_OR_NEWER
        prefabIcon = EditorGUIUtility.FindTexture( "Prefab Icon" );
#else
        prefabIcon = EditorGUIUtility.FindTexture( "PrefabNormal Icon" );
#endif
        scriptIcon = EditorGUIUtility.FindTexture( "cs Script Icon" );
#if UNITY_2018_3_OR_NEWER
        goIcon = EditorGUIUtility.FindTexture("UnityEngine/GameObject Icon");
#else 
        goIcon = EditorGUIUtility.FindTexture("GameObject Icon");
#endif
        if(goIcon == null)
        {
          goIcon = EditorGUIUtility.FindTexture( "Prefab Icon" );
        }
        materialIcon = EditorGUIUtility.FindTexture( "Material Icon" );
        sceneAssetIcon = EditorGUIUtility.FindTexture( "SceneAsset Icon" );
        viewToolZoomIcon = EditorGUIUtility.FindTexture( "d_ViewToolZoom" );
        Texture2D downArrowTex = (Texture2D)findObject("com.eh.down");
        downArrow = new GUIStyle();
        downArrow.margin = new RectOffset(0,0,3,0);
        
        downArrow.normal.background = downArrowTex;
        downArrow.fixedWidth = 15;
        downArrow.fixedHeight = 15;

        Texture2D upArrowTex = (Texture2D)findObject("com.eh.up");

        upArrow = new GUIStyle();
        upArrow.margin = new RectOffset(0,0,3,0);
        upArrow.normal.background = upArrowTex;
        upArrow.fixedWidth = 15;
        upArrow.fixedHeight = 15;

        edit = new GUIStyle();
        edit.margin = new RectOffset(1,4,3,0);
        edit.normal.background = (Texture2D)findObject("com.eh.edit");
        edit.fixedWidth = 15;
        edit.fixedHeight = 15;

        save = new GUIStyle();
        save.margin = new RectOffset(1,1,3,0);
        save.normal.background = (Texture2D)findObject("com.eh.save");
        save.fixedWidth = 15;
        save.fixedHeight = 15;

        rename = new GUIStyle();
        rename.margin = new RectOffset(1,1,3,0);
        rename.normal.background = (Texture2D)findObject("com.eh.rename");
        rename.fixedWidth = 15;
        rename.fixedHeight = 15;

        load = new GUIStyle();
        load.margin = new RectOffset(1,1,3,0);
        load.normal.background = (Texture2D)findObject("com.eh.load");
        load.fixedWidth = 15;
        load.fixedHeight = 15;

        loadSearch = new GUIStyle();
        loadSearch.margin = new RectOffset(1,5,3,0);
        loadSearch.normal.background = (Texture2D)findObject("com.eh.loadSearch");
        loadSearch.fixedWidth = 15;
        loadSearch.fixedHeight = 15;

      }

    }


    private Texture2D MakeTex(int width, int height, Color col)
    {
        Color[] pix = new Color[width*height];
 
        for(int i = 0; i < pix.Length; i++)
            pix[i] = col;
 
        Texture2D result = new Texture2D(width, height);
        result.SetPixels(pix);
        result.Apply();
        result.hideFlags = HideFlags.HideAndDontSave;
 
        return result;
    }

    void OnDisable()
    {
      if(resultStyle2 != null)
      {
        DestroyImmediate(resultStyle1.normal.background);
        DestroyImmediate(resultStyle2.normal.background);
      }
      if(childWindow)
      {
        childWindow.Close();
      }
    }

    void OnGUI () 
    {
      init(); // dirty flag means this only runs once.
      Instance = this;
      float topViewPercent = searchJob == null ? 1.0f : dividePercentY;
      bool canInteract = searchJob == null || searchJob.SearchStatus != SearchStatus.InProgress;
      GUI.enabled = canInteract;
      scrollPosition = GUILayout.BeginScrollView(scrollPosition, GUILayout.Height(position.height * topViewPercent));
      
      GUILayout.BeginHorizontal();
      CVS();

      Event e = Event.current;
      bool searchWhilePlaying = !Application.isPlaying;
      searchWhilePlaying = true;
      if (e.type == EventType.KeyDown && searchWhilePlaying) {
        if(e.keyCode == KeyCode.Return && metaKeyDown(e) && !(e.shift))
        {
          e.Use();
          if(continueSearch())
          {
            doSearch();
            return;//searching wipes the layout!!!
          }
        }
        if(e.keyCode == KeyCode.Return && metaKeyDown(e) && e.shift && searchOptions.searchType == SearchType.SearchAndReplace)
        {
          e.Use();
          if(continueSearch())
          {
            doSearchAndReplace();
            return;//searching wipes the layout!!!
          }
        }
        if(e.keyCode == KeyCode.Backspace && metaKeyDown(e) && e.shift)
        {
          e.Use();
          searchJob = null;
          return;//searching wipes the layout!!!
        }
      }

      GUILayout.BeginHorizontal();
      float lw = EditorGUIUtility.labelWidth;
      EditorGUIUtility.labelWidth = SRWindow.compactLabelWidth;
      List<GUIContent> searchLabels = new List<GUIContent>();
      //searchLabels.AddRange(searchForOptionLabels);
      foreach(string label in searchForOptionLabels)
      {
        searchLabels.Add(SRLoc.GUI(label));
      }
      searchLabels.Add(new GUIContent("------"));
      if(savedSearches.searches.Count > 0)
      {
        foreach(SavedSearch ss in savedSearches.searches)
        {
          searchLabels.Add(new GUIContent(ss.name));
        }
        searchLabels.Add(new GUIContent("-------"));
        searchLabels.Add(new GUIContent("Edit Searches"));
      }
      searchLabels.Add(new GUIContent("Save Search"));
      int newSearchForIndex = EditorGUILayout.Popup( SRLoc.GUI("search.popup"), searchForIndex, searchLabels.ToArray());
      if(newSearchForIndex != searchForIndex)
      {
        if(newSearchForIndex == 3)
        {
          // Ignore! This is a divider.
        }else{
          
          if(newSearchForIndex < 3)
          {
            searchForIndex = newSearchForIndex;
            EditorPrefs.SetInt(Keys.prefSearchFor, searchForIndex);
            //search is one of the normal persistent searches.
            currentSearch = searches[searchForIndex];
          }else{
            // either loading a search, or saving the current search.
            if(newSearchForIndex == searchLabels.Count - 1)
            {
              //show save search window.
              childWindow = (SearchWindow)ScriptableObject.CreateInstance(typeof(SaveSearchWindow));
              childWindow.parent = this;
              childWindow.ShowUtility();
            }else if (newSearchForIndex == searchLabels.Count - 2)
            {
              childWindow = (SearchWindow)ScriptableObject.CreateInstance(typeof(LoadSearchWindow));
              childWindow.parent = this;
              childWindow.ShowUtility();
            }else{
              int savedSearchIndex = newSearchForIndex - 4;
              if(savedSearchIndex < savedSearches.searches.Count)
              { 
                searchForIndex = newSearchForIndex;
                EditorPrefs.SetInt(Keys.prefSearchFor, searchForIndex);
                SavedSearch ss = savedSearches.searches[searchForIndex - 4];
                LoadSearch(ss);
              }
            }
          }
        }
      }

      EditorGUIUtility.labelWidth = lw; // i love stateful gui! :(

      updateSearchType();

      drawHelp();
      
      GUILayout.EndHorizontal();
      // GUILayout.BeginHorizontal();
      // GUILayout.EndHorizontal();

      CVE();

      GUILayout.EndHorizontal();
      // GUILayout.Space(10);
      GUILayout.BeginVertical();
      currentSearch.Draw(searchOptions);


      GUI.enabled = canInteract && currentSearch.CanSearch(searchOptions) && searchWhilePlaying; //NOTE: whether we can search while the application is playing is controlled by the search scope. 
      GUILayout.BeginHorizontal();
      GUILayout.FlexibleSpace();
      CVS();
      if(GUILayout.Button("Search", GUILayout.Width(200.0f)))
      {
        if(continueSearch())
        {
          doSearch();
          return; // We have to return immediately because all our guilayout is wiped
        }
      } 
      GUI.enabled = canInteract && currentSearch.CanSearchAndReplace(searchOptions) && searchWhilePlaying; //NOTE: whether we can search while the application is playing is controlled by the search scope.

      if(searchOptions.searchType == SearchType.SearchAndReplace)
      {
        if(GUILayout.Button("Search And Replace", GUILayout.Width(200.0f)))
        {
          if(continueSearch())
          {
            doSearchAndReplace();
            return;
          }
        }
      }

      GUI.enabled = canInteract;
      CVE();
      GUILayout.FlexibleSpace();
      GUILayout.EndHorizontal();

      if(searchJob == null)
      {
        GUILayout.FlexibleSpace();
        
        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();

        GUILayout.EndHorizontal();
      }// else searchjob will draw it!

      GUILayout.EndVertical();


      GUILayout.EndScrollView();
      GUI.enabled = true;

      float dividerHeight = 7.0f;
      Color c = GUI.backgroundColor;
      if(resizing)
      {
        GUI.color = Color.green;
      }
      if(GUILayout.RepeatButton(thumb, dragDivider, new GUILayoutOption[]{GUILayout.ExpandWidth(true), GUILayout.Height(dividerHeight)}))
      {
        resizing = true;
      }
      GUI.color = c;

      if(resizing)
      {
        float minSplitViewSize = 60.0f; // The smallest we will allow the 'split' to be sized.

        float maxY = Mathf.Min(SRWindow.Instance.position.height - minSplitViewSize, Event.current.mousePosition.y);
        float mouseY = Mathf.Max(minSplitViewSize, maxY);
        mouseY -= dividerHeight * 0.5f;
        dividePercentY  = mouseY / SRWindow.Instance.position.height ;
        Repaint();
        if(Event.current.type == EventType.Used)
        {
          resizing = false;
        }
      }


      if(searchJob != null)
      {
        searchJob.Draw();
      }
      

      GUILayout.Space(10);

      if(persist)
      {
        persist = false;
        // currentSearch.Persist();
        foreach(SearchItemSet sis in searchesToPersist)
        {
          sis.Persist();
        }
      }

    }

    void OnInspectorUpdate()
    {
      if(searchJob != null && searchJob.SearchStatus == SearchStatus.InProgress)
      {
        Repaint();
      }
    }

    void drawHelp()
    {
      if(GUILayout.Button(new GUIContent(EditorGUIUtility.FindTexture( "_help" )), SRWindow.richTextStyle, GUILayout.Width(20)))
      {
        SRHelpWindow.ShowHelpWindow("help.overview");
      }
    }


    public static bool metaKeyDown(Event e)
    {
      if(Application.platform == RuntimePlatform.OSXEditor)
      {
        return e.command;
      }
      // all other platforms.
      return e.control;

    }

    //parses view data into data data~!
    public void updateSearchType()
    {
      bool newMode = GUILayout.Toggle(replaceMode, SRLoc.GUI("replace.toggle"), GUILayout.Width(75.0f));
      if(newMode != replaceMode)
      {
        replaceMode = newMode;
        searchOptions.searchType = replaceMode ? SearchType.SearchAndReplace : SearchType.Search;
        int newPrefVal = replaceMode ? 1 : 0;
        EditorPrefs.SetInt(Keys.prefSearchType, newPrefVal);
      }
    }

    private bool continueSearch()
    {
      string warning = currentSearch.GetWarning();
      if(warning != string.Empty)
      {
        return EditorUtility.DisplayDialog("Warning", warning, "Continue", "Cancel");
      }
      return true;
    }


    private void doSearch()
    {
      // Just in case I'm wrapping this!
      if(currentSearch.CanSearch(searchOptions))
      {
        SearchOptions options = searchOptions.Copy(); 
        options.searchType = SearchType.Search;
        searchJob = new SearchJob(currentSearch, options, currentSearch.searchScope.ToData(options));
        searchJob.AddAssets(SearchAssetFactory.GetAssets( currentSearch.searchScope.ToData(options)));
        EditorCoroutineUtility.StartCoroutine(searchJob.ExecuteAsync(), this);
      }
    }

    private void doSearchAndReplace()
    {
      if(currentSearch.CanSearchAndReplace(searchOptions))
      {
        SearchOptions options = searchOptions.Copy();
        options.searchType = SearchType.SearchAndReplace;

        searchJob = new SearchJob(currentSearch, options, currentSearch.searchScope.ToData(options));
        searchJob.AddAssets(SearchAssetFactory.GetAssets( currentSearch.searchScope.ToData(options)));
        EditorCoroutineUtility.StartCoroutine(searchJob.ExecuteAsync(), this);
      }
    }

    private void updateReplaceMode()
    {
      replaceMode = searchOptions.searchType == SearchType.SearchAndReplace ? true : false;
      int replacePref = replaceMode ? 1 : 0;
      EditorPrefs.SetInt(Keys.prefSearchType, replacePref);
    }

    public void LoadSearch(SavedSearch search)
    {
      SavedSearch newSearch = search.Clone();
      currentSearch = newSearch.search;
      currentSearch.SetPath(Keys.SavedSearch);
      searchOptions = newSearch.options;

      updateReplaceMode();
      PersistCurrentSearch(); 

    }

    public void SaveSearch(SavedSearch search)
    {
      savedSearches.AddSearch(search);
      LoadSearch(search); 
      searchForIndex = savedSearches.searches.IndexOf(search) + 4;
      EditorPrefs.SetInt(Keys.prefSearchFor, searchForIndex);
    }

    public void RemoveSearch(SavedSearch search)
    {
      // Most of this logic here is just updating the ui.

      SavedSearch ss = null;
      if(searchForIndex > 3)
      {
        ss = savedSearches.searches[searchForIndex-4];
      }

      savedSearches.searches.Remove(search);

      if(searchForIndex > 3)
      {
        // deleted our current search!
        if(search == ss)
        {
          searchForIndex = 3;
        }else{
          searchForIndex = savedSearches.searches.IndexOf(ss) + 4;
        }
        EditorPrefs.SetInt(Keys.prefSearchFor, searchForIndex);
      }
    }

    public void RemoveSearchItem(SearchItem item) 
    {
      currentSearch.Remove(item, true);
      PersistCurrentSearch();
    }

    public void MoveSearch(SavedSearch moveSearch, int moveSearchAmount)
    {
      SavedSearch ss = null;
      if(searchForIndex > 3)
      {
        ss = savedSearches.searches[searchForIndex-4];
      }
      int index = savedSearches.searches.IndexOf(moveSearch);
      savedSearches.searches.Remove(moveSearch);
      index += moveSearchAmount;
      savedSearches.searches.Insert(index, moveSearch);
      if(searchForIndex > 3)
      {
        searchForIndex = savedSearches.searches.IndexOf(ss) + 4;
        EditorPrefs.SetInt(Keys.prefSearchFor, searchForIndex);
      }
    }

    public void PersistCurrentSearch()
    {
      searchesToPersist.Add(currentSearch);
      persist = true;
    }

    public void PersistSearch(SearchItemSet search)
    {
      searchesToPersist.Add(search);
      persist = true;
    }

    public bool Compact()
    {
      return position.width < compactWidth;
    }

    public void CVS()
    {
      if(Compact())
      {
        GUILayout.BeginVertical();
      }
    }

    public void CVE()
    {
      if(Compact())
      {
        GUILayout.EndVertical();
      }
    }

    public static void Divider()
    {
      GUILayout.Box(GUIContent.none, SRWindow.divider, new GUILayoutOption[]{GUILayout.ExpandWidth(true), GUILayout.Height(1)});
    }

    public bool isSearchingInScene()
    {
      return currentSearch.isSearchingInScene();
    }

    public bool isSearchingDependencies()
    {
      return currentSearch.isSearchingDependencies();
    }


    public void changeScopeTo(ProjectScope scope)
    {
      currentSearch.changeScopeTo(scope);
      PersistCurrentSearch();
    }

    public ProjectScope getCurrentScope()
    {
      return currentSearch.getCurrentScope();
    } 

    public void duplicateSearchItem(SearchItem item)
    {
      currentSearch.AddCopy(item);
    }

    public static UnityEngine.Object findObject(string name)
    {
      string[] guids = AssetDatabase.FindAssets (name);
      if(guids.Length > 0)
      {
        if (guids.Length == 0)
        {
          return AssetDatabase.LoadAssetAtPath<Texture2D>(AssetDatabase.GUIDToAssetPath(guids[0]));
        }
        else
        {
          foreach (string guid in guids)
          {
            string assetPath = AssetDatabase.GUIDToAssetPath(guid);
            if (Path.GetFileNameWithoutExtension(assetPath) == name)
            { 
              return AssetDatabase.LoadAssetAtPath<Texture2D>(AssetDatabase.GUIDToAssetPath(guid));
            }
          }
        }
      }
      return null;
    }
  }
}
