using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using WizardsCode.DevLogger;

namespace WizardsCode.DevLog
{
    public static class DevLogPanel
    {
        public static DevLogEntries m_DevLog;
        static ReorderableList logList;
        static float listLabelWidth = 80;
        static int listDescriptionLines = 6;
        static Vector2 listScrollPosition;

        public static DevLogEntries DevLog
        {
            get { return m_DevLog; }
            set
            {
                if (m_DevLog  != value)
                {
                    m_DevLog = value;
                    ConfigureReorderableLogList();
                } else
                {
                    m_DevLog = value;
                }
            }
        }

        public static void OnGUI()
        {
            if (logList == null)
            {
                EditorGUILayout.LabelField("Setup your log in the 'Settings' Tab.");
            }
            else
            {
                listScrollPosition = EditorGUILayout.BeginScrollView(listScrollPosition);
                logList.DoLayoutList();
                EditorGUILayout.EndScrollView();
            }
        }

        internal static void OnEnable()
        {
            DevLog = AssetDatabase.LoadAssetAtPath(EditorPrefs.GetString("DevLogScriptableOjectPath_" + Application.productName), typeof(DevLogEntries)) as DevLogEntries;
            ConfigureReorderableLogList();
        }

        internal static void OnDisable()
        {
            EditorPrefs.SetString("DevLogScriptableOjectPath_" + Application.productName, AssetDatabase.GetAssetPath(DevLog));
        }

        private static void ConfigureReorderableLogList()
        {
            if (DevLog != null)
            {
                logList = new ReorderableList(DevLog.entries, typeof(DevLogEntry), true, true, true, true);
                logList.drawElementCallback = DrawLogListElement;
                logList.drawHeaderCallback = DrawHeader;
                logList.elementHeightCallback = ElementHeightCallback;
                logList.displayAdd = false;
            }
            else
            {
                logList = null;
            }
        }

        private static float ElementHeightCallback(int index)
        {
            float height = EditorGUIUtility.singleLineHeight; // title
            height += EditorGUIUtility.singleLineHeight * listDescriptionLines; // descrption
            height += EditorGUIUtility.singleLineHeight * DevLog.entries[index].metaData.Count; // meta data
            height += EditorGUIUtility.singleLineHeight * DevLog.entries[index].captures.Count; // capture
            height += EditorGUIUtility.singleLineHeight;// Commit Hash
            height += EditorGUIUtility.singleLineHeight;// Tweeted
            height += EditorGUIUtility.singleLineHeight;// Date
            height += 10; // space
            return height;
        }

        private static void DrawHeader(Rect rect)
        {
            EditorGUI.LabelField(rect, "Entries");
        }

        private static void DrawLogListElement(Rect rect, int index, bool isActive, bool isFocused)
        {
            Entry entry = DevLog.entries[index];

            Rect labelRect = new Rect(rect.x, rect.y, listLabelWidth, EditorGUIUtility.singleLineHeight);
            EditorGUI.PrefixLabel(labelRect, new GUIContent("Title"));
            Rect fieldRect = new Rect(labelRect.x + listLabelWidth, labelRect.y, rect.width - listLabelWidth, labelRect.height);
            EditorGUI.TextField(fieldRect, entry.shortDescription);

            labelRect = new Rect(labelRect.x, labelRect.y + EditorGUIUtility.singleLineHeight, labelRect.width, labelRect.height);
            EditorGUI.PrefixLabel(labelRect, new GUIContent("Description"));
            fieldRect = new Rect(fieldRect.x, labelRect.y, fieldRect.width, EditorGUIUtility.singleLineHeight * listDescriptionLines);
            EditorGUI.TextArea(fieldRect, entry.longDescription);

            labelRect = new Rect(labelRect.x, labelRect.y + EditorGUIUtility.singleLineHeight * listDescriptionLines, labelRect.width, labelRect.height);
            if (DevLog.entries[index].metaData.Count != 0)
            {
                EditorGUI.PrefixLabel(labelRect, new GUIContent("Meta Data"));
                for (int i = 0; i < entry.metaData.Count; i++)
                {
                    fieldRect = new Rect(fieldRect.x, labelRect.y + EditorGUIUtility.singleLineHeight * i, fieldRect.width, EditorGUIUtility.singleLineHeight);
                    EditorGUI.TextField(fieldRect, entry.metaData[i]);
                }
            }

            labelRect = new Rect(labelRect.x, labelRect.y + EditorGUIUtility.singleLineHeight * DevLog.entries[index].metaData.Count, labelRect.width, labelRect.height);
            if (DevLog.entries[index].captures.Count != 0)
            {
                EditorGUI.PrefixLabel(labelRect, new GUIContent("Captures"));
                for (int i = 0; i < entry.captures.Count; i++)
                {
                    fieldRect = new Rect(fieldRect.x, labelRect.y + EditorGUIUtility.singleLineHeight * i, fieldRect.width, EditorGUIUtility.singleLineHeight);
                    EditorGUI.TextField(fieldRect, entry.captures[i].Filename);
                }
            }

            labelRect = new Rect(labelRect.x, labelRect.y + EditorGUIUtility.singleLineHeight * DevLog.entries[index].captures.Count, labelRect.width, labelRect.height);
            EditorGUI.PrefixLabel(labelRect, new GUIContent("Commit"));
            fieldRect = new Rect(fieldRect.x, labelRect.y, fieldRect.width, EditorGUIUtility.singleLineHeight);
            EditorGUI.TextArea(fieldRect, entry.commitHash);

            labelRect = new Rect(labelRect.x, labelRect.y + EditorGUIUtility.singleLineHeight, labelRect.width, labelRect.height);
            EditorGUI.PrefixLabel(labelRect, new GUIContent("Tweeted"));
            fieldRect = new Rect(fieldRect.x, labelRect.y, fieldRect.width, EditorGUIUtility.singleLineHeight);
            EditorGUI.Toggle(fieldRect, entry.tweeted);

            labelRect = new Rect(labelRect.x, labelRect.y + EditorGUIUtility.singleLineHeight, labelRect.width, labelRect.height);
            EditorGUI.PrefixLabel(labelRect, new GUIContent("Created"));
            fieldRect = new Rect(fieldRect.x, labelRect.y, fieldRect.width, EditorGUIUtility.singleLineHeight);
            EditorGUI.LabelField(fieldRect, entry.created.ToString("dd MMM yyyy"));
        }
    }
}
