using System;
using System.Collections.Generic;
using System.Text;
using UnityEditor;
using UnityEngine;
using WizardsCode.DevLog;
using WizardsCode.EditorUtils;

namespace WizardsCode.DevLogger
{
    /// <summary>
    /// The GUI Panel for a new DevLog Entry
    /// </summary>
    public static class EntryPanel
    {
        static Vector2 windowScrollPos;
        static string shortText = "";
        static string detailText = "";
        static string uiStatusText = "";
        static string gitCommit = "";
        static List<string> suggestedMetaData;
        static List<bool> selectedMetaData;
        static string newMetaDataItem;

        internal static void OnEnable()
        {
            int numOfHashtags = EditorPrefs.GetInt("numberOfSuggestedMetaData", 0);
            if (numOfHashtags == 0)
            {
                suggestedMetaData = new List<string>() { "#IndieGame", "#MadeWithUnity" };
                selectedMetaData = new List<bool> { true, true };
            }
            else
            {
                suggestedMetaData = new List<string>();
                selectedMetaData = new List<bool>();

                for (int i = 0; i < numOfHashtags; i++)
                {
                    suggestedMetaData.Add(EditorPrefs.GetString("suggestedMetaData_" + i));
                    selectedMetaData.Add(EditorPrefs.GetBool("selectedMetaData_" + i));
                }
            }
        }

        internal static void OnDisable()
        {
            // TODO Create a constants file for these preference names
            EditorPrefs.SetInt("numberOfSuggestedMetaData", suggestedMetaData.Count);
            for (int i = 0; i < suggestedMetaData.Count; i++)
            {
                EditorPrefs.SetString("suggestedMetaData_" + i, suggestedMetaData[i]);
                EditorPrefs.SetBool("selectedMetaData_" + i, selectedMetaData[i]);
            }
        }

        internal static void Populate(string hash, string description)
        {
            shortText = description;
            gitCommit = hash;
        }

        public static void OnGUI()
        {
            windowScrollPos = EditorGUILayout.BeginScrollView(windowScrollPos);

            Skin.StartSection("Log Entry", false);
            LogEntryGUI();
            Skin.EndSection();

            Skin.StartSection("Meta Data", false);
            MetaDataGUI();
            Skin.EndSection();

            Skin.StartSection("Posting", false);
            PostingGUI();
            Skin.EndSection();

            Skin.StartSection("Capture");
            MediaPanel.OnGUI();
            Skin.EndSection();

            EditorGUILayout.Space();
            TwitterPanel.OnGUI(shortText + GetSelectedMetaData());

            EditorGUILayout.EndScrollView();
        }

        /// <summary>
        /// Get a string containing all the selected hashtags for this tweet.
        /// </summary>
        /// <returns>Space separated list of hashtags</returns>
        public static string GetSelectedMetaData()
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < suggestedMetaData.Count; i++)
            {
                if (selectedMetaData[i])
                {
                    sb.Append(" ");
                    sb.Append(suggestedMetaData[i]);
                }
            }
            return sb.ToString();
        }

        private static void LogEntryGUI()
        {
            EditorStyles.textField.wordWrap = true;
            EditorGUILayout.LabelField("Short Entry (required)");
            shortText = EditorGUILayout.TextArea(shortText, GUILayout.Height(35));

            EditorGUILayout.LabelField("Long Entry (optional)");
            detailText = EditorGUILayout.TextArea(detailText, GUILayout.Height(100));
        }

        private static void MetaDataGUI() {
            EditorGUILayout.BeginHorizontal();

            EditorGUILayout.BeginVertical();
            for (int i = 0; i < suggestedMetaData.Count; i++)
            {
                selectedMetaData[i] = EditorGUILayout.ToggleLeft(suggestedMetaData[i], selectedMetaData[i]);
            }

            EditorGUILayout.BeginHorizontal();
            newMetaDataItem = EditorGUILayout.TextField(newMetaDataItem);
            if (GUILayout.Button("Add"))
            {
                suggestedMetaData.Add(newMetaDataItem);
                selectedMetaData.Add(true);
                newMetaDataItem = "";
            }
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();

            gitCommit = EditorGUILayout.TextField("Git Commit", gitCommit);
            EditorGUILayout.EndHorizontal();
        }

        private static void PostingGUI() {
            if (!string.IsNullOrEmpty(shortText))
            {
                EditorGUILayout.BeginHorizontal();
                
                bool hasSelection = false;
                for (int i = 0; i < MediaPanel.ImageSelection.Count; i++)
                {
                    if (MediaPanel.ImageSelection[i])
                    {
                        hasSelection = true;
                        break;
                    }
                }

                if (hasSelection)
                {
                    if (GUILayout.Button("Devlog (no Tweet) with selected image and text"))
                    {
                        AppendDevlog(true, false);
                    }
                }
                else
                {
                    if (GUILayout.Button("DevLog (no Tweet) with text only"))
                    {
                        AppendDevlog(false, false);
                    }
                }
                EditorGUILayout.EndHorizontal();
            }
            else
            {
                EditorGUILayout.LabelField("No valid actions at this time.");
            }

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Open Devlog"))
            {
                string filepath = DevLog.GetAbsoluteProjectDirectory() + DevLog.GetRelativeCurrentFilePath();
                System.Diagnostics.Process.Start(filepath);
            }

            if (GUILayout.Button("Open Media Folder"))
            {
                string filepath = DevLog.GetAbsoluteDirectory().Replace(@"/", @"\");
                System.Diagnostics.Process.Start("Explorer.exe", @"/open,""" + filepath);
            }
            EditorGUILayout.EndHorizontal();

        }

        public static void AppendDevlog(bool withImage, bool withTweet)
        {
            Entry entry = new Entry();

            entry.shortDescription = shortText;
            StringBuilder text = new StringBuilder(entry.shortDescription);

            if (!string.IsNullOrEmpty(gitCommit))
            {
                entry.commitHash = gitCommit;

                text.Append("\n\nGit Commit: " + gitCommit);
                gitCommit = "";
            }

            for (int i = 0; i < suggestedMetaData.Count; i++)
            {
                if (selectedMetaData[i])
                {
                    entry.metaData.Add(suggestedMetaData[i]);
                }
            }
            
            if (withTweet)
            {
                entry.tweeted = true;
                text.Append("\n\n[This DevLog entry was Tweeted.]");
            }

            if (withImage)
            {
                List<string> mediaFilePaths = new List<string>();
                for (int i = 0; i < MediaPanel.ImageSelection.Count; i++)
                {
                    if (MediaPanel.ImageSelection[i])
                    {
                        DevLogScreenCapture capture = EditorUtility.InstanceIDToObject(MediaPanel.LatestCaptures[i]) as DevLogScreenCapture;
                        mediaFilePaths.Add(capture.Filename);
                        entry.captures.Add(capture);
                    }
                }
                
                entry.longDescription = detailText;

                DevLog.Append(text.ToString(), detailText, mediaFilePaths);
            }
            else
            {
                entry.longDescription = detailText;
                DevLog.Append(text.ToString(), detailText);
            }

            DevLogPanel.DevLog.entries.Add(entry);

            shortText = "";
            detailText = "";
        }
    }
}
