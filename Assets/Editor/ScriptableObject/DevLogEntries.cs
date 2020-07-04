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


    }
}
