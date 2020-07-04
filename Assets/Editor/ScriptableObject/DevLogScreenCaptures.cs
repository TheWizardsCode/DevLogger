using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WizardsCode.DevLogger
{
    public class DevLogScreenCaptures : ScriptableObject
    {
        [SerializeField]
        public List<DevLogScreenCapture> captures = new List<DevLogScreenCapture>();

        public int Count { get { return captures.Count; } }
    }
}
