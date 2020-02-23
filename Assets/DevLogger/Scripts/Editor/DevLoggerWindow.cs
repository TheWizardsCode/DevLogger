﻿using System.Text;
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
        string shortText = "";
        string detailText = "";
        string uiMageText = "";

        [UnityEditor.MenuItem("Window/Wizards Code/Dev Logger")]
        public static void ShowWindow()
        {
            EditorWindow.GetWindow(typeof(DevLoggerWindow), false, "DevLog: " + Application.productName, true);
        }

        #region GUI
        void OnGUI()
        {
            if (string.IsNullOrEmpty(uiMageText))
            {
                EditorGUILayout.LabelField("Welcome to " + Application.productName + " v" + Application.version);
            }
            else
            {
                EditorGUILayout.LabelField(uiMageText);
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
            EditorGUILayout.LabelField(string.Format("Tweet ({0} chars + {1} for selected hashtags = {2} chars)", shortText.Length, GetSelectedHashtagLength(), GetFullTweetText().Length));
            if (!string.IsNullOrEmpty(shortText) && GetFullTweetText().Length <= 140)
            {
                EditorGUILayout.BeginHorizontal();
                if (Capture.latestImages != null)
                {
                    if (GUILayout.Button("Tweet (and DevLog) with text only"))
                    {
                        if (Twitter.PublishTweet(GetFullTweetText(), out string response))
                        {
                            uiMageText = "Tweet sent succesfully";
                        }
                        AppendDevlog(false, true);
                    }
                }

                if (Capture.latestImages != null)
                {
                    if (GUILayout.Button("Tweet (and DevLog) with selected image and text"))
                    {
                        /**
                        string directory = Capture.GetProjectFilepath(); ;
                        string[] extensions = { "Image files", "png,jpg,gif" };
                        string mediaFilePath = EditorUtility.OpenFilePanelWithFilters("Select an Image", directory, extensions);
        **/
                        string mediaFilePath = Capture.GetLatestImagePath(imageSelection);
                        mediaFilePath = mediaFilePath.Substring(Capture.GetImagesFilepath().Length);
                        if (!string.IsNullOrEmpty(mediaFilePath))
                            if (!string.IsNullOrEmpty(mediaFilePath))
                            {
                                if (Twitter.PublishTweetWithMedia(GetFullTweetText(), mediaFilePath, out string response))
                                {
                                    uiMageText = "Tweet with image sent succesfully";
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
            if (Capture.latestImages != null)
            {
                imageSelection = GUILayout.SelectionGrid(imageSelection, Capture.latestImages.ToArray(), 2, GUILayout.Height(160));
            }

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Save Screenshot"))
            {
                string filename = Capture.SaveScreenshot();
                uiMageText = "Saving Screenshot to " + filename;
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
                string mediaFilePath = Capture.GetLatestImagePath(imageSelection);
                mediaFilePath = mediaFilePath.Substring(Capture.GetImagesFilepath().Length);
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