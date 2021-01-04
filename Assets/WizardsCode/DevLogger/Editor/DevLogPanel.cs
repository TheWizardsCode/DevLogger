using System;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using WizardsCode.Social;

namespace WizardsCode.DevLogger
{
    public class DevLogPanel
    {   
        ReorderableList logList;
        float listLabelWidth = 80;
        int listDescriptionLines = 6;
        Vector2 listScrollPosition;

        public DevLogPanel(DevLogEntries entries)
        {
            this.entries = entries;
        }

        internal DevLogEntries entries { get; set; }
        internal DevLogScreenCaptureCollection ScreenCaptures { get; set; }

        public void OnGUI()
        {
            ConfigureReorderableLogList();

            if (logList == null)
            {
                EditorGUILayout.LabelField("Setup your log in the 'Settings' Tab.");
            }
            else
            {
                if (GUILayout.Button("View Devlog", GUILayout.Height(30)))
                {
                    string filepath = DevLogMarkdown.GetAbsoluteProjectDirectory() + DevLogMarkdown.GetRelativeCurrentFilePath();
                    System.Diagnostics.Process.Start(filepath);
                }

                if (logList.index >= 0)
                {
                    string response;
                    if (TwitterSettings.IsConfigured)
                    {
                        if (GUILayout.Button("Tweet Selected Entry", GUILayout.Height(30)))
                        {
                            if (Twitter.PublishTweet(entries.GetEntry(logList.index), out response))
                            {
                                entries.GetEntry(logList.index).tweeted = true;
                                entries.GetEntry(logList.index).lastTweetFileTime = DateTime.Now.ToFileTimeUtc();
                            }
                            else
                            {
                                // TODO Handle failed tweet gracefully
                                Debug.LogWarning("Tweet failed. Not currently handling this gracefully. Response " + response);
                            }
                        }
                    }

                    if (DiscordSettings.IsConfigured)
                    {
                        if (GUILayout.Button("Post selected to Discord", GUILayout.Height(30)))
                        {
                            Discord.PostEntry(entries.GetEntry(logList.index));
                            entries.GetEntry(logList.index).discordPost = true;
                            entries.GetEntry(logList.index).lastDiscordPostFileTime = DateTime.Now.ToFileTimeUtc();
                        }
                    }
                }

                listScrollPosition = EditorGUILayout.BeginScrollView(listScrollPosition);
                logList.DoLayoutList();
                EditorGUILayout.EndScrollView();
            }
        }

        private DevLogEntries lastEntriesList;
        private void ConfigureReorderableLogList()
        {
            if (entries != lastEntriesList)
            {
                logList = new ReorderableList(entries.GetEntries(), typeof(DevLogEntry), true, true, true, true);
                logList.drawElementCallback = DrawLogListElement;
                logList.drawHeaderCallback = DrawHeader;
                logList.elementHeightCallback = ElementHeightCallback;
                logList.onReorderCallback = SaveReorderedList;
                logList.displayAdd = false;

                lastEntriesList = entries;
            }
            else if (entries == null)
            {
                logList = null;
            }
        }

        private void SaveReorderedList(ReorderableList list)
        {
            EditorUtility.SetDirty(entries);
            DevLogMarkdown.Rewrite(entries);
            AssetDatabase.SaveAssets();
        }

        private float ElementHeightCallback(int index)
        {
            float height = EditorGUIUtility.singleLineHeight; // title
            height += EditorGUIUtility.singleLineHeight;// Date
            height += EditorGUIUtility.singleLineHeight * listDescriptionLines; // descrption
            height += EditorGUIUtility.singleLineHeight * entries.GetEntry(index).metaData.Count; // meta data
            height += EditorGUIUtility.singleLineHeight * entries.GetEntry(index).captures.Count; // capture
            height += EditorGUIUtility.singleLineHeight;// Commit Hash
            height += EditorGUIUtility.singleLineHeight;// Social Flag
            height += EditorGUIUtility.singleLineHeight;// Tweeted
            height += EditorGUIUtility.singleLineHeight;// Discord
            height += 10; // space
            return height;
        }

        private void DrawHeader(Rect rect)
        {
            EditorGUI.LabelField(rect, "Entries");
        }

