using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using System;
using System.Text;

namespace WizardsCode.DevLogger
{
    public class EntryPanelSettings
    {
        public static int MetaDataItemCount
        {
            get { return GetSuggestedMetaDataItems().Count; }
        }

        public static MetaDataItems GetSuggestedMetaDataItems()
        {
            //TODO cache meta data item list
            string json = EditorPrefs.GetString("suggestedMetaData_" + Application.productName, "{}");

            MetaDataItems results = JsonUtility.FromJson<MetaDataItems>(json);
            if (results == null || results.Count == 0)
            {
                results = new MetaDataItems();
                results.Add(new MetaDataItem("#MadeWithUnity", false));
                SetSuggestedMetaDataItems(results);
            }
            return results;
        }

        public static void SetSuggestedMetaDataItems(MetaDataItems data)
        {
            EditorPrefs.SetString("suggestedMetaData_" + Application.productName, JsonUtility.ToJson(data)) ;
        }

        public static void AddSuggestedMetaDataItem(MetaDataItem item)
        {
            MetaDataItems items = GetSuggestedMetaDataItems();
            items.Add(item);
            SetSuggestedMetaDataItems(items);
        }

        internal static void ResetMetaData()
        {
            EditorPrefs.SetString("suggestedMetaData_" + Application.productName, "");
        }
    }

    [Serializable]
    public class MetaDataItem {
        public string name;
        public bool isSelected;

        public MetaDataItem(string name, bool isSelected)
        {
            this.name = name;
            this.isSelected = isSelected;
        }
    }

    [Serializable]
    public class MetaDataItems
    {
        public List<MetaDataItem> items = new List<MetaDataItem>();

        public int Count { get { return items == null ? 0 : items.Count; } }

        public void Add (MetaDataItem item)
        {
            items.Add(item);
        }

        public MetaDataItem GetItem(int index)
        {
            return items[index];
        }

        /**
        public static MetaDataItems FromJson(string json)
        {
            //TODO this manual parsing of JSON is crazy, surely there is a better way in Unity by now? See JsonUtility
            MetaDataItems result = new MetaDataItems();
            string parseableJson = json.Substring(json.IndexOf('[') + 1, json.LastIndexOf(']') - json.IndexOf('[') - 1);
            string[] items = parseableJson.Split(',');
            for (int i = 0; i < items.Length; i++)
            {
                result.Add()
            }

            return result;
        }

        public string ToJson()
        {
            //TODO this manual creation of JSON is crazy, surely there is a better way in Unity by now? See JsonUtility
            StringBuilder json = new StringBuilder();
            json.AppendLine("{");
            json.AppendLine("\t\"items\" : [");
            for (int i = 0; i < items.Length; i++)
            {
                json.Append(JsonUtility.ToJson(items[i]));
                if (i < items.Length - 1)
                {
                    json.AppendLine(",");
                }
            }
            json.AppendLine("]");
            json.AppendLine("}");
            return json.ToString();
        }
        */
    }
}
