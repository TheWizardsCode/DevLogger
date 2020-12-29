using UnityEngine;
using UnityEditor;
using System.IO;
using System;
using System.Reflection;

namespace sr
{
  [System.Serializable]
  public class ReplaceItemRunScript : ReplaceItem
  {

    //TODO: use player prefs to save this, instead of per-object.
    string methodName = "";
    string className = "";
    string assemblyName = "";
    bool verboseLogging = false;

    [System.NonSerialized]
    Vector2 scrollViewPos;

    [System.NonSerialized]
    string error = "";
    [System.NonSerialized]
    MethodInfo cachedMethod;
    
    public override void Draw()
    {
      EditorGUI.BeginChangeCheck();
      className = EditorGUILayout.TextField("Class Name", className);
      methodName = EditorGUILayout.TextField("Method Name ", methodName);
      assemblyName = EditorGUILayout.TextField("Assembly Name ", assemblyName);
      verboseLogging = EditorGUILayout.Toggle("Log Exceptions?", verboseLogging);
      if(EditorGUI.EndChangeCheck())
      {
        cachedMethod = GetMethod(out error);

        SRWindow.Instance.PersistCurrentSearch();
      }

      if(className == "" || methodName == "")
      {
        EditorGUILayout.HelpBox("Enter the info for a static method.", MessageType.Info);
      }
      else if(error != "")
      {
        EditorGUILayout.HelpBox("Error: " + error, MessageType.Warning);
      }else{
        EditorGUILayout.HelpBox("Method " + className + "." + methodName + " will be run on each search result.", MessageType.Info);
      }
    }

    // Fixes nulls in serialization manually...*sigh*.
    public override void OnDeserialization()
    { 
      popupLabel = "Run Script...";
      if(className == null)
      { 
        className = "";
      }
      if(methodName == null)
      { 
        methodName = "";
      }
      if(assemblyName == null)
      { 
        assemblyName = "";
      }
      cachedMethod = GetMethod(out error);
    }

    public override bool IsValid()
    {
      // todo validate string!
      return cachedMethod != null;
    }

    protected override void Swap() 
    {
    }

    MethodInfo GetMethod(out string error)
    {
      string assemblyName = "Assembly-CSharp-Editor";
      try
      {

        if(this.assemblyName != "")
        {
          assemblyName = this.assemblyName;
        }
        Type t = Type.GetType(className + ", " + assemblyName);
        if(t == null)
        {
          error = "Could not find type '" + className + "'' in assembly '" + assemblyName + "'";
          return null;
        }
        MethodInfo method = t.GetMethod(methodName, BindingFlags.Static | BindingFlags.Public);
        if(method == null)
        {
          error = className + " does not have a method called '" + methodName + "'";
          return null;
        }
        ParameterInfo[] parameters = method.GetParameters();
        if(parameters.Length != 1)
        {
          error = "'" + methodName + "'' has " + parameters.Length + " parameters.";
          return null;
        }
        if(parameters[0].ParameterType != typeof(SearchParameters))
        {
          error = "'" + methodName + "'s parameter is of type " + parameters[0].ParameterType;
          return null;
        }
        error = "";
        return method;
      }
      catch
      {
        error = "An exception occurred.";
        return null;
      }
    }

    protected override void replace(SearchJob job, SerializedProperty prop, SearchResult result)
    {
      try
      {
        SearchParameters searchparameters = new SearchParameters();
        searchparameters.assetBeingSearched = job.assetData;
        searchparameters.prop = prop;
        cachedMethod.Invoke(null, new object[]{ searchparameters });

        result.actionTaken = SearchAction.RanScript;
        result.replaceStrRep = searchparameters.Message;
      }
      catch(Exception ex)
      {
        result.actionTaken = SearchAction.Error;
        result.error = ex.Message;
        if(verboseLogging)
        {
          Debug.LogException(ex);
        }
      }
    }


  } // End Class
} // End Namespace