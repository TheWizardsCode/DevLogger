using UnityEngine;
using UnityEditor;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Reflection;

namespace sr
{

  [System.Serializable]
  public class DynamicTypeData<T> : DynamicTypeData
  {
    public T searchValue;
  }

  /**
   * The base class for all dynamic types. What's a dynamic type? It provides the
   * generic search mechanisms for a specific type of data...dynamically! For 
   * example The user may decide to search for strings, we determine the type
   * is a string and display a DynamicTypeString field. This class will display
   * the appropriate GUI, handle the search, and serialize for re-use. Most
   * methods do not need to be extended, but it seems like every data type has
   * an edge case requiring something unique. :P
   *
   * Dynamic types are always the child of a DynamicTypeField, which does the 
   * dynamic-ing of these classes.
   *
   * This class does not handle replacing, that is delegated to the ReplaceItem
   * class.
   */
  [System.Serializable]
  public class DynamicTypeData
  {

    public Conditional conditional = Conditional.Equals;

    protected ReplaceItem replaceItem; // This does not exist for complex dynamic types like DynamicTypeCollection!
    public ReplaceItem ReplaceItem { 
      get
      {
        return replaceItem; 
      }
    }

    protected int replaceOptionsIndex = 0; // -1 means we do not have options.
    protected List<ReplaceItem> replaceOptions;

    // These are the default replace options for most things. This may be
    // different depending on certain types, but in general is applied to all
    // standard replaces.
    public static List<Type> defaultReplaceOptions = new List<Type>()
    {
      typeof(ReplaceItemWithSeparateProperty),  
      typeof(ReplaceItemSwapWithPrefab),
      typeof(ReplaceItemRunScript),
    };

    
    [System.NonSerialized]
    protected string[] replaceOptionsPopupStrings;

    [System.NonSerialized]
    public DynamicTypeField parent;

    [System.NonSerialized]
    protected SearchOptions options;

    [System.NonSerialized]
    public bool showMoreOptions = true;

    [System.NonSerialized]
    public bool assetRequiresImporter = false;

    /**
     * By default properties can always be replaced, unless they come from values
     * derived during an asset import (aka Textures and AudioClips).
     * This specifically talks about whether there is something about the 
     * SerializedProperty that defines that this is unsearchable.
     */
    [System.NonSerialized]
    public bool propertyCanBeReplaced = true;


    public virtual void Draw(SearchOptions options)
    {
      //stub
    }

    protected void drawReplaceItem(SearchOptions options)
    {
      if(options.searchType == SearchType.SearchAndReplace)
      {
        if(propertyCanBeReplaced)
        {
          drawReplaceOptions();
          replaceItem.options = options;
          replaceItem.showMoreOptions = showMoreOptions;
          GUILayout.BeginVertical();
          replaceItem.Draw();
          GUILayout.EndVertical();
        }else{
          GUILayout.BeginHorizontal();
          GUILayout.Space(SRWindow.compactLabelWidth);
          GUILayout.Label("Property is read-only and cannot be replaced.");
          GUILayout.EndHorizontal();
        }
      }
    }

    protected virtual void drawReplaceOptions()
    {
      if(replaceOptionsIndex >= 0 && replaceOptionsPopupStrings != null)
      {
        float lw = EditorGUIUtility.labelWidth;
        EditorGUIUtility.labelWidth = SRWindow.compactLabelWidth;
        int newIndex = EditorGUILayout.Popup(" ", replaceOptionsIndex, replaceOptionsPopupStrings);
        EditorGUIUtility.labelWidth = lw; // i love stateful gui! :(

        if(newIndex != replaceOptionsIndex)
        {
          replaceOptionsIndex = newIndex;
          replaceItem = replaceOptions[replaceOptionsIndex];
          SRWindow.Instance.PersistCurrentSearch();
        }
      }else{
        EditorGUILayout.LabelField("No replace options found.");
      }
    }

    // Upon initialization of all replace item options, this will create values
    // for the popup, and set the replaceItem variables.
    protected void initReplaceOptions(List<Type> inputTypes)
    {
      // This is passed a list of classes. We should then
      // compare this with the list of known replace items and make sure we have all of our valid types.

      // First we append our default options to this list.
      List<Type> replaceTypes = new List<Type>(inputTypes);
      replaceTypes.AddRange(defaultReplaceOptions);

      if(replaceOptions == null)
      {
        replaceOptions = new List<ReplaceItem>(); 
      }


      // We create a new list of items, so that we maintain order and any old types are removed.
      List<ReplaceItem> newReplaceOptions = new List<ReplaceItem>();
      foreach(Type rt in replaceTypes)
      {
        bool found = false;
        foreach(ReplaceItem ri in replaceOptions)
        {
          if(ri.GetType() == rt)
          {
            newReplaceOptions.Add(ri);
            break;
          }
        }
        if(!found)
        { 

          ConstructorInfo ctor = rt.GetConstructor(Type.EmptyTypes);
          if(ctor != null)
          {
            object instance = ctor.Invoke(new object[0]);
            ReplaceItem ri = (ReplaceItem) instance;
            newReplaceOptions.Add(ri);
          }else{
            Debug.Log("[Search & Replace] Constructor for " + rt + " not found.");
          }
        }
      }
      replaceOptions = newReplaceOptions;
      replaceOptionsPopupStrings = new string[replaceOptions.Count];
      for(int i = 0; i < replaceOptions.Count; i++)
      {
        ReplaceItem ri = replaceOptions[i];
        ri.OnDeserialization();
        ri.SetParent(this);
        replaceOptionsPopupStrings[i] = ri.popupLabel;
      }

      if(replaceOptionsIndex >= replaceOptions.Count)
      {
        if(replaceOptions.Count > 0)
        {
          replaceOptionsIndex = replaceOptions.Count - 1;
        }else{
          replaceOptionsIndex = -1; 
        }
      }
      if(replaceOptionsIndex > -1)
      {
        replaceItem = replaceOptions[replaceOptionsIndex];
      }
    }

