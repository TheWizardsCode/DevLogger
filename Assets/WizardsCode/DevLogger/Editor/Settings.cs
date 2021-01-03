using UnityEngine;
using UnityEditor;
using System.IO;
using UnityEngine.SceneManagement;

namespace WizardsCode.DevLogger
{
    public static class Settings
    {
        public static string CaptureFileFolderPath
        {
            get { 
                string path = EditorPrefs.GetString(CapturesFolderPathKey); 
                if (string.IsNullOrEmpty(path))
                {
                    path = Application.dataPath;
                    path = path.Substring(0, path.Length - "Assets/".Length);
                    path += Path.DirectorySeparatorChar + "DevLog" + Path.DirectorySeparatorChar + "ScreenCaptures";
                    CaptureFileFolderPath = path;
                }

                return path;
            }
            set {
                EditorPrefs.SetString(CapturesFolderPathKey, value); 
            }
        }

        public static bool OrganizeCapturesByProject
        {
            get { return EditorPrefs.GetBool(OrganizeCapturesByProjectKey); }
            set { EditorPrefs.SetBool(OrganizeCapturesByProjectKey, value); }
        }

        public static bool OrganizeCapturesByScene { 
            get { return EditorPrefs.GetBool(OrganizeCapturesBySceneKey); }
            set { EditorPrefs.SetBool(OrganizeCapturesBySceneKey, value); }
        }

        public static string DevLogScriptableObjectPath { 
            get { return EditorPrefs.GetString(DevLogScriptableObjectPathKey); }
            set { EditorPrefs.SetString(DevLogScriptableObjectPathKey, value); }
        }

        public static string ScreenCaptureScriptableObjectPath
        {
            get { return EditorPrefs.GetString(ScreenCaptureScriptableObjectPathKey); }
            set { EditorPrefs.SetString(ScreenCaptureScriptableObjectPathKey, value); }
        }

        #region Keys
        static string CapturesFolderPathKey
        { get { return "DevLogCapturesFolderPath_" + Application.productName; } }

        static string OrganizeCapturesByProjectKey
        { get { return "DevLogOrganizeByProject_" + Application.productName; } }

        static string OrganizeCapturesBySceneKey
        {
            get { return "DevLogOrganizeByScene_" + Application.productName; }
        }

        static string DevLogScriptableObjectPathKey
        { get { return "DevLogScriptableObjectPath_" + Application.productName; } }

        static string ScreenCaptureScriptableObjectPathKey
        {
            get { return "DevLogScreenCapturesObjectPath_" + Application.productName; }
        }
        #endregion
    }
}
