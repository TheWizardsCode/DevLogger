using UnityEngine;
using UnityEditor;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Text;

namespace sr
{
  /**
   * Provides a search field for strings.
   */
  [System.Serializable]
  public class DynamicTypeString : DynamicTypeData<string>
  {
    [System.NonSerialized]
    public string displayValue = "";

    public bool contains = true;
    public bool ignoreCase = true;
    public bool regex = false;

    [System.NonSerialized]
    GUIContent caseSensitiveGUI;
    
    [System.NonSerialized]
    GUIContent exactMatchGUI;
    
    [System.NonSerialized]
    GUIContent regexGUI;

    [System.NonSerialized]
    public Regex expr;
    [System.NonSerialized]
    bool regexValid = false; 

    public static List<Type> replaceOptionTypes = new List<Type>()
    {
      typeof(ReplaceItemString),  
    };


    public override void Draw(SearchOptions options)
    {
      GUILayout.BeginHorizontal();
      SRWindow.Instance.CVS();

      drawControlStart();
      string newValue = EditorGUILayout.TextField(displayValue, GUILayout.MaxWidth(SRWindow.Instance.position.width - SRWindow.boxPad));
      GUILayout.EndHorizontal();
      if(newValue != displayValue)
      {
        displayValue = newValue;
        searchValue = StringUtil.Unescape(newValue);
        SRWindow.Instance.PersistCurrentSearch();
      }
      if(showMoreOptions)
      { 
        if(SRWindow.Instance.Compact())
        {
          EditorGUILayout.BeginHorizontal();
          GUILayout.Space(SRWindow.compactLabelWidthEP);
        }
        EditorGUILayout.BeginHorizontal();
        SRWindow.Instance.CVS();

        bool newIgnoreCase = EditorGUILayout.ToggleLeft(caseSensitiveGUI, ignoreCase, GUILayout.Width(95));
        if(newIgnoreCase != ignoreCase)
        {
          ignoreCase = newIgnoreCase;
          SRWindow.Instance.PersistCurrentSearch();

        }
        GUI.enabled = !regex && !anyValueEquals();
        bool newContains = EditorGUILayout.ToggleLeft(exactMatchGUI, contains, GUILayout.Width(75));
        if(newContains != contains)
        {
          contains = newContains;
          SRWindow.Instance.PersistCurrentSearch();
        }
        GUI.enabled = !anyValueEquals();
        bool newRegex = EditorGUILayout.ToggleLeft(regexGUI, regex, GUILayout.Width(60));
        if(newRegex != regex)
        {
          regex = newRegex;
          SRWindow.Instance.PersistCurrentSearch();
          
        }
        SRWindow.Instance.CVE();
        EditorGUILayout.EndHorizontal();
        if(SRWindow.Instance.Compact())
        {
          EditorGUILayout.EndHorizontal();
        }
      }
      GUI.enabled = true;


      if(regex)
      {
        RegexOptions regexOptions = RegexOptions.Singleline | RegexOptions.Compiled;
        if(ignoreCase)
        {
          regexOptions |= RegexOptions.IgnoreCase;
        }
        try{
          expr = new Regex(searchValue, regexOptions);
          regexValid = true;
        }catch
        {
          expr = null;
          regexValid = false;
        }
      }
      SRWindow.Instance.CVE();
      GUILayout.EndHorizontal();
      drawReplaceItem(options);
    }

    public override void OnDeserialization()
    {
      initReplaceOptions(replaceOptionTypes);

      displayValue = StringUtil.Escape(searchValue);


      caseSensitiveGUI = new GUIContent("Ignore Case", "Whether or not this search ignore case or not.");
      exactMatchGUI = new GUIContent("Contains", "Whether this should match the exact text here, or if the searched text contains this value. For example if this is on then 'hi' would match 'hi', 'how hip' and 'ship'. If the search is not case sensitive then it would also match 'HI', 'how HIP' and 'ShIp'. When Regex is checked this checkbox does nothing.");
      regexGUI = new GUIContent("Regex", "Advanced! Whether or not the given string is a regular expression. Uses C# regular expressions. Groupings are supported.");
    }

    public override bool ValueEquals(SerializedProperty prop)
    {
      // Debug.Log("[DynamicTypeString] prop:"+prop.propertyPath);
      if(!showMoreOptions)
      {
        return prop.stringValue == searchValue;
      }else{
        if(regex)
        {
          return expr.Matches(prop.stringValue).Count > 0;
        }else{
          //not regex  
          if(contains)
          {
            if(ignoreCase)
            {
              return prop.stringValue.ToLowerInvariant().IndexOf(searchValue.ToLowerInvariant()) > -1;
            }else{
              return prop.stringValue.IndexOf(searchValue) > -1;
            }
          }else{
            if(ignoreCase)
            {
              return string.Equals(prop.stringValue, searchValue, StringComparison.OrdinalIgnoreCase);
            }else{
              return prop.stringValue == searchValue;
            }
          }
        }
      }
    }

    public override bool IsValid()
    {
      bool regexOK = regex ? regexValid : true;
      return regexOK;
    }

    public override string StringValue()
    {
      return "'"+displayValue+"'";
    }

    public override string StringValueFor(SerializedProperty prop)
    {
      string val = prop.stringValue;
      if(!showMoreOptions)
      {
        //exact match.
        return "'"+val+"'";
      }else{
        if(regex)
        {
          //run the regex and get some info.
          MatchCollection matches = expr.Matches(prop.stringValue);
          StringBuilder builder = new StringBuilder();
          int match = 1;
          foreach(Match m in matches)
          {
            if(matches.Count > 1)
            {
              if(match == 1)
              {
                builder.Append("(multiple matches) ");
              }
              builder.Append("#"+match+": ");
            }
            builder.Append(m.Value);
            match++;
            if(match-1 != matches.Count )
            {
              builder.Append(", ");
            }
          }
          return builder.ToString();
        }else{
          if(contains)
          {
            if(val.Length > 100)
            {
              if(ignoreCase)
              {
                int strIndex = val.IndexOf(searchValue, StringComparison.OrdinalIgnoreCase);
                return "'" + val.Substring(strIndex, searchValue.Length) + "'";
              }else{
                int strIndex = val.IndexOf(searchValue, StringComparison.OrdinalIgnoreCase);
                return "'" + val.Substring(strIndex, searchValue.Length) + "'";
              }
            }else{
              return "'"+val+"'";
            }

          }else{
            //exact match.
            return "'"+val+"'";
          }
        }
      }
    }

    public override SerializedPropertyType PropertyType()
    {
      return SerializedPropertyType.String;
    }

    public override bool _hasAdvancedOptions()
    {
      return true;
    }

    protected override void initializeDefaultValue(SerializedProperty prop)
    {
      searchValue = prop.stringValue;
      displayValue = searchValue;
    }
  }
}
