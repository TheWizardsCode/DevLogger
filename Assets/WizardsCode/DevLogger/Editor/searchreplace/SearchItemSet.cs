using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System;

namespace sr
{
  /**
   * Defines a number of search items that a user can search for. Determines 
   * which items are valid for a search. Clones searches. Provides utility functions
   * for queries related to all searches.
   */ 
  [System.Serializable]
  public class SearchItemSet
  {
    public List<SearchItem> items = new List<SearchItem>();
    
    [System.NonSerialized]
    public List<SearchItem> validatedItems = new List<SearchItem>();

    [System.NonSerialized]
    List<SearchItem> itemsToRemove = new List<SearchItem>();

    [System.NonSerialized]
    List<SearchItem> itemsToAdd = new List<SearchItem>();

    public SearchScope searchScope;

    string path;

    // string key;

    public static string GetSearchPath(string suffix)
    { 
      return Application.persistentDataPath+"/searchreplace/currentSearch_"+suffix;
    } 

    public void SetPath(string key)
    {
      path = GetSearchPath(key);
      // this.key = key;
    }

    public static SearchItemSet PopulateFromDisk(string key)
    {
      SearchItemSet sis = null;
      string path = GetSearchPath(key);
      // Debug.Log("[SearchItemSet] path:" + path);
      try{
 
        sis = (SearchItemSet)SerializationUtil.Deserialize(path);
        if(sis != null)
        {
          sis.OnDeserialization();
          sis.SetPath(key);
        }
      }catch(System.Exception ex)
      {
        Debug.LogException(ex);
      }
      if(sis == null) 
      {
        sis = new SearchItemSet();
        sis.OnDeserialization();
        sis.SetPath(key);
        sis.AddNew(key);
        return sis;
      }else{
        return sis;
      }
    }

    // Fixes nulls in serialization manually...*sigh*.
    public void OnDeserialization()
    {
      if(itemsToRemove == null)
      {
        itemsToRemove = new List<SearchItem>();
      }
      if(itemsToAdd == null)
      {
        itemsToAdd = new List<SearchItem>();
      }
      foreach(SearchItem item in items)
      {
        item.OnDeserialization();
      }
      if(validatedItems == null)
      {
        validatedItems = new List<SearchItem>();
      }
      if(searchScope == null)
      {
        searchScope = new SearchScope();
      }

      searchScope.OnDeserialization();
    }

    public void Persist()
    {
      SerializationUtil.Serialize(path, this);
    }
 
    public void OnSearchBegin()
    {
      validatedItems.Clear();
      for(int i = 0; i < items.Count;i++)
      {
        items[i].sortIndex = i;
      }
      foreach(SearchItem item in items)
      {
        if(item.IsValid())
        {
          validatedItems.Add(item);
          item.OnSearchBegin();
        }else{
          Debug.Log("Item is not valid.");
        }
      }
    }

    public void OnSearchEnd(SearchJob job)
    {
      foreach(SearchItem item in validatedItems)
      {
        item.OnSearchEnd(job);
      }
    }

    public void OnAssetSearchBegin(SearchJob job)
    {
      foreach(SearchItem item in validatedItems)
      {
        item.OnAssetSearchBegin(job);
      }
    }

    public void OnAssetSearchEnd(SearchJob job)
    {
      foreach(SearchItem item in validatedItems)
      {
        item.OnAssetSearchEnd(job);
      }
    }

    public SearchItemSet Clone()
    {
      SearchItemSet copy = (SearchItemSet)SerializationUtil.Copy(this);
      copy.OnDeserialization();
      return copy;
    }

    public void AddNew(string searchFor, bool persist=true)
    {
      switch(searchFor)
      {
        case Keys.Property:
          SearchItemProperty itemProp = new SearchItemProperty();
          AddNew(itemProp);
        break;
        case Keys.Global:
        SearchItemGlobal itemGlobal = new SearchItemGlobal();
        AddNew(itemGlobal);
        break;
        case Keys.Instances:
        SearchItemInstances itemInstances = new SearchItemInstances();
        AddNew(itemInstances);
        break;
      }
      if(persist)
      {
        SRWindow.Instance.PersistSearch(this);
      }
    }

    public void AddNew(SearchItem item)
    {
      item.OnDeserialization(); //reduces the need for redundant ctor init.
      items.Add(item);
      sortItems();
    }

    void sortItems()
    {
      for(int i=0; i < items.Count; i++)
      {
        items[i].sortIndex = i;
      }
    }

    public void AddCopy(SearchItem item)
    {
      SearchItem copy = (SearchItem)SerializationUtil.Copy(item);
      copy.OnDeserialization();
      int insertPoint = items.IndexOf(item);
      copy.sortIndex = insertPoint; //HACK! This will get overridden, but right now we can use this var for the insertion point.

      // This has to be done later because this will most likely occur during a Draw()
      // operation.
      // items.Insert(insertPoint, item);

      itemsToAdd.Add(copy);
    }

