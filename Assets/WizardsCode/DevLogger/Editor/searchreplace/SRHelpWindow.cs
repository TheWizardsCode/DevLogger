using UnityEditor;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace sr
{
    public class SRHelpWindow : EditorWindow
    {
        public SRHelpSection currentSection;

        Vector2 scrollViewPos;

        private static SRHelpWindow instance;


        Texture2D icon;

        [System.Serializable]
        public class SRHelpSection 
        {
            public string key;
            public string label;
            public string contents;

            public SRHelpSection(string key, string label, string contents)
            {
                this.key = key;
                this.label = label;
                this.contents = contents;
            }

            public SRHelpSection(string key)
            {
                this.key = key;
                this.label = key + ".title";
                this.contents = key + ".contents";
            }
        }

        private Dictionary<string, SRHelpSection> sectionsHash = new Dictionary<string, SRHelpSection>(); 
        private List<SRHelpSection> sections = new List<SRHelpSection>();

        public static void ShowHelpWindow(string key)
        {
            if(instance == null)
            {
                instance = ScriptableObject.CreateInstance(typeof(SRHelpWindow)) as SRHelpWindow;
                instance.AddSection(new SRHelpSection("help.overview"));
                instance.AddSection(new SRHelpSection("help.intro"));
                instance.AddSection(new SRHelpSection("help.location.options"));

                instance.AddSection(new SRHelpSection("help.search.text"));
                instance.AddSection(new SRHelpSection("help.object.search"));
                instance.AddSection(new SRHelpSection("help.usage.vs.instance"));
                instance.AddSection(new SRHelpSection("help.search.results"));
                instance.AddSection(new SRHelpSection("help.replace.actions"));
                instance.AddSection(new SRHelpSection("help.property.search"));
                instance.AddSection(new SRHelpSection("help.instance.search"));
                instance.AddSection(new SRHelpSection("help.numeric.data"));
                instance.AddSection(new SRHelpSection("help.animation.clips"));
                instance.AddSection(new SRHelpSection("help.script.search"));
                instance.AddSection(new SRHelpSection("help.running.scripts"));
                instance.AddSection(new SRHelpSection("help.further.support"));
                instance.ShowUtility();
            }
            
            instance.SetSection(key);
        }




        public void AddSection(SRHelpSection section)
        {
            sections.Add(section);

        }

        void init()
        {
            if(sectionsHash.Count == 0)
            {
                icon = (Texture2D)SRWindow.findObject( "psr_icon" );
                foreach(SRHelpSection section in sections)
                {
                    sectionsHash.Add(section.key, section);
                    if(currentSection != null && section.key == currentSection.key)
                    {
                        currentSection = section;
                    }
                }
                titleContent = SRLoc.GUI("help.window.title");
            }
        }

        public void SetSection(string key)
        {
            init(); 
            currentSection = sectionsHash[key];
            scrollViewPos = Vector2.zero;
            focusHack();
        }

        void focusHack()
        {
            GUIUtility.keyboardControl = 0;
            GUIUtility.hotControl = 0;

        }


        void OnGUI()
        {
            // if the hash is empty, that means we reset.
            init();
            float tocWidth = 200;
            GUILayout.BeginHorizontal();
            GUILayout.Space(10);
            GUILayout.BeginVertical(GUILayout.Width(tocWidth));
            GUILayout.Space(5);
            
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            GUILayout.Box(icon, new GUIStyle());
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            GUILayout.Space(5);
            GUILayout.Label("Project Search & Replace\nVersion "+SRVersion.GetVersion()+"\n© 2020 Enemy Hideout LLC\n");
            
            GUIStyle sectionButtonStyle = new GUIStyle((GUIStyle)"TE toolbarbutton");
            GUIStyle sectionButtonStyleSelected = new GUIStyle((GUIStyle)"TE toolbarbutton");
            float gray = EditorGUIUtility.isProSkin ? 0.8f : 0.2f;
            sectionButtonStyle.normal.textColor = new Color(gray, gray, gray, 1.0f);
            sectionButtonStyleSelected.normal.textColor = new Color(0.2f, 0.5f, 0.2f, 1.0f);
            sectionButtonStyleSelected.fontStyle = FontStyle.Bold;
            foreach(SRHelpSection section in sections)
            {
                GUIStyle buttonStyle = section == currentSection ? sectionButtonStyleSelected : sectionButtonStyle;
                if(GUILayout.Button(SRLoc.L(section.label), buttonStyle, GUILayout.Width(tocWidth))) 
                {
                    SetSection(section.key);
                }
            }
            GUILayout.EndVertical();
            GUILayout.BeginVertical();

            scrollViewPos = EditorGUILayout.BeginScrollView(scrollViewPos);
            GUIStyle wrappingStyle = new GUIStyle(GUI.skin.label);
            wrappingStyle.wordWrap = true;
            wrappingStyle.richText = true;
            GUIContent content = new GUIContent(SRLoc.L(currentSection.contents));
            float labelWidth = EditorGUIUtility.currentViewWidth - tocWidth - 37;

            float size = wrappingStyle.CalcHeight(content, labelWidth);
            float bottomMargin = 100;
            EditorGUILayout.SelectableLabel(content.text, wrappingStyle, GUILayout.Height(size + bottomMargin),  GUILayout.Width(labelWidth));
            EditorGUILayout.EndScrollView();
            GUILayout.EndVertical();
            GUILayout.EndHorizontal();
        }

        void OnInspectorUpdate()
        {
            Repaint();
        }

        void OnDestroy()
        {
            instance = null;
        }
    }
}