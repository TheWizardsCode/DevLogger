using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

namespace WizardsCode.DevLogger
{
    /// <summary>
    /// Manages a DevLog document.
    /// </summary>
    public class DevLog
    {
        public const string STORAGE_DIRECTORY = "DevLog/";

        public static void Append(DevLogEntry entry)
        {
            StringBuilder sb = new StringBuilder();

            Directory.CreateDirectory(STORAGE_DIRECTORY);

            if (!File.Exists(GetRelativeCurrentFilePath()))
            {
                sb.Append(GetDevLogIntro());
            }

            sb.Append(GetNewEntryHeading());
            sb.AppendLine(entry.shortDescription);
            sb.AppendLine();

            if (!string.IsNullOrEmpty(entry.longDescription))
            {
                sb.AppendLine("### Details");
                sb.AppendLine(entry.longDescription);
                sb.AppendLine();
            }

            if (entry.captures != null)
            {
                for (int i = 0; i < entry.captures.Count; i++)
                {
                    sb.Append("![Screenshot](");
                    sb.Append(entry.captures[i].GetRelativeImagePath());
                    sb.AppendLine(")");
                    sb.AppendLine();
                }
            }

            using (StreamWriter file = File.AppendText(GetRelativeCurrentFilePath()))
            {
                file.Write(sb.ToString());
                file.Close();
            }
        }

        private static string GetDevLogIntro()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("# Devlog for ");
            sb.Append(Application.productName);
            sb.Append(" Version ");
            sb.Append(Application.version);
            sb.AppendLine();
            sb.AppendLine();
            return sb.ToString();
        }

        private static string GetNewEntryHeading()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("## ");
            sb.AppendLine(DateTime.Now.ToString("dddd d MMMM yyyy, HH:mm"));
            sb.AppendLine();
            return sb.ToString();
        }

        /// <summary>
        /// Get the path to the folder in which the project is stored
        /// </summary>
        /// <returns></returns>
        public static string GetAbsoluteProjectDirectory()
        {
            string projectPath = Application.dataPath;
            projectPath = projectPath.Replace("Assets", "");
            return projectPath;
        }

        /// <summary>
        /// Get the Absolute directory (including the full path) to the
        /// directory in which the DevLog is stored.
        /// </summary>
        /// <returns></returns>
        public static string GetAbsoluteDirectory()
        {
            return GetAbsoluteProjectDirectory() + STORAGE_DIRECTORY;
        }

        /// <summary>
        /// Get the current file path (including filename) relative to the project directory root.
        /// </summary>
        /// <returns></returns>
        public static string GetRelativeCurrentFilePath()
        {
            StringBuilder sb = new StringBuilder(STORAGE_DIRECTORY);
            sb.Append("Devlog for ");
            sb.Append(Application.productName);
            sb.Append(" v");
            sb.Append(Application.version);
            sb.Append(".md");
            return sb.ToString();
        }
    }
}
