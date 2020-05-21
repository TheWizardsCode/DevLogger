using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WizardsCode.DevLogger;

namespace WizardsCode.DevLogger
{
    public class DevLogEntries : ScriptableObject
    {
        [SerializeField, Tooltip("The entries in this Dev Log.")]
        public List<Entry> entries = new List<Entry>();


    }

    [Serializable]
    public class Entry
    {
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
        [SerializeField, Tooltip("Has this entry been tweeted?")]
        public bool tweeted = false;
        [SerializeField, Tooltip("The date and time the log entry was created.")]
        public DateTime created = DateTime.Now;
    }
}
