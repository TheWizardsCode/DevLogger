using System;
using UnityEditor;
using UnityEngine;
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
        EntryPanel m_EntryPanel;
        TwitterPanel m_TwitterPanel;
        DiscordPanel m_DiscordPanel;
        SchedulingPanel m_SchedulingPanel;
        GitPanel m_GitPanel;
        MediaPanel m_MediaPanel;
        DevLogPanel m_DevLogPanel;
        DevLogScreenCaptures m_ScreenCaptures;
        DevLogEntries m_DevLogEntries;
        Camera m_CaptureCamera;

        private static bool startCapture;

        private string[] toolbarLabels = { "Entry", "Dev Log", "Schedule", "Git", "Settings" };
        private int selectedTab = 0;

        [UnityEditor.MenuItem("Tools/Wizards Code/Dev Logger")]
        public static void ShowWindow()
        {
            EditorWindow.GetWindow(typeof(DevLoggerWindow), false, "DevLog: " + Application.productName, true);
        }

        private void Awake()
        {
            m_DevLogEntries = AssetDatabase.LoadAssetAtPath(Settings.DevLogScriptableObjectPath, typeof(DevLogEntries)) as DevLogEntries;
            m_DevLogPanel = new DevLogPanel(m_DevLogEntries);

            m_ScreenCaptures = AssetDatabase.LoadAssetAtPath(Settings.ScreenCaptureScriptableObjectPath, typeof(DevLogScreenCaptures)) as DevLogScreenCaptures;
            if (m_CaptureCamera == null)
            {
                m_CaptureCamera = Camera.main;
            }
            
            m_MediaPanel = new MediaPanel(m_ScreenCaptures, 
                m_CaptureCamera);

            m_EntryPanel = new EntryPanel(m_DevLogEntries, m_ScreenCaptures);
            m_TwitterPanel = new TwitterPanel(m_EntryPanel);
            m_DiscordPanel = new DiscordPanel(m_EntryPanel);
            m_SchedulingPanel = new SchedulingPanel(m_DevLogEntries);
            m_GitPanel = new GitPanel(m_EntryPanel);
        }

        private void OnEnable()
        {
            EditorApplication.update += Update;
            m_MediaPanel.OnEnable();
            if (m_SchedulingPanel == null)
            {
                // For some reason the scheduling panel is occasionally set to null, this resets it
                // TODO remove this workaround by fixing the actual bug!!!
                m_SchedulingPanel = new SchedulingPanel(m_DevLogEntries);
            }
            m_SchedulingPanel.OnEnable();
        }

        private void OnDisable()
        {
            EditorApplication.update -= Update;
            m_MediaPanel.OnEnable();
            m_SchedulingPanel.OnDisable();
        }

        private void OnImageSelection()
        {
            m_MediaPanel.OnImageSelection();
        }

        void Update()
        {
            m_MediaPanel.Update();
            if (m_SchedulingPanel == null)
            {
                m_SchedulingPanel = new SchedulingPanel(m_DevLogEntries);
                m_SchedulingPanel.OnEnable();
            }
            m_SchedulingPanel.Update();
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
                m_MediaPanel.CaptureGif();
                startCapture = false;
            }
        }

        void OnGUI()
        {            
            try
            {
                selectedTab = GUILayout.Toolbar(selectedTab, toolbarLabels);
                switch (selectedTab)
                {
                    case 0:
                        if (m_DevLogEntries != null && m_ScreenCaptures != null)
                        {
                            m_EntryPanel.ScreenCaptures = m_ScreenCaptures;
                            m_EntryPanel.entries = m_DevLogEntries;
                            m_EntryPanel.OnGUI();

                            EditorGUILayout.Space();

                            m_TwitterPanel.screenCaptures = m_ScreenCaptures;
                            m_TwitterPanel.OnGUI();

                            EditorGUILayout.Space();

                            m_DiscordPanel.screenCaptures = m_ScreenCaptures;
                            m_DiscordPanel.OnGUI();

                            EditorGUILayout.Space();

                            m_MediaPanel.CaptureCamera = m_CaptureCamera;
                            m_MediaPanel.ScreenCaptures = m_ScreenCaptures;
                            m_MediaPanel.OnGUI();
                        }
                        else
                        {
                            SettingsTabUI();
                        }
                        break;
                    case 1:
                        if (m_DevLogPanel == null) m_DevLogPanel = new DevLogPanel(m_DevLogEntries);

                        m_DevLogPanel.ScreenCaptures = m_ScreenCaptures;
                        m_DevLogPanel.entries = m_DevLogEntries;
                        m_DevLogPanel.OnGUI();
                        break;
                    case 2:
                        m_SchedulingPanel.OnGUI();
                        break;
                    case 3:
                        m_GitPanel.OnGUI();
                        break;
                    case 4:
                        SettingsTabUI();
                        break;
                }
            } catch (InvalidCastException)
            {
                // this is a workaround. An exception is thrown when a new scene is loaded.
                Repaint();
            }
        }

        /// <summary>
        /// Display the settings UI.
        /// </summary>
        public void SettingsTabUI()
        {
            Skin.StartSection("Capture Storage", false);

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel("Captures Save Folder");
            string originalPath = Settings.CaptureFileFolderPath;
            Settings.CaptureFileFolderPath = EditorGUILayout.TextField(Settings.CaptureFileFolderPath, GUILayout.Height(40));
            if (GUILayout.Button("Browse"))
            {
                Settings.CaptureFileFolderPath = EditorUtility.OpenFolderPanel("Select a folder in which to save captures", Settings.CaptureFileFolderPath, "");
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel("File Organizaton");
            EditorGUILayout.BeginVertical();
            Settings.OrganizeCapturesByProject = EditorGUILayout.ToggleLeft("Organize in Project sub folders (e.g. 'root/Project')", Settings.OrganizeCapturesByProject);
            Settings.OrganizeCapturesByScene = EditorGUILayout.ToggleLeft("Organize in Scene sub folders (e.g. 'root/Project/Scene')", Settings.OrganizeCapturesByScene);
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
            string existingPath = AssetDatabase.GetAssetPath(m_DevLogEntries);
            m_DevLogEntries = EditorGUILayout.ObjectField(m_DevLogEntries, typeof(DevLogEntries), true) as DevLogEntries;
            if (m_DevLogEntries == null)
            {
                if (GUILayout.Button("Create"))
                {
                    string filename = "Assets/Dev Log " + Application.version + ".asset";
                    m_DevLogEntries = ScriptableObject.CreateInstance<DevLogEntries>();
                    AssetDatabase.CreateAsset(m_DevLogEntries, filename);
                    AssetDatabase.SaveAssets();
                }
            }
            string newPath = AssetDatabase.GetAssetPath(m_DevLogEntries);
            if (existingPath != newPath)
            {
                Settings.DevLogScriptableObjectPath = newPath;
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel("Screen Capture Storage");
            existingPath = AssetDatabase.GetAssetPath(m_ScreenCaptures);
            m_ScreenCaptures = EditorGUILayout.ObjectField(m_ScreenCaptures, typeof(DevLogScreenCaptures), true) as DevLogScreenCaptures;
            if (m_ScreenCaptures == null)
            {
                if (GUILayout.Button("Create"))
                {
                    string filename = "Assets/Screen Captures " + Application.version + ".asset";
                    m_ScreenCaptures = ScriptableObject.CreateInstance<DevLogScreenCaptures>();
                    AssetDatabase.CreateAsset(m_ScreenCaptures, filename);
                    AssetDatabase.SaveAssets();
                    Settings.CaptureFileFolderPath = filename;
                }
            }
            newPath = AssetDatabase.GetAssetPath(m_ScreenCaptures);
            if (existingPath != newPath)
            {
                Settings.ScreenCaptureScriptableObjectPath = newPath;
            }
            EditorGUILayout.EndHorizontal();
            Skin.EndSection();

            Skin.StartSection("Helpers (Dev Only)", false);

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Reset"))
            {
                if (EditorUtility.DisplayDialog("Reset Twitter OAuth Tokens?",
                    "Do you also want to clear the Twitter access tokens?",
                    "Yes", "Do Not Clear Them")) {
                    TwitterSettings.ClearAccessTokens();
                }
            }

            if (GUILayout.Button("Reset Meta Data"))
            {
                EntryPanelSettings.ResetMetaData();
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
 