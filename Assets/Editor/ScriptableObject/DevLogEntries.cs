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
        List<DevLogEntry> m_Entries = new List<DevLogEntry>();

        internal void AddEntry(DevLogEntry entry)
        {
            m_Entries.Add(entry);
        }
        internal DevLogEntry GetEntry(int index)
        {
            return m_Entries[index];
        }

        internal List<DevLogEntry> GetEntries()
        {
            return m_Entries;
        }

        /// <summary>
        /// Get list of all Entries that are marked for social sharing
        /// but have not yet been shared socially.
        /// </summary>
        /// <returns></returns>
        internal List<DevLogEntry> GetAvailableSocialEntries()
        {
            List<DevLogEntry>  results = new List<DevLogEntry>();
            for (int i = 0; i < m_Entries.Count; i++)
            {
                if (m_Entries[i].isSocial && !m_Entries[i].tweeted && !m_Entries[i].discordPost) {
                    results.Add(m_Entries[i]);
                }
            }
            return results;
        }
    }
}
