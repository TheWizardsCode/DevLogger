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
        DiscordPanel discordPanel;
        GitPanel gitPanel;
        MediaPanel mediaPanel;
        DevLogPanel devLogPanel;
        DevLogScreenCaptures m_ScreenCaptures;
        DevLogEntries m_DevLogEntries;
        Camera m_CaptureCamera;

        string m_CapturesFolderPath;

        private string[] toolbarLabels = { "Entry", "Dev Log", "Git", "Settings" };
        private int selectedTab = 0;

        [UnityEditor.MenuItem("Tools/Wizards Code/Dev Logger")]
        public static void ShowWindow()
        {
            EditorWindow.GetWindow(typeof(DevLoggerWindow), false, "DevLog: " + Application.productName, true);
        }

        private void Awake()
        {
            // todo these keys should be in a constants file
            m_CapturesFolderPath = EditorPrefs.GetString("DevLogCapturesFolderPath_" + Application.productName);
            m_OrganizeByProject = EditorPrefs.GetBool("DevLogOrganizeByProject_" + Application.productName);
            m_OrganizeByScene = EditorPrefs.GetBool("DevLogOrganizeByScene_" + Application.productName);

            m_DevLogEntries = AssetDatabase.LoadAssetAtPath(EditorPrefs.GetString("DevLogScriptableObjectPath_" + Application.productName), typeof(DevLogEntries)) as DevLogEntries;
            devLogPanel = new DevLogPanel(m_DevLogEntries);

            m_ScreenCaptures = AssetDatabase.LoadAssetAtPath(EditorPrefs.GetString("DevLogScreenCapturesObjectPath_" + Application.productName), typeof(DevLogScreenCaptures)) as DevLogScreenCaptures;
            if (m_CaptureCamera == null)
            {
                m_CaptureCamera = Camera.main;
            }
            mediaPanel = new MediaPanel(m_ScreenCaptures, m_CaptureCamera, m_CapturesFolderPath, m_OrganizeByProject, m_OrganizeByScene);

            entryPanel = new EntryPanel(m_DevLogEntries, m_ScreenCaptures);
            twitterPanel = new TwitterPanel(entryPanel);
            discordPanel = new DiscordPanel(entryPanel);
            gitPanel = new GitPanel(entryPanel);
        }

        private void OnEnable()
        {
            EditorApplication.update += Update;
            mediaPanel.OnEnable();
            entryPanel.OnEnable();
            discordPanel.OnEnable();
            GitSettings.Load();
        }

        private void OnDisable()
        {
            EditorApplication.update -= Update;
            mediaPanel.OnEnable();
            entryPanel.OnDisable();
            discordPanel.OnDisable();
            GitSettings.Save();
            AssetDatabase.SaveAssets();

            // todo these keys should be in a constants file
            EditorPrefs.SetString("DevLogCapturesFolderPath_" + Application.productName, m_CapturesFolderPath);
            EditorPrefs.SetBool("DevLogOrganizeByProject_" + Application.productName, m_OrganizeByProject);
            EditorPrefs.SetBool("DevLogOrganizeByScene_" + Application.productName, m_OrganizeByScene);
            EditorPrefs.SetString("DevLogScriptableObjectPath_" + Application.productName, AssetDatabase.GetAssetPath(m_DevLogEntries));
            EditorPrefs.SetString("DevLogScreenCapturesObjectPath_" + Application.productName, AssetDatabase.GetAssetPath(m_ScreenCaptures));
        }

        private void OnImageSelection()
        {
            mediaPanel.OnImageSelection();
        }

        private static bool startCapture;
        private bool m_OrganizeByProject = true;
        private bool m_OrganizeByScene = true;

        void Update()
        {
            mediaPanel.Update();
        }

        #region GUI

        [MenuItem("Tools/Wizards Code/Capture GIF %#`")]
        static void CaptureGif()
        {
            startCapture = true;
        }

        [MenuItem("Tools/Wizards Code/Capture GIF", true)]
        static bool ValidateCaptureGif()
        {
            return Application.isPlaying;
        }
        private void OnInspectorUpdate()
        {
            if (startCapture)
            {
                mediaPanel.CaptureGif();
                startCapture = false;
            }
        }

        void OnGUI()
        {            try
            {
                selectedTab = GUILayout.Toolbar(selectedTab, toolbarLabels);
                switch (selectedTab)
                {
                    case 0:
                        if (m_DevLogEntries != null && m_ScreenCaptures != null)
                        {
                            entryPanel.ScreenCaptures = m_ScreenCaptures;
                            entryPanel.Entries = m_DevLogEntries;
                            entryPanel.OnGUI();

                            EditorGUILayout.Space();

                            mediaPanel.CaptureCamera = m_CaptureCamera;
                            mediaPanel.ScreenCaptures = m_ScreenCaptures;
                            mediaPanel.OnGUI();

                            EditorGUILayout.Space();

                            twitterPanel.screenCaptures = m_ScreenCaptures;
                            twitterPanel.OnGUI();

                            EditorGUILayout.Space();

                            discordPanel.screenCaptures = m_ScreenCaptures;
                            discordPanel.OnGUI();
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
            Skin.StartSection("Capture Storage", false);

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel("Captures Save Folder");
            string originalPath = m_CapturesFolderPath;
            m_CapturesFolderPath = EditorGUILayout.TextField(m_CapturesFolderPath);
            if (GUILayout.Button("Browse"))
            {
                m_CapturesFolderPath = EditorUtility.OpenFolderPanel("Select a folder in which to save captures", m_CapturesFolderPath, "");
            }
            if (m_CapturesFolderPath != originalPath)
            {
                mediaPanel.capturesFolder = m_CapturesFolderPath;
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel("File Organizaton");
            EditorGUILayout.BeginVertical();
            m_OrganizeByProject = EditorGUILayout.ToggleLeft("Organize in Project sub folders (e.g. 'root/Project')", m_OrganizeByProject);
            m_OrganizeByScene = EditorGUILayout.ToggleLeft("Organize in Scene sub folders (e.g. 'root/Project/Scene')", m_OrganizeByScene);
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();

            Skin.EndSection();


            Skin.StartSection("Camera", false);
            if (!m_CaptureCamera) m_CaptureCamera = Camera.main;
            if (!m_CaptureCamera)
            {
                EditorGUILayout.LabelField("No main camera in scene, please tag a camera as MainCamera or select a camera here.");
            }

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel("Camera for captures");
            m_CaptureCamera = (Camera)EditorGUILayout.ObjectField(m_CaptureCamera, typeof(Camera), true);
            EditorGUILayout.EndHorizontal();
            Skin.EndSection();

            Skin.StartSection("Dev Log Objects", false);
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
            Skin.EndSection();

            Skin.StartSection("Helpers", false);

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

            Skin.EndSection();
        }

        


        #endregion

    }
}