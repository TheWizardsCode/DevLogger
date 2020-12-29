using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEditor;
using System.Text;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
#if UNITY_2018_3_OR_NEWER
using UnityEditor.Experimental.SceneManagement;
#endif

namespace sr
{
  /**
   * When a search result is found, it is encapsulated in this class. This provides
   * a potential view onto a search result without loading the object into memory.
   */
  public class SearchResult
  {

    public bool alternate = false;
    public int recordNum;
    public SearchAction actionTaken = SearchAction.Found;
    // String representation of the search and replace items. Used for display.
    public string strRep;
    public string replaceStrRep;
    public string error;

    public const string notFound = "{6} Did not find <b>{0}</b>.";
    public const string notFoundCompact = "{0}";

    public const string found = "{6} Found <b>{0}</b> on {2}";
    public const string foundCompact = "{3}";
    
    public const string replaced = "{6} Replaced <b>{0}</b> with <b>{1}</b> on {2}";
    public const string replacedCompact = "{3}";

    public const string instanceFound = "{6} Instance Found <b>{0}</b> on {2}";
    public const string instanceFoundCompact = "{3}";

    public const string instanceReplaced = "{6} Replaced Instance <b>{0}</b> with <b>{1}</b> on {2}";
    public const string instanceReplacedCompact = "{3}";

    public const string notReplaced = "{6} Did <color=red>not</color> replace <b>{0}</b>. Reason: '{5}' on {2}";
    public const string notReplacedCompact = "{3}";


    public const string errorTemplate = "{6} <color=red>ERROR:</color> {5} Found <b>{0}</b> on {2}";
    public const string errorTemplateCompact = "{5}";
    public const string unknown = "{6} Unknown Action Taken!";

    public const string assetMissingScript = "{6} Asset <b>{0}</b> is missing script.";
    public const string assetMissingScriptCompact = "{0}";

    public const string ranScript = "{6} Script: '{1}' on result: <b>{3}</b>.";
    public const string ranScriptCompact = "Expand for script output.";

    public const string objectRemoved = "{6} Removed instance on {1}";
    public const string objectRemovedCompact = "{3}";

    public bool Selected { get; set; }

    public PathInfo pathInfo;
    public SearchResultGroup resultGroup;
    public SearchResultSet resultSet;

    public virtual void CopyToClipboard(StringBuilder sb)
    {
      string template = "";

      switch(actionTaken)
      {
        case SearchAction.Found:
        //search only.
        template = found;
        break;
        case SearchAction.Replaced:
        template = replaced;
        break;
        case SearchAction.InstanceFound:
        template = instanceFound;
        break;
        case SearchAction.InstanceReplaced:
        template = instanceReplaced;
        break;
        case SearchAction.InstanceNotReplaced:
        template = notReplaced;
        break;
        case SearchAction.Error:
        template = errorTemplate;
        break;
        case SearchAction.NotFound:
        template = notFound;
        break;
        case SearchAction.AssetMissingScript:
        template = assetMissingScript;
        break;
        case SearchAction.RanScript:
        template = ranScript;
        break;
        case SearchAction.ObjectRemoved:
          template = objectRemoved;
          break;
        default:
        template = unknown;
        break;
      }
      string labelStr = format(template);
      labelStr = labelStr.Replace("<b>", "").Replace("</b>", "");
      sb.Append(labelStr);
      sb.Append("\n");
    }


