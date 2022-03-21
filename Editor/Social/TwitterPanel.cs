using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace WizardsCode.DevLogger
{
    [Serializable]
    public class TwitterPanel
    {
        [SerializeField] bool showTwitter = false;

        public void OnSettingsGUI()
        {
            EditorGUILayout.BeginVertical("Box");
            showTwitter = EditorGUILayout.Foldout(showTwitter, "Twitter", EditorStyles.foldout);
            if (showTwitter)
            {
                OnAuthorizeTwitterGUI();
            }
            EditorGUILayout.EndVertical();
        }

        private void OnAuthorizeTwitterGUI()
        {
            GUILayout.Label("Authorize on Twitter", EditorStyles.boldLabel);
            TwitterSettings.ApiKey = EditorGUILayout.TextField("Consumer API Key", TwitterSettings.ApiKey);
            TwitterSettings.ApiSecret = EditorGUILayout.TextField("Consumer API Secret", TwitterSettings.ApiSecret);
            TwitterSettings.AccessToken = EditorGUILayout.TextField("Acess Token", TwitterSettings.AccessToken);
            TwitterSettings.AccessSecret = EditorGUILayout.TextField("Access Secret", TwitterSettings.AccessSecret);
        }
    }
}