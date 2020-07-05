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
        [SerializeField] string username = "Dev Logger (test)";
        [SerializeField] string url;


        internal DevLogScreenCaptures screenCaptures { get; set; }

        UnityWebRequest www;

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
                        Message message = new Message(username, entryPanel.shortText + entryPanel.GetSelectedMetaData(), screenCaptures);
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

        public void PostMessage(Message message)
        {
            List<IMultipartFormSection> formData = new List<IMultipartFormSection>();
            formData.Add(new MultipartFormDataSection("username", message.username));
            formData.Add(new MultipartFormDataSection("content", message.content));
            for (int i = 0; i < message.files.Count; i++)
            {
                formData.Add(new MultipartFormFileSection(message.files[i].name, message.files[i].fileData, message.files[i].filename, message.files[i].contentType));
            }
            www = UnityWebRequest.Post(url, formData);
            
            AsyncOperation asyncOperation = www.SendWebRequest();
            EditorApplication.update += PostRequestUpdate;
        }

        void PostRequestUpdate()
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
        public string content;
        public string username;
        public List<ImageContent> files = new List<ImageContent>();

        internal Message(string username, string content, DevLogScreenCaptures screenCaptures)
        {
            this.username = username;
            this.content = content;

            for (int i = 0; i < screenCaptures.Count; i++)
            {
                DevLogScreenCapture capture = screenCaptures.captures[i];
                if (!screenCaptures.captures[i].IsSelected)
                {
                    continue;
                }

                ImageContent image = new ImageContent();
                image.name = "File" + i;
                image.filepath = capture.ImagePath;
                image.filename = Path.GetFileName(capture.ImagePath);
                image.fileData = File.ReadAllBytes(image.filepath);
                if (image.filepath.EndsWith("jpg")) {
                    image.contentType = "image/jpg";
                } 
                else if (image.filepath.EndsWith("png"))
                {
                    image.contentType = "image/png";
                }

                files.Add(image);
            }
        }
    }

    [Serializable]
    public struct ImageContent
    {
        public string name;
        public string filepath;
        public string filename;
        public byte[] fileData;
        public string contentType;
    }
}
