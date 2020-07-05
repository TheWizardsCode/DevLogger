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

        public const string EDITOR_PREFS_DISCORD_IS_CONFIGURED = "DiscordIsConfigured_";
        public const string EDITOR_PREFS_DISCORD_USERNAME = "DiscordUsername_";
        public const string EDITOR_PREFS_DISCORD_WEBHOOK_URL = "DiscordWebhookURL_";

        internal void OnDisable()
        {
            EditorPrefs.SetBool(EDITOR_PREFS_DISCORD_IS_CONFIGURED + Application.productName, isConfigured);
            EditorPrefs.SetString(EDITOR_PREFS_DISCORD_USERNAME + Application.productName, username);
            EditorPrefs.SetString(EDITOR_PREFS_DISCORD_WEBHOOK_URL + Application.productName, url);
        }

        internal void OnEnable()
        {
            isConfigured = EditorPrefs.GetBool(EDITOR_PREFS_DISCORD_IS_CONFIGURED + Application.productName, false);
            username = EditorPrefs.GetString(EDITOR_PREFS_DISCORD_USERNAME + Application.productName, "Dev Logger");
            url = EditorPrefs.GetString(EDITOR_PREFS_DISCORD_WEBHOOK_URL + Application.productName, url);
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
                        
                        PostMessage(message);
                    }
                }
                else
                {
                    EditorGUILayout.LabelField("Nothing to post. Type something into the Short Text box.");
                }
            }
            EditorGUILayout.EndVertical();
        }

        public static void PostEntry(DevLogEntry entry)
        {
            PostMessage(new Message(username, entry));
        }

        private static void PostMessage(Message message)
        {
            List<IMultipartFormSection> formData = new List<IMultipartFormSection>();
            formData.Add(new MultipartFormDataSection("username", message.username));
            formData.Add(new MultipartFormDataSection("content", message.introText + "\n" + message.bodyText));
            for (int i = 0; i < message.files.Count; i++)
            {
                formData.Add(new MultipartFormFileSection(message.files[i].name, message.files[i].fileData, message.files[i].filename, message.files[i].contentType));
            }
            www = UnityWebRequest.Post(url, formData);
            
            AsyncOperation asyncOperation = www.SendWebRequest();
            EditorApplication.update += PostRequestUpdate;
        }

        static void PostRequestUpdate()
        {
            if (!www.isDone)
                return;

            if (www.isNetworkError || www.isHttpError)
            {
                Debug.LogError(www.error);
            }
            else
            {
                Debug.Log(www.downloadHandler.text);
            }
            www.Dispose();
            EditorApplication.update -= PostRequestUpdate;
        }
    }

    [Serializable]
    public class Message
    {
        public string introText;
        public string bodyText;
        public string username;
        public List<ImageContent> files = new List<ImageContent>();

        internal Message(string username, DevLogEntry entry)
        {
            this.username = username;

            this.introText = entry.shortDescription;
            for (int i = 0; i < entry.metaData.Count; i++)
            {
                if (!entry.metaData[i].StartsWith("#"))
                {
                    introText += " " + entry.metaData[i];
                }
            }

            this.bodyText = entry.longDescription;

            for (int i = 0; i < entry.captures.Count; i++)
            {
                DevLogScreenCapture capture = entry.captures[i];
                ImageContent image = new ImageContent("File" + i, capture);
                files.Add(image);
            }
        }

        /// <summary>
        /// Create a new Discord message.
        /// </summary>
        /// <param name="username">The unsername to use when posting.</param>
        /// <param name="introText">The intro text that will appear before the first image, if there is one.</param>
        /// <param name="bodyText">The main body text.</param>
        /// <param name="screenCaptures">The set of available and selected screen captures that may be shared as part of the message.</param>
        internal Message(string username, string introText, string bodyText, DevLogScreenCaptures screenCaptures) : this (username, introText, screenCaptures)
        {
            this.bodyText = bodyText;
        }

        /// <summary>
        /// Create a new Discord message.
        /// </summary>
        /// <param name="username">The unsername to use when posting.</param>
        /// <param name="introText">The intro text that will appear before the first image, if there is one.</param>
        /// <param name="screenCaptures">The set of available and selected screen captures that may be shared as part of the message.</param>
        internal Message(string username, string introText, DevLogScreenCaptures screenCaptures)
        {
            this.username = username;
            this.introText = introText;

            for (int i = 0; i < screenCaptures.Count; i++)
            {
                DevLogScreenCapture capture = screenCaptures.captures[i];
                if (!capture.IsSelected)
                {
                    continue;
                }

                ImageContent image = new ImageContent("File" + i, capture);
                files.Add(image);
            }
        }
    }

    [Serializable]
    public class ImageContent
    {
        public string name;
        public string filepath;
        public string filename;
        public byte[] fileData;
        public string contentType;

        internal ImageContent (string name, DevLogScreenCapture capture)
        {
            this.name = name;
            this.filepath = capture.ImagePath;
            this.filename = Path.GetFileName(capture.ImagePath);
            this.fileData = File.ReadAllBytes(this.filepath);
            if (this.filepath.EndsWith("jpg"))
            {
                this.contentType = "image/jpg";
            }
            else if (this.filepath.EndsWith("png"))
            {
                this.contentType = "image/png";
            }
            else if (this.filepath.EndsWith("gif"))
            {
                this.contentType = "image/gif";
            }
        }
    }
}
