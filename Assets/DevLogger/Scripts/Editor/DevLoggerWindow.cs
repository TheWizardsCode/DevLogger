using System;
using System.Collections.Generic;
using System.Text;
using UnityEditor;
using UnityEngine;
using WizardsCode.DevLog;
using WizardsCode.Social;
using WizardsCode.uGIF;

namespace WizardsCode.DevLogger {

    /// <summary>
    /// The main DevLogger control window.
    /// </summary>
    public class DevLoggerWindow : EditorWindow
    {
        string[] suggestedHashTags = {  "#IndieGameDev", "#MadeWithUnity" };
        string shortText = "";
        string detailText = "";
        string uiImageText = "";

        private int maxImagesToRemember = 4;

        [UnityEditor.MenuItem("Tools/Wizards Code/Dev Logger")]
        public static void ShowWindow()
        {
            EditorWindow.GetWindow(typeof(DevLoggerWindow), false, "DevLog: " + Application.productName, true);
        }

        [SerializeField]
        private List<int> _latestCaptures;
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

        #region GUI
        void OnGUI()
        {
            if (!Twitter.IsAuthenticated)
            {
                OnAuthorizeTwitterGUI();
                return;
            } else
            {
                StartSection("Log Entry", false);
                LogEntryGUI();
                EndSection();

                StartSection("Twitter");
                TwitterGUI();
                EndSection();

                StartSection("Media Capture");
                MediaGUI();
                EndSection();
            }

            StartSection("Debug");
            DebugGUI();
            EndSection();
        }

        private void DebugGUI()
        {
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Reset"))
            {
                LatestCaptures = new List<int>();
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

            if (string.IsNullOrEmpty(uiImageText))
            {
                EditorGUILayout.LabelField("Welcome to " + Application.productName + " v" + Application.version);
            }
            else
            {
                EditorGUILayout.LabelField(uiImageText);
            }
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
            EditorGUILayout.LabelField(string.Format("Tweet ({0} chars + {1} for selected hashtags = {2} chars)", shortText.Length, GetSelectedHashtagLength(), GetFullTweetText().Length));
            if (!string.IsNullOrEmpty(shortText) && GetFullTweetText().Length <= 140)
            {
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("Tweet (and DevLog) with text only"))
                {
                    if (Twitter.PublishTweet(GetFullTweetText(), out string response))
                    {
                        uiImageText = "Tweet sent succesfully";
                    }
                    AppendDevlog(false, true);
                }

                if (LatestCaptures != null && LatestCaptures.Count > 0)
                {
                    if (GUILayout.Button("Tweet (and DevLog) with selected image and text"))
                    {
                        DevLogScreenCapture capture = EditorUtility.InstanceIDToObject(LatestCaptures[imageSelection]) as DevLogScreenCapture;
                        string mediaFilePath = capture.GetRelativeImagePath();
                        if (!string.IsNullOrEmpty(mediaFilePath))
                            if (!string.IsNullOrEmpty(mediaFilePath))
                            {
                                if (Twitter.PublishTweetWithMedia(GetFullTweetText(), mediaFilePath, out string response))
                                {
                                    uiImageText = "Tweet with image sent succesfully";
                                }
                            }
                        AppendDevlog(false, true);
                    }
                }
                EditorGUILayout.EndHorizontal();
            }
            else
            {
                EditorGUILayout.LabelField("No valid actions at this time.");
            }
        }

        private int GetSelectedHashtagLength()
        {
            return GetSelectedHashTags().Length;
        }

