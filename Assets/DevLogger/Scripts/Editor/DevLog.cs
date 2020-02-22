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
        /// <summary>
        /// Append an entry to the current DevLog.
        /// </summary>
        /// <param name="shortText">The short text content of this log entry.</param>
        /// <param name="mediaFilePath">The path to a media file to include as an image. If null no image will be included.</param>
        public static void Append(string shortText, string mediaFilePath = null)
        {
            StringBuilder entry = new StringBuilder();

            if (!File.Exists(GetCurrentFilePath()))
            {
                entry.Append(GetDevLogIntro());
            }
            
            entry.Append(GetNewEntryHeading());
            entry.AppendLine(shortText);
            entry.AppendLine();

            if (mediaFilePath != null) {
                entry.Append("![Screenshot](");
                entry.Append(mediaFilePath);
                entry.AppendLine(")");
                entry.AppendLine();
            }

            using (StreamWriter file = File.AppendText(GetCurrentFilePath()))
            {
                file.Write(entry.ToString());
                file.Close();
            }
        }

        private static string GetDevLogIntro()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine();
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

        public static string GetCurrentFilePath()
        {
            StringBuilder sb = new StringBuilder("DevLog/Devlog for ");
            sb.Append(Application.productName);
            sb.Append(" v");
            sb.Append(Application.version);
            sb.Append(".md");
            return sb.ToString();
        }
    }
}
