using System.Text;
using uGIF;
using UnityEditor;
using UnityEngine;
using WizardsCode.Social;

namespace WizardsCode.DevLogger.Editor {

    /// <summary>
    /// The main DevLogger control window.
    /// </summary>
    public class DevLoggerWindow : EditorWindow
    {
        string[] suggestedHashTags = {  "#IndieGameDev", "#MadeWithUnity" };
        string tweetText = "";
        string messageText = "";
        string mediaFilePath;

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

                StartSection("Media Capture");
                MediaGUI();
                EndSection();

                StartSection("Twitter");
                TwitterGUI();
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
            EditorGUILayout.LabelField(string.Format("Tweet ({0} chars + {1} for selected hashtags = {2} chars)", tweetText.Length, GetSelectedHashtagLength(), GetFullTweetText().Length));
            if (!string.IsNullOrEmpty(tweetText) && GetFullTweetText().Length <= 140)
            {
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("Tweet with text only"))
                {
                    if (Twitter.PublishTweet(GetFullTweetText(), out string response))
                    {
                        tweetText = "";
                        messageText = "Tweet sent succesfully";
                    }
                }

                if (GUILayout.Button("Tweet with image and text"))
                {
                    string directory = "D:\\images";
                    string mediaFilePath = EditorUtility.OpenFilePanel("Select an Image", directory, "gif");
                    Twitter.PublishTweetWithMedia(GetFullTweetText(), mediaFilePath, out string response);
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
            return tweetText + GetSelectedHashTags();
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
        CaptureToGIF _capture;
        public CaptureToGIF Capture
        {
            get
            {
                if (_capture == null)
                {
                    _capture = Camera.main.gameObject.GetComponent<CaptureToGIF>();
                    if (_capture == null)
                    {
                        _capture = Camera.main.gameObject.AddComponent<CaptureToGIF>();
                    }
                }
                return _capture;
            }
        }

        private void MediaGUI()
        {
            if (GUILayout.Button("Save Screenshot"))
            {
                Capture.HiResScreenShot();
            }

            if (EditorApplication.isPlaying)
            {

                if (GUILayout.Button("Save Animated GIF"))
                {
                    Capture.frameRate = 30;
                    Capture.downscale = 4;
                    Capture.duration = 10;
                    Capture.StartCapturing();
                }
            }
            else
            {
                EditorGUILayout.LabelField("Enter play mode to capture an animated GIF.");
            }
        }
        #endregion

        #region Log Entry
        private void LogEntryGUI()
        {
            EditorStyles.textField.wordWrap = true;
            tweetText = EditorGUILayout.TextArea(tweetText, GUILayout.Height(35));
            EditorGUILayout.LabelField("Hashtags: " + GetSelectedHashTags());
        }
        #endregion
    }
}