        /// <summary>
        /// Get a string containing all the selected hashtags for this tweet.
        /// </summary>
        /// <returns>Space separated list of hashtags</returns>
        private string GetSelectedHashTags()
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < suggestedHashTags.Length; i++)
            {
                sb.Append(" ");
                sb.Append(suggestedHashTags[i]);
            }
            return sb.ToString();
        }

        /// <summary>
        /// Get the Tweet in the form it would be sent.
        /// </summary>
        /// <returns>The tweet as it will be sent.</returns>
        private string GetFullTweetText()
        {
            return shortText + GetSelectedHashTags();
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
        CaptureScreen _capture;
        public CaptureScreen Capture
        {
            get
            {
                if (_capture == null)
                {
                    Camera camera = Camera.main;
                    if (camera == null)
                    {
                        camera = Camera.allCameras[0];
                    }

                    if (camera == null)
                    {
                        camera = Camera.current;
                    }

                    _capture = camera.gameObject.GetComponent<CaptureScreen>();
                    if (_capture == null)
                    {
                        _capture = camera.gameObject.AddComponent<CaptureScreen>();
                    }
                }
                return _capture;
            }
        }

        int imageSelection;
        private void MediaGUI()
        {
            if (LatestCaptures != null && LatestCaptures.Count > 0)
            {
                Texture2D[] imageTextures = new Texture2D[LatestCaptures.Count];
                for (int i = 0; i < LatestCaptures.Count; i++)
                {
                    DevLogScreenCapture capture = EditorUtility.InstanceIDToObject(LatestCaptures[i]) as DevLogScreenCapture;
                    imageTextures[i] = capture.Texture;
                }
                imageSelection = GUILayout.SelectionGrid(imageSelection, imageTextures, 2, GUILayout.Height(160));
            }

            EditorGUILayout.BeginHorizontal();
            if (Application.isPlaying)
            {
                if (GUILayout.Button("Game View"))
                {
                    CaptureScreen(DevLogScreenCapture.ImageEncoding.png);
                }

                if (GUILayout.Button("Animated GIF"))
                {
                    CaptureScreen(DevLogScreenCapture.ImageEncoding.gif);
                }
            } else
            {
                if (GUILayout.Button("Inspector"))
                {
                    CaptureWindowScreenshot("UnityEditor.InspectorWindow");
                }

                if (GUILayout.Button("Scene View"))
                {
                    CaptureWindowScreenshot("UnityEditor.SceneView");
                }

                if (GUILayout.Button("Game View"))
                {
                    //CaptureScreen(DevLogScreenCapture.ImageEncoding.png);
                    CaptureWindowScreenshot("UnityEditor.GameView");
                }
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
        /// UnityEditor.GameView
        /// UnityEditor.SceneView
        /// UnityEditor.AssetStoreWindow
        /// UnityEditor.TimelineWindow
        /// UnityEditor.ConsoleWindow
        /// UnityEditor.AnimationWindow
        /// UnityEditor.Graphs.AnimatorControllerTool
        /// UnityEditor.InspectorWindow
        /// UnityEditor.NavMeshEditorWindow
        /// UnityEditor.LightingWindow
        /// UnityEditor.SceneHierarchyWindow
        /// UnityEditor.InspectorWindow
        /// UnityEditor.ProjectBrowser
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
                screenCapture.Texture = windowTexture;

                byte[] bytes = windowTexture.EncodeToPNG();
                System.IO.File.WriteAllBytes(screenCapture.GetRelativeImagePath(), bytes);
                screenCapture.IsImageSaved = true;
                AddToLatestCaptures(screenCapture);
            };

            AssetDatabase.AddObjectToAsset(screenCapture, "Assets/Screen Captures.asset");
            AssetDatabase.SaveAssets();
        }

        private void CaptureScreen(DevLogScreenCapture.ImageEncoding encoding)
        {
            DevLogScreenCapture screenCapture = ScriptableObject.CreateInstance<DevLogScreenCapture>();
            screenCapture.Encoding = encoding;
            screenCapture.name = "Game";

            switch (encoding)
            {
                case DevLogScreenCapture.ImageEncoding.png:
                    Capture.CaptureScreenshot(ref screenCapture);
                    break;
                case DevLogScreenCapture.ImageEncoding.gif:
                    // FIXME: this information should be passed in using the screenCapture object
                    Capture.frameRate = 30;
                    Capture.downscale = 2;
                    Capture.duration = 10;
                    Capture.useBilinearScaling = true;
                    Capture.CaptureAnimatedGIF(ref screenCapture);
                    break;
            }

            AddToLatestCaptures(screenCapture);

            AssetDatabase.AddObjectToAsset(screenCapture, "Assets/Screen Captures.asset");
            AssetDatabase.SaveAssets();
        }

        private void AddToLatestCaptures(DevLogScreenCapture screenCapture)
        {
            if (screenCapture != null)
            {
                LatestCaptures.Insert(0, screenCapture.GetInstanceID());
                if (LatestCaptures.Count > maxImagesToRemember)
                {
                    LatestCaptures.RemoveAt(LatestCaptures.Count - 1);
                }
                uiImageText = "Captured as " + screenCapture.GetRelativeImagePath();
            }
            else
            {
                uiImageText = "Error capturing screen";
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
            detailText = EditorGUILayout.TextArea(detailText, GUILayout.Height(100)); ;

            EditorGUILayout.LabelField("Hashtags: " + GetSelectedHashTags());

            if (!string.IsNullOrEmpty(shortText))
            {
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("DevLog (no Tweet) with text only"))
                {
                    AppendDevlog(false, false);
                }

                if (GUILayout.Button("Devlog (no Tweet) with selected image and text"))
                {
                    AppendDevlog(true, false);
                }
                EditorGUILayout.EndHorizontal();
            }
            else
            {
                EditorGUILayout.LabelField("No valid actions at this time.");
            }

            if (GUILayout.Button("Open Devlog"))
            {
                string filepath = DevLog.GetAbsoluteProjectDirectory() + DevLog.GetRelativeCurrentFilePath();
                System.Diagnostics.Process.Start(filepath);
            }
        }

        private void AppendDevlog(bool withImage, bool withTweet)
        {
            StringBuilder entry = new StringBuilder(shortText);
            if (withTweet)
            {
                entry.Append("\n\n[This DevLog entry was Tweeted.]");
            }

            if (withImage)
            {
                DevLogScreenCapture capture = EditorUtility.InstanceIDToObject(LatestCaptures[imageSelection]) as DevLogScreenCapture;
                string mediaFilePath = capture.Filename;
                if (!string.IsNullOrEmpty(mediaFilePath))
                {
                    DevLog.Append(entry.ToString(), detailText, mediaFilePath);
                }
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