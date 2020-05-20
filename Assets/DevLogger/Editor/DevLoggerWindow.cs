using Moments;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
using WizardsCode.DevLog;
using WizardsCode.EditorUtils;
using WizardsCode.Git;
using WizardsCode.Social;

namespace WizardsCode.DevLogger
{

    /// <summary>
    /// The main DevLogger control window.
    /// </summary>
    [Serializable]
    public class DevLoggerWindow : EditorWindow
    {
        [SerializeField] EntryPanel entryPanel;
        [SerializeField] TwitterPanel twitterPanel;
        [SerializeField] GitPanel gitPanel;
        [SerializeField] MediaPanel mediaPanel;

        private string[] toolbarLabels = { "Entry", "Dev Log", "Git", "Settings" };
        private int selectedTab = 0;

        [UnityEditor.MenuItem("Tools/Wizards Code/Dev Logger")]
        public static void ShowWindow()
        {
            EditorWindow.GetWindow(typeof(DevLoggerWindow), false, "DevLog: " + Application.productName, true);
        }

        private void Awake()
        {
            mediaPanel = new MediaPanel();
            entryPanel = new EntryPanel(mediaPanel);
            twitterPanel = new TwitterPanel(entryPanel);
            gitPanel = new GitPanel(entryPanel);
        }

        private void OnEnable()
        {
            EditorApplication.update += Update;
            entryPanel.OnEnable();
            DevLogPanel.OnEnable();
            GitSettings.Load();
        }

        private void OnDisable()
        {
            EditorApplication.update -= Update;
            entryPanel.OnDisable();
            DevLogPanel.OnDisable();
            GitSettings.Save();
        }

        private void OnDestroy()
        {
            mediaPanel.OnDestroy();
        }

        private bool showSettings = false;

        void Update()
        {
            mediaPanel.Update();
        }

        #region GUI
        void OnGUI()
        {
            selectedTab = GUILayout.Toolbar(selectedTab, toolbarLabels);
            switch (selectedTab)
            {
                case 0:
                    if (mediaPanel.CaptureCamera && DevLogPanel.DevLog != null) {
                        entryPanel.OnGUI();
                        EditorGUILayout.Space();
                        mediaPanel.OnGUI();
                        EditorGUILayout.Space();
                        twitterPanel.OnGUI();
                    } else
                    {
                        SettingsTab();
                    }
                    break;
                case 1:
                    DevLogPanel.OnGUI();
                    break;
                case 2:
                    gitPanel.OnGUI();
                    break;
                case 3:
                    SettingsTab();
                    break;
            }
        }

        public void SettingsTab()
        {
            EditorGUILayout.BeginVertical("Box");
            EditorGUILayout.BeginVertical();
            if (!mediaPanel.CaptureCamera)
            {
                EditorGUILayout.LabelField("No main camera in scene, please select tag a camera as MainCamera or select a camera here.");
            }

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel("Camera for captures");
            mediaPanel.CaptureCamera = (Camera)EditorGUILayout.ObjectField(mediaPanel.CaptureCamera, typeof(Camera), true);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel("Dev Log Storage");
            DevLogPanel.DevLog = EditorGUILayout.ObjectField(DevLogPanel.DevLog, typeof(DevLogEntries), true) as DevLogEntries;
            if (DevLogPanel.DevLog == null)
            {
                if (GUILayout.Button("Create"))
                {
                    DevLogPanel.DevLog = ScriptableObject.CreateInstance<DevLogEntries>();
                    AssetDatabase.CreateAsset(DevLogPanel.DevLog, "Assets/Dev Log " + Application.version + ".asset");
                    AssetDatabase.SaveAssets();
                }
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Reset"))
            {
                mediaPanel.LatestCaptures = new List<int>();
                mediaPanel.ImageSelection = new List<bool>();
                if (EditorUtility.DisplayDialog("Reset Twitter OAuth Tokens?",
                    "Do you also want to clear the Twitter access tokens?",
                    "Yes", "Do Not Clear Them")) {
                    Twitter.ClearAccessTokens();
                }
            }

            if (GUILayout.Button("Capture DevLogger"))
            {
                mediaPanel.CaptureWindowScreenshot("WizardsCode.DevLogger.DevLoggerWindow");
            }

            if (GUILayout.Button("Dump Window Names"))
            {
                EditorWindow[] allWindows = Resources.FindObjectsOfTypeAll<EditorWindow>();
                foreach (EditorWindow window in allWindows)
                {
                    Debug.Log("Window name: " + window);
                }
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.LabelField("Welcome to " + Application.productName + " v" + Application.version);
            
            EditorGUILayout.EndVertical();

            EditorGUILayout.EndVertical();
        }

        


        #endregion

    }
}