using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using WizardsCode.DevLog;
using WizardsCode.Social;

namespace WizardsCode.DevLogger
{
    public static class TwitterPanel
    {
        static bool showTwitter = false;
        private static string m_StatusText;

        public static void OnGUI(string tweet)
        {
            EditorGUILayout.BeginVertical("Box");
            showTwitter = EditorGUILayout.Foldout(showTwitter, "Twitter", EditorStyles.foldout);
            if (showTwitter)
            {
                if (Twitter.IsAuthenticated)
                {
                    EditorGUILayout.LabelField(tweet);
                    if (!string.IsNullOrEmpty(tweet) && tweet.Length <= 280)
                    {
                        m_StatusText = string.Format("Tweet is {0} chars. {1} of which is for hashtags)", tweet.Length, GetSelectedMetaDataLength());

                        EditorGUILayout.BeginHorizontal();
                        if (GUILayout.Button("Tweet (and DevLog) with text only"))
                        {
                            if (Twitter.PublishTweet(tweet, out string response))
                            {
                                m_StatusText = "Tweet sent succesfully";
                                EntryPanel.AppendDevlog(true, true);
                            }
                            else
                            {
                                Debug.LogError(response);
                            }
                        }

                        if (MediaPanel.LatestCaptures != null && MediaPanel.LatestCaptures.Count > 0)
                        {
                            if (GUILayout.Button("Tweet (and DevLog) with image(s) and text"))
                            {
                                List<string> mediaFilePaths = new List<string>();
                                for (int i = 0; i < MediaPanel.ImageSelection.Count; i++)
                                {
                                    if (MediaPanel.ImageSelection[i])
                                    {
                                        DevLogScreenCapture capture = EditorUtility.InstanceIDToObject(MediaPanel.LatestCaptures[i]) as DevLogScreenCapture;
                                        mediaFilePaths.Add(capture.GetRelativeImagePath());
                                    }
                                }

                                if (Twitter.PublishTweetWithMedia(tweet, mediaFilePaths, out string response))
                                {
                                    m_StatusText = "Tweet with image(s) sent succesfully";
                                }
                                else
                                {
                                    Debug.LogError(response);
                                    m_StatusText = response;
                                }

                                EditorGUILayout.LabelField(m_StatusText);

                                EntryPanel.AppendDevlog(true, true);
                            }
                        }
                        EditorGUILayout.EndHorizontal();
                    }
                    else
                    {
                        EditorGUILayout.LabelField("No valid actions at this time.");
                    }
                }
                else
                {
                    OnAuthorizeTwitterGUI();
                }
            }
            EditorGUILayout.EndVertical();
        }

        private static int GetSelectedMetaDataLength()
        {
            return EntryPanel.GetSelectedMetaData().Length;
        }

        private static void OnAuthorizeTwitterGUI()
        {
            GUILayout.Label("Authorize on Twitter", EditorStyles.boldLabel);
            EditorPrefs.SetString(Twitter.EDITOR_PREFS_TWITTER_API_KEY, EditorGUILayout.TextField("Consumer API Key", EditorPrefs.GetString(Twitter.EDITOR_PREFS_TWITTER_API_KEY)));
            EditorPrefs.SetString(Twitter.EDITOR_PREFS_TWITTER_API_SECRET, EditorGUILayout.TextField("Consumer API Secret", EditorPrefs.GetString(Twitter.EDITOR_PREFS_TWITTER_API_SECRET)));
            EditorPrefs.SetString(Twitter.EDITOR_PREFS_TWITTER_ACCESS_TOKEN, EditorGUILayout.TextField("Acess Token", EditorPrefs.GetString(Twitter.EDITOR_PREFS_TWITTER_ACCESS_TOKEN)));
            EditorPrefs.SetString(Twitter.EDITOR_PREFS_TWITTER_ACCESS_SECRET, EditorGUILayout.TextField("Access Secret", EditorPrefs.GetString(Twitter.EDITOR_PREFS_TWITTER_ACCESS_SECRET)));
        }
    }
}