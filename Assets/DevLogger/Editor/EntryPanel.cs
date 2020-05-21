using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using WizardsCode.DevLogger;
using WizardsCode.EditorUtils;
using WizardsCode.Git;

namespace WizardsCode.DevLogger
{
    /// <summary>
    /// The GUI Panel for a new DevLog Entry
    /// </summary>
    [Serializable]
    public class EntryPanel
    {
        [SerializeField] Vector2 windowScrollPos;
        [SerializeField] internal string shortText = "";
        [SerializeField] string detailText = "";
        [SerializeField] string uiStatusText = "";
        [SerializeField] string gitCommit = "";
        [SerializeField] List<string> suggestedMetaData;
        [SerializeField] List<bool> selectedMetaData;
        [SerializeField] string newMetaDataItem;
        internal MediaPanel mediaPanel;

        public EntryPanel(MediaPanel mediaPanel)
        {
            this.mediaPanel = mediaPanel;
        }

        internal void OnEnable()
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

        internal void OnDisable()
        {
            // TODO Create a constants file for these preference names
            EditorPrefs.SetInt("numberOfSuggestedMetaData", suggestedMetaData.Count);
            for (int i = 0; i < suggestedMetaData.Count; i++)
            {
                EditorPrefs.SetString("suggestedMetaData_" + i, suggestedMetaData[i]);
                EditorPrefs.SetBool("selectedMetaData_" + i, selectedMetaData[i]);
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

            Skin.StartSection("Data");
            FoldersGUI();
            Skin.EndSection();

            EditorGUILayout.EndScrollView();
        }

        /// <summary>
        /// Get a string containing all the selected hashtags for this tweet.
        /// </summary>
        /// <returns>Space separated list of hashtags</returns>
        public string GetSelectedMetaData()
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


            EditorGUILayout.BeginHorizontal();
            gitCommit = EditorGUILayout.TextField("Git Commit", gitCommit);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndHorizontal();
        }

        private void PostingGUI() {
            if (!string.IsNullOrEmpty(shortText))
            {
                EditorGUILayout.BeginHorizontal();

                bool hasSelection = false;
                for (int i = 0; i < mediaPanel.ScreenCaptures.Count; i++)
                {
                    if (mediaPanel.ScreenCaptures.captures[i])
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

        private void FoldersGUI() {
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

        public void AppendDevlog(bool withImage, bool withTweet)
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
                for (int i = 0; i < mediaPanel.ScreenCaptures.Count; i++)
                {
                    if (mediaPanel.ScreenCaptures.captures[i].IsSelected)
                    {
                        DevLogScreenCapture capture = mediaPanel.ScreenCaptures.captures[i];
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
