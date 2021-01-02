using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using WizardsCode.DevLogger;
using WizardsCode.Social;

namespace WizardsCode.DevLogger
{
    [Serializable]
    public class TwitterPanel
    {
        [SerializeField] bool showTwitter = false;
        [SerializeField] EntryPanel entryPanel;
        string m_StatusText;

        internal DevLogScreenCaptures screenCaptures { get; set; }

        public TwitterPanel(EntryPanel entryPanel)
        {
            this.entryPanel = entryPanel;
        }

        public void OnGUI()
        {
            string tweet = entryPanel.shortText + entryPanel.GetSelectedMetaData();
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
                                entryPanel.AppendDevlog(true, true);
                            }
                            else
                            {
                                Debug.LogError(response);
                            }
                        }

                        if (screenCaptures != null && screenCaptures.Count > 0)
                        {
                            if (GUILayout.Button("Tweet (and DevLog) with image(s) and text"))
                            {
                                List<string> mediaFilePaths = new List<string>();
                                for (int i = 0; i < screenCaptures.Count; i++)
                                {
                                    if (screenCaptures.captures[i].IsSelected)
                                    {
                                        DevLogScreenCapture capture = screenCaptures.captures[i];
                                        mediaFilePaths.Add(capture.ImagePath);
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

                                entryPanel.AppendDevlog(true, true);
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

        private int GetSelectedMetaDataLength()
        {
            return entryPanel.GetSelectedMetaData().Length;
        }

        private void OnAuthorizeTwitterGUI()
        {
            GUILayout.Label("Authorize on Twitter", EditorStyles.boldLabel);
            TwitterSettings.ApiKey = EditorGUILayout.TextField("Consumer API Key", TwitterSettings.ApiKey);
            TwitterSettings.ApiSecret = EditorGUILayout.TextField("Consumer API Secret", TwitterSettings.ApiSecret);
            TwitterSettings.AccessToken = EditorGUILayout.TextField("Acess Token", TwitterSettings.AccessToken);
            TwitterSettings.AccessSecret = EditorGUILayout.TextField("Access Secret", TwitterSettings.ApiSecret);
        }
    }
}