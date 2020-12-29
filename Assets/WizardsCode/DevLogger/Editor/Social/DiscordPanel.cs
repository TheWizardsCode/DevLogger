using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;
using System.Net;
using System.Collections.Specialized;
using System.IO;

namespace WizardsCode.DevLogger
{
    [Serializable]
    public class DiscordPanel
    {
        [SerializeField] bool isConfigured = false;
        [SerializeField] bool showDiscord = false;
        [SerializeField] EntryPanel entryPanel;
        [SerializeField] static string username = "Dev Logger (test)";
        [SerializeField] static string url;

        internal DevLogScreenCaptures screenCaptures { get; set; }

        static UnityWebRequest www;

        public DiscordPanel(EntryPanel entryPanel)
        {
            this.entryPanel = entryPanel;
        }

        internal void OnDisable()
        {
            EditorPrefs.SetBool(Discord.EDITOR_PREFS_DISCORD_IS_CONFIGURED + Application.productName, isConfigured);
            EditorPrefs.SetString(Discord.EDITOR_PREFS_DISCORD_USERNAME + Application.productName, username);
            EditorPrefs.SetString(Discord.EDITOR_PREFS_DISCORD_WEBHOOK_URL + Application.productName, url);
        }

        internal void OnEnable()
        {
            isConfigured = EditorPrefs.GetBool(Discord.EDITOR_PREFS_DISCORD_IS_CONFIGURED + Application.productName, false);
            username = EditorPrefs.GetString(Discord.EDITOR_PREFS_DISCORD_USERNAME + Application.productName, "Dev Logger");
            url = EditorPrefs.GetString(Discord.EDITOR_PREFS_DISCORD_WEBHOOK_URL + Application.productName);
        }

        public void OnGUI()
        {
            EditorGUILayout.BeginVertical("Box");
            showDiscord = EditorGUILayout.Foldout(showDiscord, "Discord", EditorStyles.foldout);
            if (showDiscord)
            {
                if (string.IsNullOrEmpty(url) || string.IsNullOrEmpty(username) || !isConfigured)
                {
                    isConfigured = false; 

                    EditorGUILayout.PrefixLabel("Bot Username");
                    username = EditorGUILayout.TextField(username);

                    EditorGUILayout.PrefixLabel("Webhook URL");
                    url = EditorGUILayout.TextField(url);

                    if (GUILayout.Button("Save"))
                    {
                        isConfigured = true;
                    }
                }
                else if (!string.IsNullOrEmpty(entryPanel.shortText))
                {
                    string buttonText = "Post to Discord";
                    if (screenCaptures != null && screenCaptures.Count > 0)
                    {
                        buttonText += " with images";
                    }

                    if (GUILayout.Button(buttonText))
                    {
                        Message message;
                        if (string.IsNullOrEmpty(entryPanel.detailText))
                        {
                            message = new Message(username, entryPanel.shortText + entryPanel.GetSelectedMetaData(false), screenCaptures);
                        } 
                        else
                        {
                            message = new Message(username, entryPanel.shortText + entryPanel.GetSelectedMetaData(false), entryPanel.detailText, screenCaptures);
                        }
                        
                        Discord.PostMessage(message);
                    }
                }
                else
                {
                    EditorGUILayout.LabelField("Nothing to post. Type something into the Short Text box.");
                }
            }
            EditorGUILayout.EndVertical();
        }
    }
}
