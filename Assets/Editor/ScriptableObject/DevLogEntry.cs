using System;
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
