using Moments;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
using WizardsCode.DevLogger;
using WizardsCode.EditorUtils;
using WizardsCode.Git;
using WizardsCode.Social;

namespace WizardsCode.DevLogger
{

    /// <summary>
    /// The main DevLogger control window.
    /// </summary>
    public class DevLoggerWindow : EditorWindow
    {
        EntryPanel entryPanel;
        TwitterPanel twitterPanel;
        GitPanel gitPanel;
        MediaPanel mediaPanel;
        DevLogPanel devLogPanel;
        DevLogScreenCaptures m_ScreenCaptures;
        DevLogEntries m_DevLogEntries;
        Camera m_CaptureCamera;

        private string[] toolbarLabels = { "Entry", "Dev Log", "Git", "Settings" };
        private int selectedTab = 0;

        [UnityEditor.MenuItem("Tools/Wizards Code/Dev Logger")]
        public static void ShowWindow()
        {
            EditorWindow.GetWindow(typeof(DevLoggerWindow), false, "DevLog: " + Application.productName, true);
        }

        private void Awake()
        {
            m_DevLogEntries = AssetDatabase.LoadAssetAtPath(EditorPrefs.GetString("DevLogScriptableObjectPath_" + Application.productName), typeof(DevLogEntries)) as DevLogEntries;
            devLogPanel = new DevLogPanel(m_DevLogEntries);

            m_ScreenCaptures = AssetDatabase.LoadAssetAtPath(EditorPrefs.GetString("DevLogScreenCapturesObjectPath_" + Application.productName), typeof(DevLogScreenCaptures)) as DevLogScreenCaptures;
            if (m_CaptureCamera == null)
            {
                m_CaptureCamera = Camera.main;
            }
            mediaPanel = new MediaPanel(m_ScreenCaptures, m_CaptureCamera);

            entryPanel = new EntryPanel(m_DevLogEntries, m_ScreenCaptures);
            twitterPanel = new TwitterPanel(entryPanel);
            gitPanel = new GitPanel(entryPanel);
        }

        private void OnEnable()
        {
            EditorApplication.update += Update;
            mediaPanel.OnEnable();
            entryPanel.OnEnable();
            GitSettings.Load();
        }

        private void OnDisable()
        {
            EditorApplication.update -= Update;
            mediaPanel.OnEnable();
            entryPanel.OnDisable();
            GitSettings.Save();
            AssetDatabase.SaveAssets();
            EditorPrefs.SetString("DevLogScriptableObjectPath_" + Application.productName, AssetDatabase.GetAssetPath(m_DevLogEntries));
            EditorPrefs.SetString("DevLogScreenCapturesObjectPath_" + Application.productName, AssetDatabase.GetAssetPath(m_ScreenCaptures));
        }

        private void OnDestroy()
        {
            AssetDatabase.SaveAssets();
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
            try
            {
                selectedTab = GUILayout.Toolbar(selectedTab, toolbarLabels);
                switch (selectedTab)
                {
                    case 0:
                        if (m_CaptureCamera && m_DevLogEntries != null && m_ScreenCaptures != null)
                        {
                            entryPanel.ScreenCaptures = m_ScreenCaptures;
                            entryPanel.Entries = m_DevLogEntries;
                            entryPanel.OnGUI();

                            EditorGUILayout.Space();

                            mediaPanel.CaptureCamera = m_CaptureCamera;
                            mediaPanel.ScreenCaptures = m_ScreenCaptures;
                            mediaPanel.OnGUI();

                            EditorGUILayout.Space();

                            twitterPanel.ScreenCaptures = m_ScreenCaptures;
                            twitterPanel.OnGUI();
                        }
                        else
                        {
                            SettingsTab();
                        }
                        break;
                    case 1:
                        if (devLogPanel == null) devLogPanel = new DevLogPanel(m_DevLogEntries);

                        devLogPanel.ScreenCaptures = m_ScreenCaptures;
                        devLogPanel.Entries = m_DevLogEntries;
                        devLogPanel.OnGUI();
                        break;
                    case 2:
                        gitPanel.OnGUI();
                        break;
                    case 3:
                        SettingsTab();
                        break;
                }
            } catch (InvalidCastException)
            {
                // this is a workaround. An exception is thrown when a new scene is loaded.
                Repaint();
            }
        }

        public void SettingsTab()
        {
            EditorGUILayout.BeginVertical("Box");
            EditorGUILayout.BeginVertical();
            if (!m_CaptureCamera) m_CaptureCamera = Camera.main;
            if (!m_CaptureCamera)
            {
                EditorGUILayout.LabelField("No main camera in scene, please select tag a camera as MainCamera or select a camera here.");
            }

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel("Camera for captures");
            m_CaptureCamera = (Camera)EditorGUILayout.ObjectField(m_CaptureCamera, typeof(Camera), true);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel("Dev Log Storage");
            m_DevLogEntries = EditorGUILayout.ObjectField(m_DevLogEntries, typeof(DevLogEntries), true) as DevLogEntries;
            if (m_DevLogEntries == null)
            {
                if (GUILayout.Button("Create"))
                {
                    m_DevLogEntries = ScriptableObject.CreateInstance<DevLogEntries>();
                    AssetDatabase.CreateAsset(m_DevLogEntries, "Assets/Dev Log " + Application.version + ".asset");
                    AssetDatabase.SaveAssets();
                }
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel("Screen Capture Storage");
            m_ScreenCaptures = EditorGUILayout.ObjectField(m_ScreenCaptures, typeof(DevLogScreenCaptures), true) as DevLogScreenCaptures;
            if (m_ScreenCaptures == null)
            {
                if (GUILayout.Button("Create"))
                {
                    m_ScreenCaptures = ScriptableObject.CreateInstance<DevLogScreenCaptures>();
                    AssetDatabase.CreateAsset(m_ScreenCaptures, "Assets/Screen Captures " + Application.version + ".asset");
                    AssetDatabase.SaveAssets();
                }
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Reset"))
            {
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