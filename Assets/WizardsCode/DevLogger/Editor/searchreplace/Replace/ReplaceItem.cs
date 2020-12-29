using UnityEngine;
using UnityEditor;
using System;

namespace sr
{

  [System.Serializable]
  public class ReplaceItem<P,T> : ReplaceItem where P:DynamicTypeData<T>
  {
    /**
     * While dtd may be set in a lot of cases, parent will be set in even less
     * cases, as this defines a potential coupling for additional functionality.
     */
    [System.NonSerialized]
    public P parent;

    public T replaceValue;

    public override void Draw()
    {
      GUILayout.BeginHorizontal();
      float lw = EditorGUIUtility.labelWidth;
      EditorGUIUtility.labelWidth = SRWindow.compactLabelWidth;
      T newValue = drawEditor();
      EditorGUIUtility.labelWidth = lw; // i love stateful gui! :(
      if( !(replaceValue.Equals( newValue )))
      {
        replaceValue = newValue;
        SRWindow.Instance.PersistCurrentSearch();
      }
      drawSwap();
      GUILayout.EndHorizontal();
    }

    protected virtual T drawEditor()
    {
      return default(T);
    }

    protected override bool CanSwap()
    {
      return parent != null;
    }

    public override void SetParent(DynamicTypeData p) 
    {
      base.SetParent(p);
      if(p is P)
      {
        parent = (P)p;
      }
    }

    protected override void Swap()
    {
      T tmp = parent.searchValue;
      parent.searchValue = replaceValue;
      replaceValue = tmp;
      SRWindow.Instance.PersistCurrentSearch();

    }


  }
  /**
   * The base class for classes that replace values in a search result. A ReplaceItem
   * is the child of its corresponding DynamicTypeData, for example, DynamicTypeString
   * and ReplaceItemString. It defines the basic interface and some sensible defaults
   * for all the various classes.
   */
  [System.Serializable]
  public class ReplaceItem
  {

    public string popupLabel = "Replace With:";

    public Type type;

    [System.NonSerialized]
    public SearchOptions options;

    [System.NonSerialized]
    public bool showMoreOptions;

    /**
     * Used currently to facilitate data swap between replace and this.
     * Don't expect this to be set! It has to be set manually, and not every
     * ReplaceItem has a dtd 'parent'.
     */
    public DynamicTypeData dtd;

    public virtual void OnDeserialization()
    { 
    }

    protected void drawHeader()
    {
      EditorGUILayout.LabelField(Keys.Replace, GUILayout.Width(SRWindow.compactLabelWidthNP));
    }

    protected virtual void Swap()
    {

    }

    protected virtual bool CanSwap()
    {
      return false;
    }

    protected virtual void drawSwap()
    {
      if(CanSwap())
      {
        if(GUILayout.Button(GUIContent.none, SRWindow.swapToggle))
        {
          Swap();
          GUI.FocusControl("Nothing In Particular");
        }
      }
    }
    
    public void ReplaceProperty(SearchJob job, SerializedProperty prop, SearchResult result)
    {
      if(job.options.searchType == SearchType.SearchAndReplace)
      {
        if(job.assetData.assetRequiresImporter)
        {
          reimport(job, prop, result);
        }else{
          replace(job, prop, result);
        }
        // Did the replace operation fail? If so the actionTaken will be set to 
        // error.
        // Did something say throw this search result away? If so the action taken will be to ignore.
        if(result.actionTaken == SearchAction.Error || result.actionTaken == SearchAction.Ignored)
        {
          return;
        }
        if(result.actionTaken != SearchAction.ObjectSwapped && result.actionTaken != SearchAction.ObjectRemoved)
        {
          prop.serializedObject.ApplyModifiedProperties();
        }

        if(result.actionTaken == SearchAction.InstanceReplaced)
        {
          /**
           * The following line is neccesary for instance modifications 
           * to take effect. This may be an internal unity bug.
           */
          PrefabUtility.RecordPrefabInstancePropertyModifications(prop.serializedObject.targetObject);
        }else if(result.actionTaken != SearchAction.InstanceFound 
                 && result.actionTaken != SearchAction.RanScript 
                 && result.actionTaken != SearchAction.InstanceNotReplaced
                 && result.actionTaken != SearchAction.ObjectRemoved
                 ){
          result.actionTaken = SearchAction.Replaced;
        }
        if(result.actionTaken != SearchAction.InstanceNotReplaced)
        {
          job.assetData.assetIsDirty = true;
        }
      }else{
        if(result.actionTaken != SearchAction.InstanceFound)
        {
          result.actionTaken = SearchAction.Found;
        }
      }
    }

    protected virtual void replace(SearchJob job, SerializedProperty prop, SearchResult result)
    {
      //stub
    }

    protected virtual void reimport(SearchJob job, SerializedProperty prop, SearchResult result)
    {
      //stub
    }

    // This is exposed solely to allow ReplaceItems to call replace() but without
    // exposing replace.
    public void ReplaceInternal(SearchJob job, SerializedProperty prop, SearchResult result)
    {
      replace(job, prop, result);
    }

    public virtual void Draw()
    {
      
    }

    public virtual bool hasAdvancedOptions()
    {
      return false;
    }

    public virtual void SetParent(DynamicTypeData p)
    {
      dtd = p;
    }

    // whether the current replace is a valid search.
    public virtual bool IsValid()
    {
      return true;
    }

        // A string representation of the value for display purposes, customized
    // based upon the property given. Useful when matches are fuzzy.
    public virtual string StringValueFor(SerializedProperty prop)
    {
      if(dtd != null)
      {
        return dtd.StringValueFor(prop);
      }
      return "";
    }

    //Called when the DTD has updated, giving this child an opportunity to respond.
    public virtual void OnDTDUpdate()
    {

    }



  }
}