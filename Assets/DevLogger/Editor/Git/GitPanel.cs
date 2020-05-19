using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using WizardsCode.EditorUtils;

namespace WizardsCode.Git
{
    public static class GitPanel
    {   
        public static bool isInitialized = false;
        private static string status;
        private static string statusError;
        private static Vector2 statusScrollPos;
        private static Vector2 logScrollPos;
        private static string logError;

        public static void OnGUI()
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

        private static void LogGUI()
        {
            Skin.StartSection("Log", false);
            if (logEntries != null)
            {
                logScrollPos = EditorGUILayout.BeginScrollView(logScrollPos);

                EditorGUILayout.BeginVertical();
                for (int i = 0; i < logEntries.Count; i++)
                {
                    EditorGUILayout.BeginVertical();
                    string text = logEntries[i].hash + "\n" + logEntries[i].description;
                    EditorGUILayout.SelectableLabel(text, Skin.infoLabelStyle, GUILayout.ExpandWidth(true));
                    EditorGUILayout.EndVertical();
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

        private static void StatusBoxGUI()
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
            }

            Skin.EndSection();
        }

        private static async Task UpdateStatus()
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

        private static List<GitLogEntry> logEntries;
        
        private static async Task UpdateLog()
        {
            logScrollPos = Vector2.zero;
            try
            {
                logEntries = new List<GitLogEntry>();

                string log = await Git.Log();

                string[] lines = log.Split(new[] { '\r', '\n' });
                for (int i = 0; i < lines.Length; i++)
                {
                    string hash = lines[i].Substring(0, 39);
                    string description = lines[i].Substring(41);
                    logEntries.Add(new GitLogEntry(hash, description));
                }
            }
            catch (Exception error)
            {
                logError = error.Message;
            }
        }
    }
}
