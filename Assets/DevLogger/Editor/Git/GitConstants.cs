using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace WizardsCode.Git
{
    [InitializeOnLoad]
    public static class GitConstants
    {
        private const string EDITOR_PREFS_SCOPE = "WizardsCode.Git";

        public static string GITPATH => $"{EDITOR_PREFS_SCOPE}_GitPath";
    }
}