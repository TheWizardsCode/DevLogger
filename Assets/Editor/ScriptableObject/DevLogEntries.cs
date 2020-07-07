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
        public List<DevLogEntry> entries = new List<DevLogEntry>();

        /// <summary>
        /// Get list of all Entries that are marked for social sharing
        /// but have not yet been shared socially.
        /// </summary>
        /// <returns></returns>
        internal List<DevLogEntry> GetAvailableSocialEntries()
        {
            List<DevLogEntry>  results = new List<DevLogEntry>();
            for (int i = 0; i < entries.Count; i++)
            {
                if (entries[i].isSocial && !entries[i].tweeted && !entries[i].discordPost) {
                    results.Add(entries[i]);
                }
            }
            return results;
        }
    }
}
