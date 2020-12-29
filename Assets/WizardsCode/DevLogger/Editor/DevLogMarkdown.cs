using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

namespace WizardsCode.DevLogger
{
    /// <summary>
    /// Manages a DevLog markdown document. This is not the DevLog data strucutre (see 
    /// DevLogEntries). This is only concerned with writing the DevLog markdown file.
    /// </summary>
    public class DevLogMarkdown
    {
        public const string STORAGE_DIRECTORY = "DevLog/";

        /// <summary>
        /// Append and entry to the current DevLog markdown file. If the file does not
        /// yet exist then create it before appending this entry as the first entry.
        /// </summary>
        /// <param name="entry">The DevLogEntry to append.</param>
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
                    sb.Append(entry.captures[i].ImagePath);
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
            Directory.CreateDirectory(projectPath);
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
        /// Rewrite the markdown file because the data structure backing it has changed in a significant way.
        /// Note if you are simply appending to the DevLog there is no need to call this method, which rewrites
        /// the whole file. This is necessary when an entry is deleted or changed, or when the order of entries
        /// is changed in some way.
        /// </summary>
        internal static void Rewrite(DevLogEntries entries)
        {
            if (File.Exists(GetRelativeCurrentFilePath()))
            {
                File.Delete(GetRelativeCurrentFilePath());
            }

            for (int i = 0; i < entries.GetEntries().Count; i++)
            {
                Append(entries.GetEntry(i));
            }
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
