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
        
        [MenuItem("Window/Wizards Code/Dev Logger")]
        public static void ShowWindow()
        {
            EditorWindow.GetWindow(typeof(DevLoggerWindow), false, "Dev Logger " + VERSION, true);
        }

        string tweetText = "";
        void OnGUI()
        {
            if (!Twitter.IsAuthenticated)
            {
                OnAuthorizeTwitterGUI();
                return;
            } else
            {
                GUILayout.Label("Tweet", EditorStyles.boldLabel);
                TweetGUI();

                GUILayout.Label("Media", EditorStyles.boldLabel);
                MediaUploadGUI();
            }
        }

        string mediaFilePath;
        private void MediaUploadGUI()
        {
            if (GUILayout.Button("Tweet with image"))
            {
                string directory = "D:\\images";
                string mediaFilePath = EditorUtility.OpenFilePanel("Select an Image", directory, "gif");
                Twitter.PublishTweetWithMedia(GetFullTweetText(), mediaFilePath, out string response);
            }
        }

        private void TweetGUI()
        {
            EditorStyles.textField.wordWrap = true;
            tweetText = EditorGUILayout.TextArea(tweetText, GUILayout.Height(35));
            GUILayout.Label("Hashtags: " + GetSelectedHashTags());
            GUILayout.Label(string.Format("Tweet ({0} chars + {1} for selected hashtags = {2} chars)", tweetText.Length, GetSelectedHashtagLength(), GetFullTweetText().Length));

            if (!string.IsNullOrEmpty(tweetText) && GetFullTweetText().Length <= 140)
            {
                if (GUILayout.Button("Send Tweet"))
                {
                    Twitter.PublishTweet(GetFullTweetText(), out string response);
                }
            }
            else
            {
                GUILayout.Label("Enter a valid tweet.");
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
    }
}