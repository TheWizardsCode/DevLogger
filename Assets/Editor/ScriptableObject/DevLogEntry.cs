﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WizardsCode.DevLogger
{
    /// <summary>
    /// A single Log entry for a DevLog.
    /// </summary>
    public class DevLogEntry : ScriptableObject
    {
        [SerializeField, Tooltip("The date and time the log entry was created.")]
        public DateTime created = DateTime.Now;
        [SerializeField, Tooltip("The short description of this entry used as the summary for an entry in both the log an dsocial media content.")]
        public string shortDescription = "";
        [SerializeField, Tooltip("The long description of this entry used as the detail for a Dev Log entry.")]
        public string longDescription = "";
        [SerializeField, Tooltip("The meta data such as links and hashtags associated with this entry.")]
        public List<string> metaData = new List<string>();
        [SerializeField, Tooltip("A list of the screen captures for this entry")]
        public List<DevLogScreenCapture> captures = new List<DevLogScreenCapture>();
        [SerializeField, Tooltip("The commit hash related to this change in the project.")]
        public string commitHash = "";
        [SerializeField, Tooltip("Whether this entry has every been tweeted.")]
        public bool tweeted;
        [SerializeField, Tooltip("The date and time (UTC file time) this entry was last tweeted.")]
        public long lastTweetFileTime;

        public string lastTweetPrettyTime { 
            get
            {
                return DateTime.FromFileTimeUtc(lastTweetFileTime).ToLocalTime().ToString("dddd dd-MMM-yyyy HH:MM");
            }
        }
    }
}