    public void Remove(SearchItem item, bool immediately)
    {
      itemsToRemove.Add(item);
    }

    public void Clear()
    {
      itemsToRemove.AddRange(items);
    }

    public string GetWarning()
    {
      foreach(SearchItem item in items)
      {
        string warning = item.GetWarning();
        if(warning != string.Empty)
        {
          return warning;
        }
      }
      return string.Empty;
    }

    public bool CanSearch(SearchOptions searchOptions)
    { 
      if(searchScope.IsValid(searchOptions))
      {
        if(items.Count > 0)
        {
          foreach(SearchItem item in items)
          {
            if(item.IsValid())
            {
              return true;
            }
          }
        }
      }

      return false;
    }

    public bool CanSearchAndReplace(SearchOptions searchOptions)
    {
      if(searchScope.IsValid(searchOptions))
      {
        if(items.Count > 0)
        {
          foreach(SearchItem item in items)
          {
            if(item.IsReplaceValid())
            {
              return true;
            }
          }
        }
      } 

      return false;
    }

    public void Draw(SearchOptions options)
    {
      searchScope.Draw(options);
      sortItems();
      if(items.Count > 0)
      {
        foreach(SearchItem item in items)
        {
          item.Draw(options);
        }

        foreach(SearchItem item in itemsToRemove)
        {
          items.Remove(item);
        }
        itemsToRemove.Clear();
        foreach(SearchItem item in itemsToAdd)
        {
          items.Insert(item.sortIndex, item);
        }
        itemsToAdd.Clear();

      }else{

      }

    }

    UnityEngine.Object drawDragAndDropControl(string dragText, string dropText, Texture2D icon, Type dragType)
    {
      Event e = Event.current;
      UnityEngine.Object retVal = null;

      Rect r = EditorGUILayout.BeginHorizontal();
      bool acceptingDrag = (e.type == EventType.DragUpdated || e.type == EventType.DragPerform) && r.Contains(e.mousePosition);

      if(acceptingDrag)
      {
        if(DragAndDrop.objectReferences.Length > 0)
        {
          UnityEngine.Object obj = DragAndDrop.objectReferences[0];
          if(dragType.IsAssignableFrom(obj.GetType()))
          {
            SRWindow.Instance.Repaint();
            DragAndDrop.AcceptDrag();
            DragAndDrop.visualMode = DragAndDropVisualMode.Link;
            if(e.type == EventType.DragPerform)
            {
              retVal = obj;
            }
          }
        }else{
          DragAndDrop.visualMode = DragAndDropVisualMode.Rejected;
        }
      }
        if(r.Contains(e.mousePosition) && DragAndDrop.visualMode == DragAndDropVisualMode.Link)
        {
          GUILayout.BeginVertical(SRWindow.searchBoxDragHighlight);
          GUILayout.Label(new GUIContent(dropText, icon), SRWindow.richTextStyle);
          GUILayout.EndVertical();
        }else{
          GUILayout.BeginVertical(SRWindow.searchBox);
          GUILayout.Label(new GUIContent(dragText, icon), SRWindow.richTextStyle);
          GUILayout.EndVertical();
        }
      EditorGUILayout.EndHorizontal();
      return retVal;
    }

    public void SearchProperty(SearchJob job, SerializedProperty prop)
    {
      foreach(SearchItem item in validatedItems)
      {
        item.SearchProperty(job, prop);  
      }
    }

    public bool searchItemCaresAboutAsset(SearchJob job)
    {
      foreach(SearchItem item in validatedItems)
      {
        if(item.caresAboutAsset(job))
        {
          return true;
        }else{
          // Debug.Log("[SearchItemSet] item:"+item + " doesn't care about "+ job.asset +" !");
        }
      }
      return false;
    }

    public void SearchGameObject(SearchJob job, GameObject go)
    {
      foreach(SearchItem item in validatedItems)
      {
        item.SearchGameObject(job, go);  
      }
    }

    public void SearchObject(SearchJob job, UnityEngine.Object obj)
    {
      foreach(SearchItem item in validatedItems)
      {
        item.SearchObject(job, obj);
      }
    }

    public bool isSearchingInScene()
    {
      return searchScope.isSearchingInScene();
    }

    public bool isSearchingDependencies()
    {
      return searchScope.isSearchingDependencies();
    }

    public void changeScopeTo(ProjectScope scope)
    {
      searchScope.changeScopeTo(scope);
    }

    public ProjectScope getCurrentScope()
    {
      return searchScope.getCurrentScope();
    } 

  
  }
}