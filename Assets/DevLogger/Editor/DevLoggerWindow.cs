using Moments;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
using WizardsCode.DevLog;
using WizardsCode.Social;

namespace WizardsCode.DevLogger
{

    /// <summary>
    /// The main DevLogger control window.
    /// </summary>
    public class DevLoggerWindow : EditorWindow
    {
        [SerializeField]
        private List<bool> availableImages = new List<bool>();
        [SerializeField]
        List<string> suggestedMetaData;
        [SerializeField]
        private List<bool> selectedMetaData;
        
        private const string DATABASE_PATH = "Assets/ScreenCaptures.asset";
        string shortText = "";
        string detailText = "";
        string uiStatusText = "";
        string gitCommit = "";

        private int maxImagesToRemember = 10;

        private Vector2 windowScrollPos;

        // Animated GIF setup
        bool preserveAspect = true; // Automatically compute height from the current aspect ratio
        int width = 360; // Width in pixels
        int fps = 16; // Height in pixels
        int bufferSize = 10; // Number of seconds to record
        int repeat = 0; // -1: no repeat, 0: infinite, >0: repeat count
        int quality = 15; // Quality of color quantization, lower = better but slower (min 1, max 100)


        [UnityEditor.MenuItem("Tools/Wizards Code/Dev Logger")]
        public static void ShowWindow()
        {
            EditorWindow.GetWindow(typeof(DevLoggerWindow), false, "DevLog: " + Application.productName, true);
        }

        [SerializeField]
        private List<int> _latestCaptures;
        private Recorder _recorder;
        private bool removeRecorder;
        private bool originalFinalBlitToCameraTarget;
        private bool m_IsSaving;

        private Recorder Recorder
        {
            get
            {
                if (_recorder == null && m_Camera)
                {
                    _recorder = m_Camera.GetComponent<Recorder>();
                    if (_recorder == null)
                    {
                        _recorder = m_Camera.gameObject.AddComponent<Recorder>();
                        _recorder.Init();

                        PostProcessLayer pp = Camera.main.GetComponent<PostProcessLayer>();
                        if (pp != null)
                        {
                            originalFinalBlitToCameraTarget = pp.finalBlitToCameraTarget;
                            pp.finalBlitToCameraTarget = false;
                        }
                        removeRecorder = true;
                    }
                    else
                    {
                        removeRecorder = false;
                    }
                }

                return _recorder;
            }
        }

        private void OnFileSaved(int arg1, string arg2)
        {
            m_IsSaving = false;
            _recorder.Record();
        }

        private void OnProcessingDone()
        {
            m_IsSaving = true;
        }

        public List<int> LatestCaptures
        {
            get
            {
                if (_latestCaptures == null)
                {
                    _latestCaptures = new List<int>();
                }
                return _latestCaptures;
            }
            set { _latestCaptures = value; }
        }

        private void OnEnable()
        {
            EditorApplication.update += Update;

            int numOfHashtags = EditorPrefs.GetInt("numberOfSuggestedMetaData", 0);
            if (numOfHashtags == 0)
            {
                suggestedMetaData = new List<string>() { "#IndieGame", "#MadeWithUnity" };
                selectedMetaData = new List<bool> { true, true };
            } else
            {
                suggestedMetaData = new List<string>();
                selectedMetaData = new List<bool>();

                for (int i = 0; i < numOfHashtags; i++)
                {
                    suggestedMetaData.Add(EditorPrefs.GetString("suggestedMetaData_" + i));
                    selectedMetaData.Add(EditorPrefs.GetBool("selectedMetaData_" + i));
                }
            }
        }

        private void OnDisable()
        {
            EditorApplication.update -= Update;

            EditorPrefs.SetInt("numberOfSuggestedMetaData", suggestedMetaData.Count);
            for (int i = 0; i < suggestedMetaData.Count; i++)
            {
                EditorPrefs.SetString("suggestedMetaData_" + i, suggestedMetaData[i]);
                EditorPrefs.SetBool("selectedMetaData_" + i, selectedMetaData[i]);
            }
        }

