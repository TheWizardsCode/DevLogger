using UnityEngine;
using UnityEditor;
using System.IO;
using System;

namespace sr
{
  [System.Serializable]
  public class ReplaceItemClass : ReplaceItem<DynamicTypeClass, ObjectID>
  {
    // public ObjectID objID;

    // [System.NonSerialized]
    // public DynamicTypeClass parent;

    [System.NonSerialized]
    bool replaceValid;

    public override void Draw()
    {
      GUILayout.BeginHorizontal();
      float lw = EditorGUIUtility.labelWidth;
      EditorGUIUtility.labelWidth = SRWindow.compactLabelWidth;
      UnityEngine.Object newValue = EditorGUILayout.ObjectField(Keys.Replace, replaceValue.obj, typeof(UnityEngine.Object), true);

      EditorGUIUtility.labelWidth = lw; // i love stateful gui! :(

      if(replaceValue.obj != newValue)
      {
        UnityEngine.Object scriptObject = ObjectUtil.getScriptObjectFromObject(newValue);
        if(scriptObject == null && newValue != null)
        {
          //looks like we couldn't get an object!
          Debug.Log("[Project Search & Replace] "+newValue+" isn't a Component, MonoBehaviour, ScriptableObject or MonoScript.");
        }
        setObject(scriptObject);
        SRWindow.Instance.PersistCurrentSearch();
      }

      if(type != null)
      {
        GUILayout.Label("("+type.Name+")");
        drawSwap();
        GUILayout.EndHorizontal();
        if(parent.type != null)
        {
          bool isMB = typeof(MonoBehaviour).IsAssignableFrom(type);
          bool isParentMB = typeof(MonoBehaviour).IsAssignableFrom(parent.type);
          bool isScript = typeof(ScriptableObject).IsAssignableFrom(type);
          bool isParentScript = typeof(ScriptableObject).IsAssignableFrom(parent.type);

          if( (isMB && isParentMB) || (isScript && isParentScript))
          {
            if(parent.type.IsAssignableFrom(type))
            {
              replaceValid = true;
              // EditorGUILayout.HelpBox("Looks good!", MessageType.Info);
            }else{
              replaceValid = true;
              EditorGUILayout.HelpBox("Inheritance Warning: These classes do not inherit from each other but Unity will attempt to merge data as best as possible.", MessageType.Info);
            }
          }
          else 
          {
            replaceValid = false;
            EditorGUILayout.HelpBox("Invalid or Mixed Parameters. Components can only be searched for, not replaced.", MessageType.Warning);
          }
        }
      }else{
        replaceValid = false;
        drawSwap();
        GUILayout.EndHorizontal();
      }
    }

    public void setObject(UnityEngine.Object o)
    {
      replaceValue.SetObject(o);
      type = ObjectUtil.getTypeFromObject(o);
    }

    public override bool IsValid()
    {
      return replaceValid;
    }

    // Fixes nulls in serialization manually...*sigh*.
    public override void OnDeserialization()
    {
      if(replaceValue == null)
      {
        replaceValue = new ObjectID();
      }
      replaceValue.OnDeserialization();
    }

    protected override void replace(SearchJob job, SerializedProperty prop, SearchResult result)
    {
      if(prop.name == "m_Script")
      {
        prop.objectReferenceValue = replaceValue.obj;
        result.replaceStrRep = type.Name;
        prop.serializedObject.ApplyModifiedProperties();
        result.actionTaken = SearchAction.Replaced;
      }else{
        bool isScript = typeof(ScriptableObject).IsAssignableFrom(type);
        if(isScript)
        {
          UnityEngine.Object obj = prop.objectReferenceValue;
          string path = AssetDatabase.GetAssetPath(obj.GetInstanceID());
          if(path == "")
          {
            // It looks like this is a scriptable object that is serialized within
            // another object and doesn't have a specific location on disk.
            SerializedObject so = new SerializedObject(obj);
            SerializedProperty m_Script = so.FindProperty("m_Script");
            m_Script.objectReferenceValue = replaceValue.obj;
            result.replaceStrRep = type.Name;
            so.ApplyModifiedProperties();
          }else{
            result.actionTaken = SearchAction.Ignored;
          }
        }else{
          result.actionTaken = SearchAction.InstanceFound;
        }
      }

    }

    protected override void Swap() 
    {
        UnityEngine.Object tmp = parent.searchValue.obj;
        parent.SetObject(replaceValue.obj);
        setObject(tmp);
        SRWindow.Instance.PersistCurrentSearch();
    }




  } // End Class
} // End Namespace