    protected void drawConditionalControl(float width)
    {
      if(showMoreOptions)
      {
        Conditional newConditional = (Conditional)EditorGUILayout.Popup((int)conditional, new string[]{"==","!=","Any"}, GUILayout.Width(width));
        if(newConditional != conditional)
        {
          conditional = newConditional;
          SRWindow.Instance.PersistCurrentSearch();
        }
      }
    }

    /*
     * Draws the 'value' or the conditional control before the field for a value.
     * Requires the code in drawControlEnd() at some point.
     */
    protected void drawControlStart(bool drawLabel=true)
    {
      GUILayout.BeginHorizontal();
      if(showMoreOptions)
      {
        drawConditionalControl(SRWindow.compactLabelWidthNP);
      }else{
        if(drawLabel)
        {
          GUILayout.Label("Value:", GUILayout.Width(SRWindow.compactLabelWidthNP)); //minus element padding when manually creating control groups.
        }
      }
      GUI.enabled = !anyValueEquals();

    }

    /** 
     * If you call drawControlStart() you'll have to call this, or some similar 
     * incantation.
     */
    protected void drawControlEnd()
    {
      GUI.enabled = true;
      GUILayout.EndHorizontal();
    }

    public virtual bool ValueEquals(SerializedProperty prop)
    {
      return false;
    }

    public bool ValueEqualsWithConditional(SerializedProperty prop)
    {
      bool val = ValueEquals(prop);
      if(!showMoreOptions)
      {
        return val;
      }
      switch(conditional)
      {
        case Conditional.Equals:
        return val;
        case Conditional.NotEquals:
        return !val;
        case Conditional.Any:
        return true;
      }
      return false;
    }

    protected bool anyValueEquals()
    {
      return (showMoreOptions && (conditional == Conditional.Any));
    }

    // In general this only needs to be overridden for complicated data types.
    public virtual void SearchProperty( SearchJob job, SearchItem item, SerializedProperty prop )
    {
      if(ValueEqualsWithConditional(prop))
      {
        if(item.subsearch != null)
        {
          //has a subsearch! continue.
          SearchItem subItem = (SearchItem)item.subsearch;
          subItem.SubsearchProperty(job, prop);
        }else{
          addMatch(job, item, prop, StringValueFor(prop));
        }
      }
    }

    /**
     * Searches game objects when the search type is global.
     */
    public virtual void SearchGameObject( SearchJob job, SearchItem item, GameObject go)
    {
    }

    /**
     * Searches UnityEngine.Objects when a search type is globlal.
     */
    public virtual void SearchObject( SearchJob job, SearchItem item, UnityEngine.Object go)
    {
    }

    // When a match is found, this will create a SearchResult and add it to the
    // job in progress.
    protected void addMatch(SearchJob job, SearchItem item, SerializedProperty prop, string stringValue)
    {
      SearchResultProperty result = new SearchResultProperty(prop, stringValue, job);
      if(result.pathInfo.prefabType == PrefabTypes.PrefabInstance || result.pathInfo.prefabType == PrefabTypes.NestedPrefabInstance)
      {
          if(job.options.searchType == SearchType.SearchAndReplace)
          {
            result.actionTaken = SearchAction.InstanceReplaced;
          }else{
            result.actionTaken = SearchAction.InstanceFound;
          }
            if(item.subsearch == null)
            {
              //Apply change, only if change already exists!
              ReplaceProperty(job, prop, result);
            }
          job.MatchFound(result, item);
      }else{
        if(item.subsearch == null)
        {
          ReplaceProperty(job, prop, result);
        }
        job.MatchFound(result, item);
      }
    }
   
    /**
     * Sometimes fields can be invalid, and in these instances we must tell our
     * parent item this, so that this search item is not executed.
     */
    public virtual bool IsValid()
    {
      return false;
    }

    /**
     * Sometimes a search can be valid but a replace is not. In most cases if a 
     * search is valid, so is the replace, so by default this returns IsValid()
     * by default.
     */
    public virtual bool IsReplaceValid()
    {
      return IsValid() && replaceItem != null && replaceItem.IsValid();
    }