        private void OnDestroy()
        {
            if (removeRecorder)
            {
                DestroyImmediate(_recorder);
            }
            PostProcessLayer pp = Camera.main.GetComponent<PostProcessLayer>();
            if (pp != null)
            {
                pp.finalBlitToCameraTarget = originalFinalBlitToCameraTarget;
            }
        }

        DevLogScreenCapture currentScreenCapture;
        private bool showTwitter = false;
        private bool showSettings = false;
        private Vector2 mediaScrollPosition;
        private string newMetaDataItem;

        void Update()
        {
            if (Recorder.State == RecorderState.PreProcessing)
            {
                return;
            }

            if (currentScreenCapture != null && !m_IsSaving && !currentScreenCapture.IsImageSaved)
            {
                AddToLatestCaptures(currentScreenCapture);

                AssetDatabase.AddObjectToAsset(currentScreenCapture, DATABASE_PATH);
                AssetDatabase.SaveAssets();

                currentScreenCapture.IsImageSaved = true;
            }
        }

        #region GUI
        void OnGUI()
        {
            if (!m_Camera)
            {
                m_Camera = Camera.main;
            }

            if (m_Camera)
            {
                windowScrollPos = EditorGUILayout.BeginScrollView(windowScrollPos);

                StartSection("Log Entry", false);
                LogEntryGUI();
                EndSection();

                StartSection("Meta Data", false);
                MetaDataGUI();
                EndSection();

                StartSection("Posting", false);
                PostingGUI();
                EndSection();

                StartSection("Media");
                MediaListGUI();
                EndSection();

                StartSection("Capture");
                MediaCaptureGUI();
                EndSection();

                EditorGUILayout.Space();
                EditorGUILayout.BeginVertical("Box");
                showTwitter = EditorGUILayout.Foldout(showTwitter, "Twitter", EditorStyles.foldout);
                if (showTwitter)
                {
                    if (Twitter.IsAuthenticated)
                    {
                        TwitterGUI();
                    }
                    else
                    {
                        OnAuthorizeTwitterGUI();
                    }
                }
                EditorGUILayout.EndVertical();

                EditorGUILayout.Space();
                EditorGUILayout.BeginVertical("Box");
                showSettings = EditorGUILayout.Foldout(showSettings, "Settings", EditorStyles.foldout);
                if (showSettings)
                {
                    SettingsGUI();
                }
                EditorGUILayout.EndVertical();

                EditorGUILayout.EndScrollView();
            } else
            {
                showSettings = true;
                EditorGUILayout.BeginVertical("Box");
                showSettings = EditorGUILayout.Foldout(showSettings, "Settings", EditorStyles.foldout);
                if (showSettings)
                {
                    SettingsGUI();
                }
                EditorGUILayout.EndVertical();
            }
        }

