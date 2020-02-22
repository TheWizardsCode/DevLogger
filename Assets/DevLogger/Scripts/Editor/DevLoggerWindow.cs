using System.Text;
using UnityEditor;
using UnityEngine;
using WizardsCode.Social;
using WizardsCode.uGIF;

namespace WizardsCode.DevLogger.Editor {

    /// <summary>
    /// The main DevLogger control window.
    /// </summary>
    public class DevLoggerWindow : EditorWindow
    {
        string[] suggestedHashTags = {  "#IndieGameDev", "#MadeWithUnity" };
        string logText = "";
        string messageText = "";

        [UnityEditor.MenuItem("Window/Wizards Code/Dev Logger")]
        public static void ShowWindow()
        {
            EditorWindow.GetWindow(typeof(DevLoggerWindow), false, Application.productName, true);
        }

        #region GUI
        void OnGUI()
        {
            if (string.IsNullOrEmpty(messageText))
            {
                EditorGUILayout.LabelField("Welcome to " + Application.productName + " v" + Application.version);
            }
            else
            {
                EditorGUILayout.LabelField(messageText);
            }

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
            EditorGUILayout.LabelField(string.Format("Tweet ({0} chars + {1} for selected hashtags = {2} chars)", logText.Length, GetSelectedHashtagLength(), GetFullTweetText().Length));
            if (!string.IsNullOrEmpty(logText) && GetFullTweetText().Length <= 140)
            {
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("Tweet (and DevLog) with text only"))
                {
                    if (Twitter.PublishTweet(GetFullTweetText(), out string response))
                    {
                        messageText = "Tweet sent succesfully";
                    }
                    AppendDevlog(false, true);
                }

                if (GUILayout.Button("Tweet (and DevLog) with selected image and text"))
                {
                    /**
                    string directory = Capture.GetProjectFilepath(); ;
                    string[] extensions = { "Image files", "png,jpg,gif" };
                    string mediaFilePath = EditorUtility.OpenFilePanelWithFilters("Select an Image", directory, extensions);
    **/
                    string mediaFilePath = Capture.GetLatestImagePath(imageSelection);
                    mediaFilePath = mediaFilePath.Substring(Capture.GetProjectFilepath().Length);
                    if (!string.IsNullOrEmpty(mediaFilePath))
                        if (!string.IsNullOrEmpty(mediaFilePath))
                    {
                        if (Twitter.PublishTweetWithMedia(GetFullTweetText(), mediaFilePath, out string response))
                        {
                            messageText = "Tweet with image sent succesfully";
                        }
                    }
                    AppendDevlog(false, true);
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
            return logText + GetSelectedHashTags();
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
                    _capture = Camera.main.gameObject.GetComponent<CaptureScreen>();
                    if (_capture == null)
                    {
                        _capture = Camera.main.gameObject.AddComponent<CaptureScreen>();
                    }
                }
                return _capture;
            }
        }

        int imageSelection;
        private void MediaGUI()
        {
            imageSelection = GUILayout.SelectionGrid(imageSelection, Capture.latestImages.ToArray(), 2, GUILayout.Height(160));

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Save Screenshot"))
            {
                string filename = Capture.SaveScreenshot();
                messageText = "Saving Screenshot to " + filename;
            }

            if (EditorApplication.isPlaying)
            {

                if (GUILayout.Button("Save Animated GIF"))
                {
                    Capture.frameRate = 30;
                    Capture.downscale = 4;
                    Capture.duration = 10;
                    Capture.CaptureAnimatedGIF();
                }
            }
            else
            {
                EditorGUILayout.LabelField("Enter play mode to capture an animated GIF.");
            }
            EditorGUILayout.EndHorizontal();
        }
        #endregion

        #region Log Entry
        private void LogEntryGUI()
        {
            EditorStyles.textField.wordWrap = true;
            logText = EditorGUILayout.TextArea(logText, GUILayout.Height(35));
            EditorGUILayout.LabelField("Hashtags: " + GetSelectedHashTags());

            if (!string.IsNullOrEmpty(logText))
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
        }

        private void AppendDevlog(bool withImage, bool withTweet)
        {
            string entry = logText;
            if (withTweet)
            {
                entry += "\n\n[This DevLog entry was Tweeted.]";
            }

            if (withImage)
            {
                string mediaFilePath = Capture.GetLatestImagePath(imageSelection);
                mediaFilePath = mediaFilePath.Substring(Capture.GetProjectFilepath().Length);
                if (!string.IsNullOrEmpty(mediaFilePath))
                {
                    DevLog.Append(entry, mediaFilePath);
                }
            }
            else
            {
                DevLog.Append(entry);
            }

            logText = "";
        }
        #endregion
    }
}