    public virtual void Draw()
    {
      GUIStyle resultStyle = alternate ? SRWindow.resultStyle1 : SRWindow.resultStyle2;
      if(Selected)
      {
        resultStyle = SRWindow.selectedStyle;
      }
      GUILayout.BeginHorizontal(resultStyle);
      string labelStr = "";
      string template = "";
      switch(actionTaken)
      {
        case SearchAction.Found:
        //search only.
        template = SRWindow.Instance.Compact() ? foundCompact : found;
        break;
        case SearchAction.Replaced:
        template = SRWindow.Instance.Compact() ? replacedCompact : replaced;
        break;
        case SearchAction.InstanceFound:
        template = SRWindow.Instance.Compact() ? instanceFoundCompact : instanceFound;
        break;
        case SearchAction.InstanceReplaced:
        template = SRWindow.Instance.Compact() ? instanceReplacedCompact : instanceReplaced;
        break;
        case SearchAction.InstanceNotReplaced:
        template = SRWindow.Instance.Compact() ? notReplacedCompact : notReplaced;
        break;
        case SearchAction.Error:
        template = SRWindow.Instance.Compact() ? errorTemplateCompact : errorTemplate;
        break;
        case SearchAction.NotFound:
        template = SRWindow.Instance.Compact() ? notFoundCompact : notFound;
        break;
        case SearchAction.AssetMissingScript:
        template = SRWindow.Instance.Compact() ? assetMissingScriptCompact : assetMissingScript;
        break;
        case SearchAction.RanScript:
        template = SRWindow.Instance.Compact() ? ranScriptCompact : ranScript;
        break;
        case SearchAction.ObjectRemoved:
          template = SRWindow.Instance.Compact() ? objectRemovedCompact : objectRemoved;
          break;
        default:
        template = unknown;
        break;
      }
      labelStr = format(template);
      float width = SRWindow.Instance.position.width - 80;
      GUIContent content = new GUIContent(labelStr);
      float height = SRWindow.richTextStyle.CalcHeight(content, width);
      EditorGUILayout.SelectableLabel(labelStr, SRWindow.richTextStyle, GUILayout.Height(height));
      Texture2D icon = SRWindow.prefabIcon;
      if(pathInfo.objID.isSceneObject)
      {
        icon = SRWindow.goIcon;
      } 
      if(GUILayout.Button(icon , new GUILayoutOption[]{GUILayout.Width(30), GUILayout.Height(20) } ))
      {
        Event e = Event.current;
        bool openInProject = e.shift;
        resultSet.Select(this, openInProject, SRWindow.metaKeyDown(e));
      }
      GUILayout.EndHorizontal();
    }

    public Object GetSelection(bool openInProject)
    {
      string extension = Path.GetExtension(pathInfo.assetPath);
        switch (extension)
        {
          case ".unity":
            if (openInProject)
            {
              // it is not possible to open a scene object in the project.
              return null;
            }
            Scene currentScene = EditorSceneManager.GetActiveScene();
            if (currentScene.path != pathInfo.assetPath)
            {
              bool shouldContinue = SceneUtil.SaveDirtyScenes();
              if(!shouldContinue)
              {
                return null;
              }
              currentScene = EditorSceneManager.OpenScene(pathInfo.assetPath, OpenSceneMode.Single);
            }
#if UNITY_2018_3_OR_NEWER
            PrefabStage currentPrefabStage = PrefabStageUtility.GetCurrentPrefabStage();
            if (currentPrefabStage != null)
            {
              StageUtility.GoToMainStage();
            }
#endif
            return pathInfo.objID.searchForObjectInScene(currentScene);
          case ".prefab":
#if UNITY_2018_3_OR_NEWER
            if (openInProject)
            {
              GameObject root = AssetDatabase.LoadAssetAtPath<GameObject>(pathInfo.assetPath);
              return pathInfo.objID.localPath.GetObject(root);
            }
            else
            {
              // is the prefab open in the stage?
              PrefabStage stage = PrefabStageUtility.GetCurrentPrefabStage();
              GameObject root = null;
              if (stage == null ||
#if UNITY_2020_1_OR_NEWER
                  stage.assetPath != pathInfo.assetPath
#else
                  stage.prefabAssetPath != pathInfo.assetPath
#endif
              )
              {
                // this is not the currently open path. open the latest.
                root = AssetDatabase.LoadAssetAtPath<GameObject>(pathInfo.assetPath);
                AssetDatabase.OpenAsset(root);
              }
              else
              {
                root = stage.prefabContentsRoot;
              }
              return pathInfo.objID.searchForObjectInScene();
            }

#else
          // This case is pretty straightforward. Just select the object.
          return EditorUtility.InstanceIDToObject(pathInfo.gameObjectID);
#endif
          case ".controller":
             EditorApplication.ExecuteMenuItem("Window/Animator");
             // now get the animator to display it.
             string assetPath = AssetDatabase.GetAssetPath(pathInfo.gameObjectID);
             UnityEngine.Object assetObj = AssetDatabase.LoadAssetAtPath(assetPath, typeof(UnityEngine.Object));
             //set it as the selection.
             Selection.activeObject = assetObj;
             //wait until inspectors are updated and now we can set the 
             // selection to the internal object.
             EditorApplication.delayCall += ()=>{ 
               Selection.activeInstanceID = pathInfo.gameObjectID;
             };
             return assetObj;
          default:
            return EditorUtility.InstanceIDToObject(pathInfo.gameObjectID);
        }
    }
    


    string format(string template)
    {
      return string.Format(template, strRep, replaceStrRep, pathInfo.FullPath(), pathInfo.compactObjectPath, pathInfo.objectPath, error, recordNum.ToString());
    }

  }
}