using UnityEngine;
using UnityEditor;
using System.IO;
using System;

namespace sr
{
  [System.Serializable]
  public class ReplaceItemString : ReplaceItem<DynamicTypeString, string>
  {
    // public string replaceStr = "";

    [System.NonSerialized]
    public string displayStr = "";

    public override void Draw()
    {
      GUILayout.BeginHorizontal();
      float lw = EditorGUIUtility.labelWidth;
      EditorGUIUtility.labelWidth = SRWindow.compactLabelWidth;
      string newStr = EditorGUILayout.TextField(Keys.Replace, displayStr);
      EditorGUIUtility.labelWidth = lw; // i love stateful gui! :(

      if(displayStr != newStr)
      {
        replaceValue = StringUtil.Unescape(newStr);
        displayStr = newStr;

        SRWindow.Instance.PersistCurrentSearch();
      }
      drawSwap();
      GUILayout.EndHorizontal();

      if(parent != null)
      {
        if(parent.conditional != Conditional.Equals)
        {
          if(parent.regex)
          {
            EditorGUILayout.HelpBox("Replacing with "+ StringUtil.Prettify(parent.conditional)+" and Regex works differently. Replacing will completely overwrite your value in this mode. ", MessageType.Warning);
          }else 
          if (parent.contains)
          {
            EditorGUILayout.HelpBox("Replacing with "+ StringUtil.Prettify(parent.conditional)+" and the Contains option works differently. Replacing will completely overwrite your value in this mode.", MessageType.Warning);
          }
        }
      }
    }

    public override void OnDeserialization()
    {
      base.OnDeserialization();
      displayStr = StringUtil.Escape(replaceValue); 
    }

    protected override void replace(SearchJob job, SerializedProperty prop, SearchResult result)
    {
      if(parent == null)
      {
        prop.stringValue = replaceValue;
      }else{
        if(parent.regex)
        {
          if(parent.conditional == Conditional.Equals)
          {
            prop.stringValue = parent.expr.Replace(prop.stringValue, replaceValue);
          }else{
            prop.stringValue = replaceValue;
          }
        }else{
          if(parent.contains)
          {
            if(parent.conditional == Conditional.Equals)
            {
              if(parent.ignoreCase)
              {
                prop.stringValue = StringUtil.ReplaceString(prop.stringValue, parent.searchValue, replaceValue, StringComparison.OrdinalIgnoreCase);
              }else{
                prop.stringValue = prop.stringValue.Replace(parent.searchValue, replaceValue);
              }
            }else{
              prop.stringValue = replaceValue;
            }
          }else{
            //exact match.
            prop.stringValue = replaceValue;
          }
        }
      }
      result.replaceStrRep = "'"+displayStr+"'";
    }

    protected override void Swap()
    {
      if(parent == null)
      {
        return;
      }
      string tmp = parent.searchValue;
      parent.searchValue = replaceValue;
      replaceValue = tmp;

      tmp = parent.displayValue;
      parent.displayValue = displayStr;
      displayStr = tmp;

      // Fixes issue where the UI shows old info in text fields....le sigh. :(
      GUI.FocusControl("Nothing In Particular");
      SRWindow.Instance.PersistCurrentSearch();
    }

    public override string StringValueFor(SerializedProperty prop)
    {
      if(parent != null)
      {
        return base.StringValueFor(prop);
      }else{
        return "'" + prop.stringValue + "'";
      }
    }



  }



}