        public Camera m_Camera;
        private void SettingsGUI()
        {
            EditorGUILayout.BeginVertical();
            if (!m_Camera)
            {
                EditorGUILayout.LabelField("No main camera in scene, please select tag a camera as MainCamera or select a camera here.");
            }
            EditorGUILayout.BeginHorizontal();
            m_Camera = (Camera)EditorGUILayout.ObjectField(m_Camera, typeof(Camera), true);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Reset"))
            {
                LatestCaptures = new List<int>();
                availableImages = new List<bool>();
                if (EditorUtility.DisplayDialog("Reset Twitter OAuth Tokens?",
                    "Do you also want to clear the Twitter access tokens?",
                    "Yes", "Do Not Clear Them")) {
                    Twitter.ClearAccessTokens();
                }
            }

            if (GUILayout.Button("Capture DevLogger"))
            {
                CaptureWindowScreenshot("WizardsCode.DevLogger.DevLoggerWindow");
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

            if (string.IsNullOrEmpty(uiStatusText))
            {
                EditorGUILayout.LabelField("Welcome to " + Application.productName + " v" + Application.version);
            }
            else
            {
                EditorGUILayout.LabelField(uiStatusText);
            }

            EditorGUILayout.EndVertical();
        }

        private static void StartSection(string title, bool withSpace = true)
        {
            if (withSpace)
            {
                EditorGUILayout.Space();
            }
            EditorGUILayout.BeginVertical("Box");
            EditorGUILayout.LabelField(title, EditorStyles.boldLabel);
        }

        private static void EndSection()
        {
            EditorGUILayout.EndVertical();
        }
        #endregion

        #region Twitter
        private void TwitterGUI()
        {
            EditorGUILayout.LabelField(GetFullTweetText());
            EditorGUILayout.LabelField(string.Format("Tweet ({0} chars + {1} for selected hashtags = {2} chars)", shortText.Length, GetSelectedMetaDataLength(), GetFullTweetText().Length));
            if (!string.IsNullOrEmpty(shortText) && GetFullTweetText().Length <= 280)
            {
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("Tweet (and DevLog) with text only"))
                {
                    if (Twitter.PublishTweet(GetFullTweetText(), out string response))
                    {
                        uiStatusText = "Tweet sent succesfully";
                        AppendDevlog(true, true);
                    } else
                    {
                        Debug.LogError(response);
                    }
                }

                if (LatestCaptures != null && LatestCaptures.Count > 0)
                {
                    if (GUILayout.Button("Tweet (and DevLog) with image(s) and text"))
                    {
                        List<string> mediaFilePaths = new List<string>();
                        for (int i = 0; i < availableImages.Count; i++)
                        {
                            if (availableImages[i])
                            {
                                DevLogScreenCapture capture = EditorUtility.InstanceIDToObject(LatestCaptures[i]) as DevLogScreenCapture;
                                mediaFilePaths.Add(capture.GetRelativeImagePath());
                            }
                        }

                        if (Twitter.PublishTweetWithMedia(GetFullTweetText(), mediaFilePaths, out string response))
                        {
                            uiStatusText = "Tweet with image(s) sent succesfully";
                        } else
                        {
                            Debug.LogError(response);
                            uiStatusText = response;
                        }
                        AppendDevlog(true, true);
                    }
                }
                EditorGUILayout.EndHorizontal();
            }
            else
            {
                EditorGUILayout.LabelField("No valid actions at this time.");
            }
        }

        private int GetSelectedMetaDataLength()
        {
            return GetSelectedMetaData().Length;
        }

