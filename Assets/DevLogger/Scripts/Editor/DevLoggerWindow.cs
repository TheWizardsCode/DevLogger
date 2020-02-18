using System.Text;
using UnityEditor;
using UnityEngine;
using WizardsCode.Social;

namespace WizardsCode.DevLogger.Editor {

    /// <summary>
    /// The main DevLogger control window.
    /// </summary>
    public class DevLoggerWindow : EditorWindow
    {
        public const string VERSION = "0.1";

        string[] suggestedHashTags = {  "#IndieGameDev", "#MadeWithUnity" };
        string tweetText = "";
        string messageText = "Welcome to DevLogger " + VERSION;
        string mediaFilePath;

        [UnityEditor.MenuItem("Window/Wizards Code/Dev Logger")]
        public static void ShowWindow()
        {
            EditorWindow.GetWindow(typeof(DevLoggerWindow), false, "Dev Logger " + VERSION, true);
        }

        void OnGUI()
        {
            EditorGUILayout.LabelField("Welcome to DevLogger " + VERSION);

            if (!Twitter.IsAuthenticated)
            {
                OnAuthorizeTwitterGUI();
                return;
            } else
            {
                StartSection("Log Entry", false);
                TweetGUI();
                EndSection();

                StartSection("Media");
                MediaGUI();
                EndSection();

                StartSection("Actions");
                ActionButtonsGUI();
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

        private void ActionButtonsGUI()
        {

            if (!string.IsNullOrEmpty(tweetText) && GetFullTweetText().Length <= 140)
            {
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("Tweet text only"))
                {
                    if (Twitter.PublishTweet(GetFullTweetText(), out string response))
                    {
                        tweetText = "";
                        messageText = "Tweet sent succesfully";
                    }
                }

                if (GUILayout.Button("Tweet with image"))
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

        private void MediaGUI()
        {
            if (GUILayout.Button("Capture Image from Game Window"))
            {
                CaptureGIF.Capture();
            } 
        }

        private void TweetGUI()
        {
            EditorStyles.textField.wordWrap = true;
            tweetText = EditorGUILayout.TextArea(tweetText, GUILayout.Height(35));
            EditorGUILayout.LabelField("Hashtags: " + GetSelectedHashTags());
            EditorGUILayout.LabelField(string.Format("Tweet ({0} chars + {1} for selected hashtags = {2} chars)", tweetText.Length, GetSelectedHashtagLength(), GetFullTweetText().Length));
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
    }
}