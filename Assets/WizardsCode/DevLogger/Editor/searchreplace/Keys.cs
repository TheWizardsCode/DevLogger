using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

namespace sr
{
  public class Keys
  {
    public const string Text = "text";
    public const string Object = "Unity Object";
    public const string PropertyLabel = "property";
    public const string Property = "prop";
    public const string GlobalLabel = "search";
    public const string Global = "type";
    public const string InstancesLabel = "instance";
    public const string Instances = "instance";
    public const string SavedSearch = "saved";
    public const string LoadSearch = "Load Search";
    public const string Replace = "Replace:";

    public const string search = "Search";
    public const string searchAndReplace = "Search And Replace";

    public static string prefPrefix = "com.enemyhideout.sr.";
    public static string prefSearchFor = prefPrefix + "searchFor";
    public static string prefSearchType = prefPrefix + "searchType";
    public static string prefSearchScope = prefPrefix + "searchScope";
    public static string prefSearchConfig = prefPrefix + "searchConfig";
    
    public static string Everything = "Everything";
    public static string Location = "A Specific Location";

  }
}
