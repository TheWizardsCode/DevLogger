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
        [SerializeField] bool showDiscord = false;
        [SerializeField] EntryPanel entryPanel;

        internal DevLogScreenCaptures screenCaptures { get; set; }

        static UnityWebRequest www;

        public DiscordPanel(EntryPanel entryPanel)
        {
            this.entryPanel = entryPanel;
        }

        public void OnGUI()
        {
            EditorGUILayout.BeginVertical("Box");
            showDiscord = EditorGUILayout.Foldout(showDiscord, "Discord", EditorStyles.foldout);
            if (showDiscord)
            {
                if (string.IsNullOrEmpty(DiscordSettings.Username) || string.IsNullOrEmpty(DiscordSettings.Username) || !DiscordSettings.IsConfigured)
                {
                    DiscordSettings.IsConfigured = false; 

                    EditorGUILayout.PrefixLabel("Bot Username");
                    DiscordSettings.Username = EditorGUILayout.TextField(DiscordSettings.Username);

                    EditorGUILayout.PrefixLabel("Webhook URL");
                    DiscordSettings.WebHookURL = EditorGUILayout.TextField(DiscordSettings.WebHookURL);

                    if (GUILayout.Button("Save"))
                    {
                        DiscordSettings.IsConfigured = true;
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
                            message = new Message(DiscordSettings.Username, entryPanel.shortText + entryPanel.GetSelectedMetaData(false), screenCaptures);
                        } 
                        else
                        {
                            message = new Message(DiscordSettings.Username, entryPanel.shortText + entryPanel.GetSelectedMetaData(false), entryPanel.detailText, screenCaptures);
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
