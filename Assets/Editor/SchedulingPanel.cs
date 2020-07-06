﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using WizardsCode.Social;

namespace WizardsCode.DevLogger
{
    /// <summary>
    /// The scheduling panel presents and controls the sceduled events.
    /// This allows the user to set a number of scheudled events that
    /// will then be used to ensure they stay on top of their Dev Log
    /// responsibilities.
    /// </summary>
    public class SchedulingPanel
    {
        string[] m_ScheduledEventGUIDs;

        ScheduledEvent[] m_EventsCache;

        float m_CacheTTL = 2;
        float m_CacheUpdateTime = 0;

        internal void OnEnable()
        {
            UpdateEventCache();
        }

        private void UpdateEventCache()
        {
            m_ScheduledEventGUIDs = AssetDatabase.FindAssets("t:ScheduledEvent");
            m_EventsCache = new ScheduledEvent[m_ScheduledEventGUIDs.Length];
            for (int i = 0; i < m_ScheduledEventGUIDs.Length; i++)
            {
                m_EventsCache[i] = AssetDatabase.LoadAssetAtPath<ScheduledEvent>(AssetDatabase.GUIDToAssetPath(m_ScheduledEventGUIDs[i]));
            }
            m_CacheUpdateTime = Time.realtimeSinceStartup + m_CacheTTL;
        }

        internal void OnDisable()
        {

        }

        internal void OnGUI()
        {
            if (Time.realtimeSinceStartup > m_CacheUpdateTime)
            {
                UpdateEventCache();
            }

            if (m_EventsCache != null)
            {
                for (int i = 0; i < m_ScheduledEventGUIDs.Length; i++)
                {

                    ScheduledEvent evt = m_EventsCache[i];

                    EditorGUILayout.BeginVertical("Box");
                    EditorGUILayout.LabelField(evt.name);

                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.PrefixLabel("Day Of Week");
                    evt.m_DayOfWeek = (DayOfWeek)EditorGUILayout.EnumPopup(evt.m_DayOfWeek);
                    EditorGUILayout.EndHorizontal();

                    EditorGUILayout.BeginHorizontal();
                    DateTime dt = DateTime.UtcNow;
                    dt = dt.Date + TimeSpan.FromSeconds(evt.m_TimeOfDay);
                    int hour = dt.Hour;
                    int minute = dt.Minute;
                    
                    EditorGUILayout.PrefixLabel("Time (24hr)");
                    hour = EditorGUILayout.IntField(hour);
                    minute = EditorGUILayout.IntField(minute);
                    if ((hour >= 0 && hour <=23) && (minute >= 0 && minute <= 59))
                    {
                        evt.m_TimeOfDay = (int)((hour * 60 * 60) + (minute * 60));
                    }
                    EditorGUILayout.EndHorizontal();

                    EditorGUILayout.BeginHorizontal();
                    evt.m_PostToTwitter = EditorGUILayout.Toggle("Twitter?", evt.m_PostToTwitter);
                    EditorGUILayout.EndHorizontal();

                    if (evt.m_PostToTwitter)
                    {
                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.PrefixLabel("Twitter Hashtag");
                        evt.m_TwitterHashtag = EditorGUILayout.TextField(evt.m_TwitterHashtag);
                        EditorGUILayout.EndHorizontal();
                    }

                    EditorGUILayout.BeginHorizontal();
                    evt.m_PostToDiscord = EditorGUILayout.Toggle("Discord?", evt.m_PostToDiscord);
                    EditorGUILayout.EndHorizontal();


                    bool posted = false;
                    if (evt.IsDue)
                    {
                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.PrefixLabel("Due Now");
                        if (GUILayout.Button("Post now"))
                        {
                            if (evt.m_PostToTwitter)
                            {
                                string response;
                                string[] tags = evt.m_TwitterHashtag.Split('#');
                                foreach (string tag in tags)
                                {
                                    if (!evt.m_DevLogEntry.metaData.Contains(tag.Trim()))
                                    {
                                        evt.m_DevLogEntry.metaData.Add(tag.Trim());
                                    }
                                }

                                if (Twitter.PublishTweet(evt.m_DevLogEntry, out response))
                                {
                                    posted = true;
                                } else
                                {
                                    posted = false;
                                    // TODO handle Twitter failure gracefully
                                    Debug.LogError("Failed to post to twitter: " + response);
                                }
                            }

                            if (evt.m_PostToDiscord)
                            {
                                Discord.PostEntry(evt.m_DevLogEntry);
                            }
                        }

                        if (posted)
                        {
                            evt.MarkDone();
                        }

                        EditorGUILayout.EndHorizontal();
                    } else
                    {
                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.PrefixLabel("Next due");
                        DateTime dueDate = evt.GetNextDueDateTimeLocal();
                        string dueLabel;
                        if (dueDate.Day == DateTime.Now.Day && dueDate.Month == DateTime.Now.Month)
                        {
                            dueLabel = "TODAY at  " + dueDate.ToString("HH:mm");
                        } else
                        {
                            dueLabel = dueDate.ToString("dddd MMMM dd, yyyy HH:mm");
                        }
                        EditorGUILayout.LabelField(dueLabel);
                        EditorGUILayout.EndHorizontal();
                    }

                    evt.m_DevLogEntry = (DevLogEntry)EditorGUILayout.ObjectField(evt.m_DevLogEntry, typeof(DevLogEntry), false);
                    EditorGUILayout.PrefixLabel("Dev Log Entry");
                    SerializedObject obj = new SerializedObject(evt);
                    EditorGUILayout.PropertyField(obj.FindProperty("m_DevLogEntry"));

                    EditorGUILayout.EndVertical();
                }
            }
        }

        internal void Update()
        {
            
        }
    }
}