﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using WizardsCode.DevLogger;
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
            Entries = entries;
        }

        internal DevLogEntries Entries { get; set; }
        internal DevLogScreenCaptures ScreenCaptures { get; set; }

        public void OnGUI()
        {
            ConfigureReorderableLogList();

            if (logList == null)
            {
                EditorGUILayout.LabelField("Setup your log in the 'Settings' Tab.");
            }
            else
            {
                if (logList.index >= 0)
                {
                    string response;
                    if (GUILayout.Button("Tweet Selected Entry", GUILayout.Height(50)))
                    {
                        if (Twitter.PublishTweet(Entries.entries[logList.index], out response)) {
                            Entries.entries[logList.index].tweeted = true;
                            Entries.entries[logList.index].lastTweetFileTime = DateTime.Now.ToFileTimeUtc();
                        } else
                        {
                            // TODO Handle failed tweet gracefully
                            Debug.LogWarning("Tweet failed. Not currently handling this gracefully. Response " + response);
                        }
                    }
                    if (GUILayout.Button("Post to Discord", GUILayout.Height(50)))
                    {
                        DiscordPanel.PostEntry(Entries.entries[logList.index]);
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
            if (Entries != lastEntriesList)
            {
                logList = new ReorderableList(Entries.entries, typeof(DevLogEntry), true, true, true, true);
                logList.drawElementCallback = DrawLogListElement;
                logList.drawHeaderCallback = DrawHeader;
                logList.elementHeightCallback = ElementHeightCallback;
                logList.onReorderCallback = SaveReorderedList;
                logList.displayAdd = false;

                lastEntriesList = Entries;
            }
            else if (Entries == null)
            {
                logList = null;
            }
        }

        private void SaveReorderedList(ReorderableList list)
        {
            EditorUtility.SetDirty(Entries);
            AssetDatabase.SaveAssets();
        }

        private float ElementHeightCallback(int index)
        {
            float height = EditorGUIUtility.singleLineHeight; // title
            height += EditorGUIUtility.singleLineHeight * listDescriptionLines; // descrption
            height += EditorGUIUtility.singleLineHeight * Entries.entries[index].metaData.Count; // meta data
            height += EditorGUIUtility.singleLineHeight * Entries.entries[index].captures.Count; // capture
            height += EditorGUIUtility.singleLineHeight;// Commit Hash
            height += EditorGUIUtility.singleLineHeight;// Tweeted
            height += EditorGUIUtility.singleLineHeight;// Date
            height += 10; // space
            return height;
        }

        private void DrawHeader(Rect rect)
        {
            EditorGUI.LabelField(rect, "Entries");
        }

        private void DrawLogListElement(Rect rect, int index, bool isActive, bool isFocused)
        {
            DevLogEntry entry = Entries.entries[index];

            Rect labelRect = new Rect(rect.x, rect.y, listLabelWidth, EditorGUIUtility.singleLineHeight);
            EditorGUI.PrefixLabel(labelRect, new GUIContent("Created"));
            Rect fieldRect = new Rect(labelRect.x + listLabelWidth, labelRect.y, rect.width - listLabelWidth, labelRect.height);
            EditorGUI.LabelField(fieldRect, entry.created.ToString("dd MMM yyyy"));

            labelRect = new Rect(labelRect.x, labelRect.y + EditorGUIUtility.singleLineHeight, labelRect.width, labelRect.height);
            EditorGUI.PrefixLabel(labelRect, new GUIContent("Title"));
            fieldRect = new Rect(fieldRect.x, labelRect.y, fieldRect.width, EditorGUIUtility.singleLineHeight * listDescriptionLines);
            EditorGUI.TextField(fieldRect, entry.shortDescription);

            labelRect = new Rect(labelRect.x, labelRect.y + EditorGUIUtility.singleLineHeight, labelRect.width, labelRect.height);
            EditorGUI.PrefixLabel(labelRect, new GUIContent("Description"));
            fieldRect = new Rect(fieldRect.x, labelRect.y, fieldRect.width, EditorGUIUtility.singleLineHeight * listDescriptionLines);
            EditorGUI.TextArea(fieldRect, entry.longDescription);

            labelRect = new Rect(labelRect.x, labelRect.y + EditorGUIUtility.singleLineHeight * listDescriptionLines, labelRect.width, labelRect.height);
            if (Entries.entries[index].metaData.Count != 0)
            {
                EditorGUI.PrefixLabel(labelRect, new GUIContent("Meta Data"));
                for (int i = 0; i < entry.metaData.Count; i++)
                {
                    fieldRect = new Rect(fieldRect.x, labelRect.y + EditorGUIUtility.singleLineHeight * i, fieldRect.width, EditorGUIUtility.singleLineHeight);
                    EditorGUI.TextField(fieldRect, entry.metaData[i]);
                }
            }

            labelRect = new Rect(labelRect.x, labelRect.y + EditorGUIUtility.singleLineHeight * Entries.entries[index].metaData.Count, labelRect.width, labelRect.height);
            if (Entries.entries[index].captures.Count != 0)
            {
                EditorGUI.PrefixLabel(labelRect, new GUIContent("Captures"));
                for (int i = 0; i < entry.captures.Count; i++)
                {
                    fieldRect = new Rect(fieldRect.x, labelRect.y + EditorGUIUtility.singleLineHeight * i, fieldRect.width, EditorGUIUtility.singleLineHeight);
                    EditorGUI.ObjectField(fieldRect, entry.captures[i], typeof(ScreenCapture), false);
                }
            }

            labelRect = new Rect(labelRect.x, labelRect.y + EditorGUIUtility.singleLineHeight * Entries.entries[index].captures.Count, labelRect.width, labelRect.height);
            EditorGUI.PrefixLabel(labelRect, new GUIContent("Commit"));
            fieldRect = new Rect(fieldRect.x, labelRect.y, fieldRect.width, EditorGUIUtility.singleLineHeight);
            EditorGUI.TextArea(fieldRect, entry.commitHash);

            labelRect = new Rect(labelRect.x, labelRect.y + EditorGUIUtility.singleLineHeight, labelRect.width, labelRect.height);
            EditorGUI.PrefixLabel(labelRect, new GUIContent("Tweeted"));
            fieldRect = new Rect(fieldRect.x, labelRect.y, fieldRect.width, EditorGUIUtility.singleLineHeight);
            EditorGUI.Toggle(fieldRect, entry.tweeted);
            if (entry.tweeted)
            {
                Rect timeRect = new Rect(fieldRect.x + 50, fieldRect.y, fieldRect.width - 50, fieldRect.height);
                EditorGUI.LabelField(timeRect, entry.lastTweetPrettyTime);
            }
            
        }
    }
}