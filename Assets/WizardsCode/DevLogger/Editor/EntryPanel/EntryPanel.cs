using System;
using System.Collections.Generic;
using System.Text;
using UnityEditor;
using UnityEngine;
using WizardsCode.EditorUtils;

namespace WizardsCode.DevLogger
{
    /// <summary>
    /// The GUI Panel for a new DevLog Entry
    /// </summary>
    [Serializable]
    public class EntryPanel
    {
        [SerializeField] Vector2 windowScrollPos = Vector2.zero;
        [SerializeField] internal string shortText = "";
        [SerializeField] internal string detailText = "";
        [SerializeField] bool isSocial = false;
        [SerializeField] string gitCommit = "";
        [SerializeField] List<string> suggestedMetaData;
        [SerializeField] List<bool> selectedMetaData;
        [SerializeField] string newMetaDataItem;

        public EntryPanel(DevLogEntries entries, DevLogScreenCaptures screenCaptures)
        {
            this.entries = entries;
            ScreenCaptures = screenCaptures;
        }

        internal DevLogEntries entries { get; set; }
        internal DevLogScreenCaptures ScreenCaptures { get; set; }

        internal void OnEnable()
        {
            int numOfHashtags = EntryPanelSettings.MetaDataItemCount;
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
                    suggestedMetaData.Add(EntryPanelSettings.GetSuggestedMetaDataItem(i));
                    selectedMetaData.Add(EntryPanelSettings.GetMetaDataSelectionStatus(i));
                }
            }
        }

        internal void OnDisable()
        {
            EntryPanelSettings.MetaDataItemCount = suggestedMetaData.Count;
            for (int i = 0; i < suggestedMetaData.Count; i++)
            {
                EntryPanelSettings.SetSuggestedMetaDataItem(i, suggestedMetaData[i]);
                EntryPanelSettings.SetMetaDataSelectionStatus(i, selectedMetaData[i]);
            }
        }

        internal void Populate(string hash, string description)
        {
            shortText = description;
            gitCommit = hash;
        }

        public void OnGUI()
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

            EditorGUILayout.EndScrollView();
        }

        /// <summary>
        /// Get a string containing all the selected hashtags for this tweet.
        /// </summary>
        /// <returns>Space separated list of hashtags</returns>
        public string GetSelectedMetaData(bool includeHashtags = true)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < suggestedMetaData.Count; i++)
            {
                if (selectedMetaData[i] && (includeHashtags || !suggestedMetaData[i].StartsWith("#")))
                {
                    sb.Append(" ");
                    sb.Append(suggestedMetaData[i]);
                }
            }
            return sb.ToString();
        }

        private void LogEntryGUI()
        {
            EditorStyles.textField.wordWrap = true;
            EditorGUILayout.LabelField("Short Entry (required)");
            shortText = EditorGUILayout.TextArea(shortText, GUILayout.Height(35));

            EditorGUILayout.LabelField("Long Entry (optional)");
            detailText = EditorGUILayout.TextArea(detailText, GUILayout.Height(100));
        }

        private void MetaDataGUI() {
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


            EditorGUILayout.BeginVertical();
            isSocial = EditorGUILayout.Toggle("Social?", isSocial);
            gitCommit = EditorGUILayout.TextField("Git Commit", gitCommit);
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();
        }

        private void PostingGUI() {
            if (!string.IsNullOrEmpty(shortText))
            {
                EditorGUILayout.BeginHorizontal();

                bool hasSelection = false;
                for (int i = 0; i < ScreenCaptures.Count; i++)
                {
                    if (ScreenCaptures.captures[i])
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
        }

        public void AppendDevlog(bool withImage, bool withTweet)
        {
            DevLogEntry entry = ScriptableObject.CreateInstance<DevLogEntry>();
            entry.name = DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToShortTimeString();

            entry.shortDescription = shortText;
            StringBuilder text = new StringBuilder(entry.shortDescription);

            entry.isSocial = isSocial;

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
                entry.lastTweetFileTime = DateTime.Now.ToFileTimeUtc();
                text.Append("\n\n[This DevLog entry was Tweeted at " + entry.lastTweetPrettyTime + ".]");
            }

            if (withImage)
            {
                List<string> mediaFilePaths = new List<string>();
                for (int i = 0; i < ScreenCaptures.Count; i++)
                {
                    if (ScreenCaptures.captures[i].IsSelected)
                    {
                        DevLogScreenCapture capture = ScreenCaptures.captures[i];
                        entry.captures.Add(capture);
                    }
                }

                entry.longDescription = detailText;
            }
            else
            {
                entry.longDescription = detailText;
            }

            entries.AddEntry(entry);
            AssetDatabase.AddObjectToAsset(entry, entries);
            EditorUtility.SetDirty(entries);
            AssetDatabase.SaveAssets();

            shortText = "";
            detailText = "";
        }
    }
}
