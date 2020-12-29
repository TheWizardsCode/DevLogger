using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;

namespace WizardsCode.DevLogger
{
    public class EntryPanelSettings
    {
        public static int MetaDataItemCount
        {
            get { return EditorPrefs.GetInt("numberOfSuggestedMetaData", 0); }
            set { EditorPrefs.SetInt("numberOfSuggestedMetaData", value); }
        }

        public static string GetSuggestedMetaDataItem(int index)
        {
            return EditorPrefs.GetString("suggestedMetaData_" + index);
        }

        public static void SetSuggestedMetaDataItem(int index, string data)
        {
            EditorPrefs.SetString("suggestedMetaData_" + index, data);
        }

        public static bool GetMetaDataSelectionStatus(int index)
        {
            return EditorPrefs.GetBool("selectedMetaData_" + index);
        }
        public static void SetMetaDataSelectionStatus(int index, bool isSelected)
        {
            EditorPrefs.SetBool("selectedMetaData_" + index, isSelected);
        }
    }
}
