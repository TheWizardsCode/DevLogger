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

        /// <summary>
        /// Append an entry to the current DevLog.
        /// </summary>
        /// <param name="shortText">The short text content of this log entry.</param>
        /// <param name="mediaFilePath">The path to a media file to include as an image. If null no image will be included.</param>
        public static void Append(string shortText, string detailText, List<string> mediaFilePaths = null)
        {
            StringBuilder entry = new StringBuilder();

            Directory.CreateDirectory(STORAGE_DIRECTORY);

            if (!File.Exists(GetRelativeCurrentFilePath()))
            {
                entry.Append(GetDevLogIntro());
            }
            
            entry.Append(GetNewEntryHeading());
            entry.AppendLine(shortText);
            entry.AppendLine();

            if (!string.IsNullOrEmpty(detailText))
            {
                entry.AppendLine("### Details");
                entry.AppendLine(detailText);
                entry.AppendLine();
            }

            if (mediaFilePaths != null) {
                for (int i = 0; i < mediaFilePaths.Count; i++)
                {
                    entry.Append("![Screenshot](");
                    entry.Append(mediaFilePaths[i]);
                    entry.AppendLine(")");
                    entry.AppendLine();
                }
            }

            using (StreamWriter file = File.AppendText(GetRelativeCurrentFilePath()))
            {
                file.Write(entry.ToString());
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