        private void DrawLogListElement(Rect rect, int index, bool isActive, bool isFocused)
        {
            DevLogEntry entry = entries.GetEntry(index);

            Rect labelRect = new Rect(rect.x, rect.y, listLabelWidth, EditorGUIUtility.singleLineHeight);
            EditorGUI.PrefixLabel(labelRect, new GUIContent("Title"));
            Rect fieldRect = new Rect(labelRect.x + listLabelWidth, labelRect.y, rect.width - listLabelWidth, labelRect.height);
            entry.shortDescription = EditorGUI.TextField(fieldRect, entry.shortDescription);

            labelRect = new Rect(labelRect.x, labelRect.y + EditorGUIUtility.singleLineHeight, labelRect.width, labelRect.height);
            EditorGUI.PrefixLabel(labelRect, new GUIContent("Description"));
            fieldRect = new Rect(fieldRect.x, labelRect.y, fieldRect.width, EditorGUIUtility.singleLineHeight * listDescriptionLines);
            entry.longDescription = EditorGUI.TextArea(fieldRect, entry.longDescription);

            labelRect = new Rect(labelRect.x, labelRect.y + EditorGUIUtility.singleLineHeight * listDescriptionLines, labelRect.width, labelRect.height);
            EditorGUI.PrefixLabel(labelRect, new GUIContent("Created"));
            fieldRect = new Rect(fieldRect.x, labelRect.y, fieldRect.width, labelRect.height);
            EditorGUI.LabelField(fieldRect, entry.created.ToString("dd MMM yyyy"));

            labelRect = new Rect(labelRect.x, labelRect.y + EditorGUIUtility.singleLineHeight, labelRect.width, labelRect.height);
            if (entries.GetEntry(index).metaData.Count != 0)
            {
                EditorGUI.PrefixLabel(labelRect, new GUIContent("Meta Data"));
                for (int i = 0; i < entry.metaData.Count; i++)
                {
                    fieldRect = new Rect(fieldRect.x, labelRect.y + EditorGUIUtility.singleLineHeight * i, fieldRect.width, EditorGUIUtility.singleLineHeight);
                    entry.metaData[i] = EditorGUI.TextField(fieldRect, entry.metaData[i]);
                }
            }

            labelRect = new Rect(labelRect.x, labelRect.y + EditorGUIUtility.singleLineHeight * entries.GetEntry(index).metaData.Count, labelRect.width, labelRect.height);
            if (entries.GetEntry(index).captures.Count != 0)
            {
                EditorGUI.PrefixLabel(labelRect, new GUIContent("Captures"));
                for (int i = 0; i < entry.captures.Count; i++)
                {
                    fieldRect = new Rect(fieldRect.x, labelRect.y + EditorGUIUtility.singleLineHeight * i, fieldRect.width, EditorGUIUtility.singleLineHeight);
                    EditorGUI.ObjectField(fieldRect, entry.captures[i], typeof(ScreenCapture), false);
                }
            }

            labelRect = new Rect(labelRect.x, labelRect.y + EditorGUIUtility.singleLineHeight * entries.GetEntry(index).captures.Count, labelRect.width, labelRect.height);
            EditorGUI.PrefixLabel(labelRect, new GUIContent("Commit"));
            fieldRect = new Rect(fieldRect.x, labelRect.y, fieldRect.width, EditorGUIUtility.singleLineHeight);
            EditorGUI.TextArea(fieldRect, entry.commitHash);

            labelRect = new Rect(labelRect.x, labelRect.y + EditorGUIUtility.singleLineHeight, labelRect.width, labelRect.height);
            EditorGUI.PrefixLabel(labelRect, new GUIContent("Social?"));
            fieldRect = new Rect(fieldRect.x, labelRect.y, fieldRect.width, EditorGUIUtility.singleLineHeight);
            entry.isSocial = EditorGUI.Toggle(fieldRect, entry.isSocial);
            
            labelRect = new Rect(labelRect.x, labelRect.y + EditorGUIUtility.singleLineHeight, labelRect.width, labelRect.height);
            EditorGUI.PrefixLabel(labelRect, new GUIContent("Tweeted?"));
            fieldRect = new Rect(fieldRect.x, labelRect.y, fieldRect.width, EditorGUIUtility.singleLineHeight);
            EditorGUI.Toggle(fieldRect, entry.tweeted);
            if (entry.tweeted)
            {
                Rect timeRect = new Rect(fieldRect.x + 20, fieldRect.y, fieldRect.width - 50, fieldRect.height);
                EditorGUI.LabelField(timeRect, " most recent " + entry.lastTweetPrettyTime);
            }

            labelRect = new Rect(labelRect.x, labelRect.y + EditorGUIUtility.singleLineHeight, labelRect.width, labelRect.height);
            EditorGUI.PrefixLabel(labelRect, new GUIContent("Discord?"));
            fieldRect = new Rect(fieldRect.x, labelRect.y, fieldRect.width, EditorGUIUtility.singleLineHeight);
            EditorGUI.Toggle(fieldRect, entry.discordPost);
            if (entry.discordPost)
            {
                Rect timeRect = new Rect(fieldRect.x + 20, fieldRect.y, fieldRect.width - 50, fieldRect.height);
                EditorGUI.LabelField(timeRect, "most recent" + entry.lastDiscordPostPrettyTime);
            }
        }
    }
}
