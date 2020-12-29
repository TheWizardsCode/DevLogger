using UnityEngine;
using UnityEditor;

namespace WizardsCode.DevLogger
{
    public static class Settings
    {
        public static string CaptureFileFolderPath
        {
            get { return EditorPrefs.GetString(CapturesFolderPathKey); }
            set
            {
                EditorPrefs.SetString(CapturesFolderPathKey, value);
            }
        }

        public static bool OrganizeCapturesByProject
        {
            get { return EditorPrefs.GetBool(OrganizeCapturesByProjectKey); }
        }

        public static bool OrganizeCapturesByScene { 
            get { return EditorPrefs.GetBool(OrganizeCapturesBySceneKey); }
        }

        public static string DevLogScriptableObjectPath { 
            get
            {
                return EditorPrefs.GetString(DevLogScriptableObjectPathKey);
            }
            set
            {
                EditorPrefs.SetString(DevLogScriptableObjectPathKey, value);
            }
        }

        public static string ScreenCaptureScriptableObjectPath
        {
            get
            {
                return EditorPrefs.GetString(ScreenCaptureScriptableObjectPathKey);
            }
        }

        #region Keys
        static string CapturesFolderPathKey
        {
            get { return "DevLogCapturesFolderPath_" + Application.productName; }
        }

        static string OrganizeCapturesByProjectKey
        {
            get { return "DevLogOrganizeByProject_" + Application.productName; }
        }

        static string OrganizeCapturesBySceneKey
        {
            get
            {
                return "DevLogOrganizeByScene_" + Application.productName;
            }
        }

        public static string DevLogScriptableObjectPathKey
        {
            get { return "DevLogScriptableObjectPath_" + Application.productName; }
        }

        public static string ScreenCaptureScriptableObjectPathKey
        {
            get
            {
                return "DevLogScreenCapturesObjectPath_" + Application.productName;
            }
        }
        #endregion
    }
}
