﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using WizardsCode.DevLog;
using WizardsCode.DevLogger;
using WizardsCode.EditorUtils;

namespace WizardsCode.Git
{
    [Serializable]
    public class GitPanel
    {   
        [SerializeField] public bool isInitialized = false;
        [SerializeField] string status;
        [SerializeField] string statusError;
        [SerializeField] Vector2 statusScrollPos;
        [SerializeField] Vector2 logScrollPos;
        [SerializeField] string logError;

        [SerializeField] List<GitLogEntry> logEntries;
        [SerializeField] EntryPanel entryPanel;

        public GitPanel(EntryPanel entryPanel)
        {
            this.entryPanel = entryPanel;
        }

        public void OnGUI()
        {
            if (!isInitialized)
            {
                StatusBoxGUI();
            } else
            {
                StatusBoxGUI();
                LogGUI();
            }
        }

        private  void LogGUI()
        {
            Skin.StartSection("Log", false);
            if (logEntries != null)
            {
                logScrollPos = EditorGUILayout.BeginScrollView(logScrollPos);

                EditorGUILayout.BeginVertical();
                for (int i = 0; i < logEntries.Count; i++)
                {
                    Skin.StartHelpBox();
                    EditorGUILayout.BeginHorizontal();
                    string text = logEntries[i].hash + "\n" + logEntries[i].description;
                    EditorGUILayout.SelectableLabel(text, Skin.infoLabelStyle, GUILayout.ExpandWidth(true));
                    if (GUILayout.Button("DevLog", GUILayout.Width(60)))
                    {
                        entryPanel.Populate(logEntries[i].hash, logEntries[i].description);
                    }
                    EditorGUILayout.EndHorizontal();
                    Skin.EndHelpBox();
                }

                EditorGUILayout.EndVertical();

                EditorGUILayout.EndScrollView();


                EditorGUILayout.LabelField($"Last Log Update: {Git.LastLogUpdate}");
            }

            if (GUILayout.Button("Update Log"))
            {
                Task.Run(UpdateLog);
            }

            Skin.EndSection();
        }

        private  void StatusBoxGUI()
        {
            Skin.StartSection("Status", false);
            EditorGUILayout.LabelField($"Last Status Update: {Git.LastStatusUpdate}");

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel("Repository Path");
            GitSettings.RepositoryPath = EditorGUILayout.TextField(GitSettings.RepositoryPath);
            EditorGUILayout.EndHorizontal();

            if (!string.IsNullOrEmpty(status) && string.IsNullOrEmpty(statusError))
            {
                statusScrollPos = EditorGUILayout.BeginScrollView(statusScrollPos);

                EditorGUILayout.BeginVertical();
                EditorGUILayout.PrefixLabel("Status");
                EditorGUILayout.SelectableLabel(status, Skin.infoLabelStyle, GUILayout.MaxHeight(1500));
                EditorGUILayout.EndVertical();

                EditorGUILayout.EndScrollView();
            } else if (!string.IsNullOrEmpty(statusError))
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.PrefixLabel("Error");
                EditorGUILayout.SelectableLabel(statusError, Skin.errorLabelStyle, GUILayout.MaxHeight(200));
                EditorGUILayout.EndHorizontal();
            }

            if (GUILayout.Button("Update Status"))
            {
                Task.Run(UpdateStatus);
                Task.Run(UpdateLog);
            }

            Skin.EndSection();
        }

        private  async Task UpdateStatus()
        {
            statusScrollPos = Vector2.zero;
            try
            {
                status = await Git.Status();
                statusError = "";
                isInitialized = true;
            }
            catch (Exception error)
            {
                status = "";
                statusError = error.Message.Replace(",", ",\n");
                //Debug.LogError(error.StackTrace);
                isInitialized = false;
            }
        }

        public  int i { get; private set; }

        private  async Task UpdateLog()
        {
            logScrollPos = Vector2.zero;
            try
            {
                logEntries = new List<GitLogEntry>();

                string log = await Git.Log();

                string[] lines = log.Split(new[] { '\r', '\n' });
                for (int i = 0; i < lines.Length; i++)
                {;
                    logEntries.Add(GetEntryFromLogLine(lines[i]));
                }
            }
            catch (Exception error)
            {
                logError = error.Message;
            }
        }

        private  GitLogEntry GetEntryFromLogLine(string line)
        {
            string hash = line.Substring(0, 39);
            string description = line.Substring(41);
            return new GitLogEntry(hash, description);
        }

        internal  async Task<GitLogEntry> LatestLog()
        {
            string log = await Git.Log(1);
            return GetEntryFromLogLine(log);
        }
    }
}