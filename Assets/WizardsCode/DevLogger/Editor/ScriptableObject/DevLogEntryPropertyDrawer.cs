using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace WizardsCode.DevLogger
{
    [CustomPropertyDrawer(typeof(DevLogEntry))]
    public class DevLogEntryPropertyDrawer : PropertyDrawer
    {
        /*
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            var container = new VisualElement();

            var createdField = new PropertyField(property.FindPropertyRelative("created"));
            var shortDescriptionField = new PropertyField(property.FindPropertyRelative("shortDescription"));

            container.Add(createdField);
            container.Add(shortDescriptionField);

            return container;
        }
        */

        
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (property.objectReferenceValue == null)
            {
                return;
            }

            DevLogEntry entry = new SerializedObject(property.objectReferenceValue as DevLogEntry).targetObject as DevLogEntry;

            var indent = EditorGUI.indentLevel;
            EditorGUI.indentLevel++;

            GUIStyle wrappedLabel = new GUIStyle();
            wrappedLabel.wordWrap = true;

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel("Short Text");
            EditorGUILayout.LabelField(entry.shortDescription, EditorStyles.wordWrappedLabel);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel("Long Text");
            EditorGUILayout.LabelField(entry.longDescription, EditorStyles.wordWrappedLabel);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel("Meta Data");
            EditorGUILayout.BeginVertical();
            EditorGUI.indentLevel++;
            for (int i = 0; i < entry.metaData.Count; i++)
            {
                EditorGUI.indentLevel--;
                EditorGUILayout.LabelField(entry.metaData[i]);
                EditorGUI.indentLevel++;
            }
            EditorGUI.indentLevel--;
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel(new GUIContent("Captures"));
            EditorGUILayout.BeginVertical();
            EditorGUI.indentLevel++;
            for (int i = 0; i < entry.captures.Count; i++)
            {
                EditorGUI.indentLevel--;
                EditorGUILayout.LabelField(entry.captures[i].Filename);
                EditorGUI.indentLevel++;
            }
            EditorGUI.indentLevel--;
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();


            EditorGUI.indentLevel = indent;

            
        }
    
    }
}