        /// <summary>
        /// Get a string containing all the selected hashtags for this tweet.
        /// </summary>
        /// <returns>Space separated list of hashtags</returns>
        private string GetSelectedMetaData()
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < suggestedMetaData.Count; i++)
            {
                if (selectedMetaData[i])
                {
                    sb.Append(" ");
                    sb.Append(suggestedMetaData[i]);
                }
            }
            return sb.ToString();
        }

        /// <summary>
        /// Get the Tweet in the form it would be sent.
        /// </summary>
        /// <returns>The tweet as it will be sent.</returns>
        private string GetFullTweetText()
        {
            return shortText + GetSelectedMetaData();
        }

        private void OnAuthorizeTwitterGUI()
        {
            GUILayout.Label("Authorize on Twitter", EditorStyles.boldLabel);
            EditorPrefs.SetString(Twitter.EDITOR_PREFS_TWITTER_API_KEY, EditorGUILayout.TextField("Consumer API Key", EditorPrefs.GetString(Twitter.EDITOR_PREFS_TWITTER_API_KEY)));
            EditorPrefs.SetString(Twitter.EDITOR_PREFS_TWITTER_API_SECRET, EditorGUILayout.TextField("Consumer API Secret", EditorPrefs.GetString(Twitter.EDITOR_PREFS_TWITTER_API_SECRET)));
            EditorPrefs.SetString(Twitter.EDITOR_PREFS_TWITTER_ACCESS_TOKEN, EditorGUILayout.TextField("Acess Token", EditorPrefs.GetString(Twitter.EDITOR_PREFS_TWITTER_ACCESS_TOKEN)));
            EditorPrefs.SetString(Twitter.EDITOR_PREFS_TWITTER_ACCESS_SECRET, EditorGUILayout.TextField("Access Secret", EditorPrefs.GetString(Twitter.EDITOR_PREFS_TWITTER_ACCESS_SECRET)));
        }
        #endregion

        #region Media

        private void MediaListGUI()
        {
            mediaScrollPosition = EditorGUILayout.BeginScrollView(mediaScrollPosition, GUILayout.Height(140));
            if (LatestCaptures != null && LatestCaptures.Count > 0)
            {
                ImageSelectionGUI();
            }
            EditorGUILayout.EndScrollView();
        }

        private void MediaCaptureGUI() { 
            EditorGUILayout.Space();

            EditorGUILayout.BeginHorizontal();
            if (Application.isPlaying)
            {
                if (GUILayout.Button("Scene View"))
                {
                    CaptureWindowScreenshot("UnityEditor.SceneView");
                }

                if (GUILayout.Button("Game View"))
                {
                    CaptureWindowScreenshot("UnityEditor.GameView");
                }
                
                EditorGUILayout.BeginVertical();

                switch (Recorder.State)
                {
                    case RecorderState.Paused: // We are paused so start recording. This allows saving of the last X seconds
                        Recorder.Setup(preserveAspect, width, width / 2, fps, bufferSize, repeat, quality);
                        Recorder.Record();

                        EditorGUILayout.LabelField("Starting Recording");
                        break;
                    case RecorderState.Recording:
                        if (GUILayout.Button("Save Animated GIF"))
                        {
                            currentScreenCapture = ScriptableObject.CreateInstance<DevLogScreenCapture>();
                            currentScreenCapture.Encoding = DevLogScreenCapture.ImageEncoding.gif;
                            currentScreenCapture.name = "In Game Footage";
                            
                            Recorder.OnPreProcessingDone = OnProcessingDone;
                            Recorder.OnFileSaved = OnFileSaved;
                            
                            Recorder.SaveFolder = currentScreenCapture.GetAbsoluteImageFolder();
                            Recorder.Filename = currentScreenCapture.Filename;

                            Recorder.Save(true);
                        }
                        break;
                    case RecorderState.PreProcessing:
                        EditorGUILayout.LabelField("Processing");
                        break;
                }

                EditorGUILayout.BeginHorizontal();
                GUILayout.Label("Buffer (in seconds)");
                bufferSize = int.Parse(GUILayout.TextField(bufferSize.ToString()));
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                GUILayout.Label("Quality (lower is better)");
                quality = int.Parse(GUILayout.TextField(quality.ToString()));
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                GUILayout.Label("Width");
                width = int.Parse(GUILayout.TextField(width.ToString()));
                EditorGUILayout.EndHorizontal();
                
                EditorGUILayout.EndVertical();
            } else
            {
                EditorGUILayout.BeginVertical();
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("Hierarchy"))
                {
                    CaptureWindowScreenshot("UnityEditor.SceneHierarchyWindow");
                }

                if (GUILayout.Button("Inspector"))
                {
                    CaptureWindowScreenshot("UnityEditor.InspectorWindow");
                }

                if (GUILayout.Button("Project"))
                {
                    CaptureWindowScreenshot("UnityEditor.ProjectBrowser");
                }

                if (GUILayout.Button("Scene View"))
                {
                    CaptureWindowScreenshot("UnityEditor.SceneView");
                }

                if (GUILayout.Button("Game View"))
                {
                    CaptureWindowScreenshot("UnityEditor.GameView");
                }

                if (GUILayout.Button("Console"))
                {
                    CaptureWindowScreenshot("UnityEditor.ConsoleWindow");
                }
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.Space();
                EditorGUILayout.Space();

                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("Package Manager"))
                {
                    CaptureWindowScreenshot("UnityEditor.PackageManager.UI.PackageManagerWindow");
                }

                if (GUILayout.Button("Asset Store"))
                {
                    CaptureWindowScreenshot("UnityEditor.AssetStoreWindow");
                }

                if (GUILayout.Button("Project Settings"))
                {
                    CaptureWindowScreenshot("UnityEditor.ProjectSettingsWindow");
                }
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.EndVertical();
            }
            EditorGUILayout.EndHorizontal();
        }

        private void ImageSelectionGUI()
        {
            EditorGUILayout.BeginHorizontal();
            for (int i = LatestCaptures.Count - 1; i >= 0;  i--)
            {
                EditorGUILayout.BeginVertical();

                EditorGUILayout.BeginHorizontal();
                DevLogScreenCapture capture = EditorUtility.InstanceIDToObject(LatestCaptures[i]) as DevLogScreenCapture;
                if (capture == null)
                {
                    LatestCaptures.RemoveAt(i);
                    availableImages.RemoveAt(i);
                    i--;
                    continue;
                }
                if (GUILayout.Button(capture.Texture, GUILayout.Width(100), GUILayout.Height(100)))
                {
                    availableImages[i] = !availableImages[i];
                }
                availableImages[i] = EditorGUILayout.Toggle(availableImages[i]);
                EditorGUILayout.EndHorizontal();

                if (GUILayout.Button("View"))
                {
                    string filepath = (DevLog.GetAbsoluteDirectory() + capture.Filename).Replace(@"/", @"\");
                    System.Diagnostics.Process.Start("Explorer.exe", @"/open,""" + filepath);
                }

                EditorGUILayout.EndVertical();
            }
            EditorGUILayout.EndHorizontal();
        }

        /// <summary>
        /// Captures a screenshot of the desired editor window. To get a list of all the
        /// windows available in the editor use:
        /// ```
        /// EditorWindow[] allWindows = Resources.FindObjectsOfTypeAll<EditorWindow>();
        ///        foreach (EditorWindow window in allWindows)
        ///        {
        ///          Debug.Log(window);
        ///        }
        ///```
        /// </summary>
        /// <param name="windowName">The name of the window to be captured, for example:
        /// WizardsCode.DevLogger.DevLoggerWindow
        /// UnityEditor.AssetStoreWindow
        /// UnityEditor.TimelineWindow
        /// UnityEditor.AnimationWindow
        /// UnityEditor.Graphs.AnimatorControllerTool
        /// UnityEditor.NavMeshEditorWindow
        /// UnityEditor.LightingWindow
        /// </param>
        private void CaptureWindowScreenshot(string windowName)
        {
            DevLogScreenCapture screenCapture = ScriptableObject.CreateInstance<DevLogScreenCapture>();
            screenCapture.Encoding = DevLogScreenCapture.ImageEncoding.png;
            screenCapture.name = windowName;

            EditorWindow window;
            if (windowName.StartsWith("UnityEditor."))
            {
                window = EditorWindow.GetWindow(typeof(Editor).Assembly.GetType(windowName));
            }
            else
            {
                Type t = Type.GetType(windowName);
                window = EditorWindow.GetWindow(t);
            }
            window.Focus();

            EditorApplication.delayCall += () =>
            {
                int width = (int)window.position.width;
                int height = (int)window.position.height;
                Vector2 position = window.position.position;
                position.y += 18;

                Color[] pixels = UnityEditorInternal.InternalEditorUtility.ReadScreenPixel(position, width, height);

                Texture2D windowTexture = new Texture2D(width, height, TextureFormat.RGB24, false);
                windowTexture.SetPixels(pixels);

                byte[] bytes = windowTexture.EncodeToPNG();
                System.IO.File.WriteAllBytes(screenCapture.GetRelativeImagePath(), bytes);
                screenCapture.IsImageSaved = true;

                AddToLatestCaptures(screenCapture);

                AssetDatabase.AddObjectToAsset(screenCapture, DATABASE_PATH);
                AssetDatabase.SaveAssets();

                this.Focus();
            };
        }

        private void AddToLatestCaptures(DevLogScreenCapture screenCapture)
        {
            if (screenCapture != null)
            {
                if (LatestCaptures.Count >= maxImagesToRemember)
                {
                    // Deleted the olded, not selected image
                    for (int i = 0; i < availableImages.Count; i++) {
                        if (!availableImages[i])
                        {
                            availableImages.RemoveAt(i);
                            LatestCaptures.RemoveAt(i);
                            break;
                        }
                    }

                    // If we didn't delete one then delete the oldest
                    if (availableImages.Count >= maxImagesToRemember)
                    {
                        availableImages.RemoveAt(0);
                        LatestCaptures.RemoveAt(0);
                    }
                }
                LatestCaptures.Add(screenCapture.GetInstanceID());
                availableImages.Add(false);
                uiStatusText = "Captured as " + screenCapture.GetRelativeImagePath();
            }
            else
            {
                uiStatusText = "Error capturing screen";
            }
        }
        #endregion

        #region Log Entry
        private void LogEntryGUI()
        {
            EditorStyles.textField.wordWrap = true;
            EditorGUILayout.LabelField("Short Entry (required)");
            shortText = EditorGUILayout.TextArea(shortText, GUILayout.Height(35));

            EditorGUILayout.LabelField("Long Entry (optional)");
            detailText = EditorGUILayout.TextArea(detailText, GUILayout.Height(100));
        }

        private void MetaDataGUI() {
            EditorGUILayout.BeginHorizontal();

            EditorGUILayout.BeginVertical();
            for (int i = 0; i < suggestedMetaData.Count; i++)
            {
                selectedMetaData[i] = EditorGUILayout.ToggleLeft(suggestedMetaData[i], selectedMetaData[i]);
            }

            EditorGUILayout.BeginHorizontal();
            newMetaDataItem = EditorGUILayout.TextField(newMetaDataItem);
            if (GUILayout.Button("Add"))
            {
                suggestedMetaData.Add(newMetaDataItem);
                selectedMetaData.Add(true);
                newMetaDataItem = "";
            }
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();

            gitCommit = EditorGUILayout.TextField("Git Commit", gitCommit);
            EditorGUILayout.EndHorizontal();
        }

        private void PostingGUI() {
            if (!string.IsNullOrEmpty(shortText))
            {
                EditorGUILayout.BeginHorizontal();
                
                bool hasSelection = false;
                for (int i = 0; i < availableImages.Count; i++)
                {
                    if (availableImages[i])
                    {
                        hasSelection = true;
                        break;
                    }
                }

                if (hasSelection)
                {
                    if (GUILayout.Button("Devlog (no Tweet) with selected image and text"))
                    {
                        AppendDevlog(true, false);
                    }
                }
                else
                {
                    if (GUILayout.Button("DevLog (no Tweet) with text only"))
                    {
                        AppendDevlog(false, false);
                    }
                }
                EditorGUILayout.EndHorizontal();
            }
            else
            {
                EditorGUILayout.LabelField("No valid actions at this time.");
            }

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Open Devlog"))
            {
                string filepath = DevLog.GetAbsoluteProjectDirectory() + DevLog.GetRelativeCurrentFilePath();
                System.Diagnostics.Process.Start(filepath);
            }

            if (GUILayout.Button("Open Media Folder"))
            {
                string filepath = DevLog.GetAbsoluteDirectory().Replace(@"/", @"\");
                System.Diagnostics.Process.Start("Explorer.exe", @"/open,""" + filepath);
            }
            EditorGUILayout.EndHorizontal();

        }

        private void AppendDevlog(bool withImage, bool withTweet)
        {
            StringBuilder entry = new StringBuilder(shortText);
            if (!string.IsNullOrEmpty(gitCommit))
            {
                entry.Append("\n\nGit Commit: " + gitCommit);
                gitCommit = "";
            }

            if (withTweet)
            {
                entry.Append("\n\n[This DevLog entry was Tweeted.]");
            }

            if (withImage)
            {
                List<string> mediaFilePaths = new List<string>();
                for (int i = 0; i < availableImages.Count; i++)
                {
                    if (availableImages[i])
                    {
                        DevLogScreenCapture capture = EditorUtility.InstanceIDToObject(LatestCaptures[i]) as DevLogScreenCapture;
                        mediaFilePaths.Add(capture.Filename);
                    }
                }

                DevLog.Append(entry.ToString(), detailText, mediaFilePaths);
            }
            else
            {
                DevLog.Append(entry.ToString(), detailText);
            }

            shortText = "";
            detailText = "";
        }
        #endregion
    }
}