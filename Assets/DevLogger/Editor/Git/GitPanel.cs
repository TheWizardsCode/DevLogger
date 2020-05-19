using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using WizardsCode.EditorUtils;

namespace WizardsCode.Git
{
    public static class GitPanel
    {

        private static string log;
        public static bool isInitialized = false;
        private static string status;
        private static string statusError;
        private static Vector2 statusScrollPos;
        private static Vector2 logScrollPos;

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

            logScrollPos = EditorGUILayout.BeginScrollView(logScrollPos);

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.SelectableLabel(log, Skin.infoLabelStyle, GUILayout.MaxHeight(200));
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.EndScrollView();


            EditorGUILayout.LabelField($"Last Log Update: {Git.LastLogUpdate}");

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

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.PrefixLabel("Status");
                EditorGUILayout.SelectableLabel(status, Skin.infoLabelStyle, GUILayout.MaxHeight(100));
                EditorGUILayout.EndHorizontal();

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

        private static async Task UpdateLog()
        {
            logScrollPos = Vector2.zero;
            try
            {
                log = await Git.Log();
            }
            catch (Exception error)
            {
                log = error.Message;
            }
        }
    }
}