    /**
     * While existing serialization exists, there are gaps in functionality which
     * means we use this special call back so that deserialization occurs in the
     * order necessary.
     */
    public virtual void OnDeserialization()
    {

    }

    // A string representation of the value for display purposes.
    public virtual string StringValue()
    {
      return "";
    }

    // A string representation of the value for display purposes, customized
    // based upon the property given. Useful when matches are fuzzy.
    public virtual string StringValueFor(SerializedProperty prop)
    {
      return StringValue();
    }


    public virtual SerializedPropertyType PropertyType()
    {
      return SerializedPropertyType.ArraySize;
    }

    /**
     * Called to do the actual replace operation. In general this is only overridden
     * for collection data types that contain 'sub' fields.
     */
    public virtual void ReplaceProperty(SearchJob job, SerializedProperty prop, SearchResult result)
    {
      if(propertyCanBeReplaced)
      {
        replaceItem.options = job.options;
        replaceItem.ReplaceProperty(job, prop, result);
      }else{
        if(job.options.searchType == SearchType.SearchAndReplace)
        {
          Debug.Log("[DynamicTypeData] property cannot be replaced!");
        }
      }
    }
   
    public virtual bool hasAdvancedOptions()
    {
      return _hasAdvancedOptions();
    }

    public virtual bool _hasAdvancedOptions()
    {
      return false;
    }

    
    void displayImporterDebug(InitializationContext ic)
    {
      string assetPath = AssetDatabase.GetAssetPath(ic.obj);
      AssetImporter importer = AssetImporter.GetAtPath(assetPath);
      Type t = importer.GetType();

      StringBuilder sb = new StringBuilder();
      foreach (MemberInfo mi in t.GetMembers() )
      {
        sb.Append("[DynamicTypeData] mi:"+mi.Name + " type:"+mi.MemberType);
        if(mi is MethodInfo)
        {
          MethodInfo method = (MethodInfo)mi;
          ParameterInfo[] parameters = method.GetParameters();
          foreach(ParameterInfo pi in parameters)
          {
            sb.Append(" param: " + pi.Name + " :  " + pi.ParameterType + "  ");
          }
        }
        sb.Append("\n");

      }
      Debug.Log("[DynamicTypeData] methods:\n"+ sb.ToString());
    }

    /**
     * When the user takes an action that changes the type of data within the field
     * for example, from string to int, this is called. We then determine some 
     * basic information about the field, such as if an asset importer is required
     * for this data type.
     * 
     * This also tries to get the associated Serialized Property so that the 
     * field can initialize with a useful value.
     */
    public virtual void OnSelect(InitializationContext ic)
    {
      //For purposes of getting information on AssetImporters uncomment this!
      //displayImporterDebug();

      if(ic.obj != null)
      {
        assetRequiresImporter = ic.obj is Texture || ic.obj is AudioClip;
      }else{
        //Nulls don't require asset importers :)
        assetRequiresImporter = false;
      }
      propertyCanBeReplaced = true;
      // Debug.Log("[DynamicTypeData] OnSelect"+ic.fieldData.fieldName);
      if(assetRequiresImporter){
        propertyCanBeReplaced = AssetImporterMethodUtil.HasImporterMethod(ic.fieldData.fieldName);
      } 

      SerializedProperty prop = ic.getProp();
      if(prop != null)
      {
        if(prop.propertyType == PropertyType())
        {
          initializeDefaultValue(prop);
        }
      }
    }

    /**
     * When OnSelect is called, this will be called with the serialized property
     * from the initialization object (the object dragged into the field). This
     * can then be used to find a useful value to search for.
     */
    protected virtual void initializeDefaultValue(SerializedProperty prop)
    {
      //stub
    }

    /**
     * Swaps the search and replace values. For those 'oops' moments.
     */
    public virtual void Swap()
    {
      
    }

    /**
     * When a conditional is being applied to the search, this will provide 
     * more information to the end user of the current state of the search.
     */
    public virtual string StringValueWithConditional()
    {
      if(showMoreOptions)
      {
        if(conditional == Conditional.Any)
        {
          return " = ANY";
        }
        return " "+StringUtil.Prettify(conditional)+" '"+StringValue()+"'";
      }else{
        return " = '"+StringValue()+"'";
      }
    }

    /**
     * Currently really only used to show a warning if you decide to search for
     * null globally.
     */
    public virtual string GetWarning()
    {
      return string.Empty;
    }

    /**
     * Allows the child class to cache useful information that can be applied 
     * when doing a search.
     */
    public virtual void OnSearchBegin()
    {

    }

    /**
     * Allows the child class to report additional search results upon search 
     * completion (for example unused classes).
     */
    public virtual void OnSearchEnd(SearchJob job, SearchItem item)
    {

    }

    /**
     * Allows the child class to complete and information  related to
     * the asset was just searched.
     */
    public virtual void OnAssetSearchBegin(SearchJob job, SearchItem item)
    {

    }

    /**
     * Allows the child class to complete and information  related to
     * the asset was just searched.
     */
    public virtual void OnAssetSearchEnd(SearchJob job, SearchItem item)
    {

    }

  }
}
