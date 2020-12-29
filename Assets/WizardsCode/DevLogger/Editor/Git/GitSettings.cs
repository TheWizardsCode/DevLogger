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
            get
            {
                if (string.IsNullOrEmpty(m_RepositoryPath)) {
                    m_RepositoryPath = Application.dataPath.Substring(0, Application.dataPath.Length - "Assets".Length);
                }
                return m_RepositoryPath;
            }
            set
            {
                m_RepositoryPath = value;
            }
        }

        public static void Save()
        {
            EditorPrefs.SetString(GitConstants.GIT_PATH, GitPath);
            EditorPrefs.SetString(GitConstants.REPOSITORY_PATH, RepositoryPath);
        }

        public static void Load()
        {
            GitPath = EditorPrefs.GetString(GitConstants.GIT_PATH, DefaultGitPath);
            RepositoryPath = EditorPrefs.GetString(GitConstants.REPOSITORY_PATH, Application.dataPath.Substring(0, Application.dataPath.Length - "Assets".Length));
        }

        public static void Reset()
        {
            EditorPrefs.DeleteKey(GitConstants.GIT_PATH);
            EditorPrefs.DeleteKey(GitConstants.REPOSITORY_PATH);
        }
    }
}
