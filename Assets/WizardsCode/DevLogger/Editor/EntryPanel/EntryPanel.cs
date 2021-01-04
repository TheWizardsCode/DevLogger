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
        [SerializeField] string newMetaDataItem;

        public EntryPanel(DevLogEntries entries, DevLogScreenCaptures screenCaptures)
        {
            this.entries = entries;
            ScreenCaptures = screenCaptures;
        }

        internal DevLogEntries entries { get; set; }
        internal DevLogScreenCaptures ScreenCaptures { get; set; }

        internal void Populate(string hash, string description)
        {
            shortText = description;
            gitCommit = hash;
        }

        public void OnGUI()
        {
            //windowScrollPos = EditorGUILayout.BeginScrollView(windowScrollPos);

            Skin.StartSection("Log Entry", false);
            LogEntryGUI();
            Skin.EndSection();

            Skin.StartSection("Meta Data", false);
            MetaDataGUI();
            Skin.EndSection();

            //EditorGUILayout.EndScrollView();
        }

        /// <summary>
        /// Get a string containing all the selected hashtags for this tweet.
        /// </summary>
        /// <returns>Space separated list of hashtags</returns>
        public string GetSelectedMetaData(bool includeHashtags = true)
        {
            StringBuilder sb = new StringBuilder();
            MetaDataItems items = EntryPanelSettings.GetSuggestedMetaDataItems();
            for (int i = 0; i < items.Count; i++)
            {
                if (includeHashtags || !items.GetItem(i).name.StartsWith("#"))
                {
                    sb.Append(" ");
                    sb.Append(items.GetItem(i).name);
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
            MetaDataItems items = EntryPanelSettings.GetSuggestedMetaDataItems();
            for (int i = 0; i < items.Count; i++)
            {
                MetaDataItem item = items.GetItem(i);
                item.IsSelected = EditorGUILayout.ToggleLeft(item.name, item.IsSelected);
            }

            EditorGUILayout.BeginHorizontal();
            newMetaDataItem = EditorGUILayout.TextField(newMetaDataItem);
            if (GUILayout.Button("Add"))
            {
                EntryPanelSettings.AddSuggestedMetaDataItem(new MetaDataItem(newMetaDataItem, true));
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

        internal void DevLogPostingGUI() {
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
                
                if (GUILayout.Button("Post Devlog Only"))
                {
                    AppendDevlog();
                }
                EditorGUILayout.EndHorizontal();
            }
            else
            {
                EditorGUILayout.LabelField("No valid actions at this time.");
            }
        }

        /// <summary>
        /// Append a devlog entry.
        /// </summary>
        /// <param name="withTweet">If true record that the entry was tweeted at the current time.</param>
        /// <param name="withDiscord">If true record that the entry was posted to discord at the current time.</param>
        /// <returns></returns>
        public DevLogEntry AppendDevlog(bool withTweet = false, bool withDiscord = false)
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

            MetaDataItems items = EntryPanelSettings.GetSuggestedMetaDataItems();
            for (int i = 0; i < items.Count; i++)
            {
                entry.metaData.Add(items.GetItem(i).name);
            }
            
            if (withTweet)
            {
                entry.tweeted = true;
                entry.lastTweetFileTime = DateTime.Now.ToFileTimeUtc();
                text.Append("\n\n[This DevLog entry was last Tweeted at " + entry.lastTweetPrettyTime + ".]");
            }

            if (withDiscord)
            {
                entry.discordPost = true;
                entry.lastDiscordPostFileTime = DateTime.Now.ToFileTimeUtc();
                text.Append("\n\n[This DevLog entry was last posted to Discord at " + entry.lastTweetPrettyTime + ".]");
            }

            List<string> mediaFilePaths = new List<string>();
            for (int i = 0; i < ScreenCaptures.Count; i++)
            {
                if (ScreenCaptures.captures[i].IsSelected)
                {
                    DevLogScreenCapture capture = ScreenCaptures.captures[i];
                    entry.captures.Add(capture);
                    ScreenCaptures.captures[i].IsSelected = false;
                }
            }
            entry.longDescription = detailText;

            entries.AddEntry(entry);
            AssetDatabase.AddObjectToAsset(entry, entries);
            EditorUtility.SetDirty(entries);
            AssetDatabase.SaveAssets();

            shortText = "";
            detailText = "";

            return entry;
        }
    }
}
