using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace WizardsCode.Git
{
    [InitializeOnLoad]
    public static class GitSettings
    {
        public static string DefaultGitPath => "C:\\Program Files\\Git\\bin\\git.exe";
        private static string m_GitPath;
        public static string GitPath
        {
            get { 
                return string.IsNullOrEmpty(m_GitPath) ? DefaultGitPath : m_GitPath; 
            }
            set
            {
                m_GitPath = value;
            }
        }

        private static string m_RepositoryPath;
        public static string RepositoryPath
        {
            get => m_RepositoryPath;
            set
            {
                m_RepositoryPath = value;
            }
        }

        static GitSettings() {
            if (string.IsNullOrEmpty(RepositoryPath))
            {
                RepositoryPath = Application.dataPath.Substring(0, Application.dataPath.Length - "Assets".Length);
            }
        }

        public static void Save()
        {
            EditorPrefs.SetString(GitConstants.GITPATH, GitPath);
        }

        public static void Load()
        {
            EditorPrefs.GetString(GitConstants.GITPATH, DefaultGitPath);
        }

        public static void Reset()
        {
            EditorPrefs.DeleteKey(GitConstants.GITPATH);
        }
    }
}
