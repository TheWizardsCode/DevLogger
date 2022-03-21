using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using WizardsCode.DevLogger;

[CustomEditor(typeof(DesignDocument))]
public class DesignDocEditor : Editor
{
    SerializedProperty m_ElevatorPitch;
    SerializedProperty m_GameIdentity;
    SerializedProperty m_GameLoop;
    SerializedProperty m_DesignPillars;
    SerializedProperty m_Features;
    SerializedProperty m_MagicMoments;
    SerializedProperty m_ArtStyle;
    SerializedProperty m_InterfaceAndControls;
    SerializedProperty m_MusicSound;
    SerializedProperty m_TargetPlatforms;

    void OnEnable()
    {
        m_ElevatorPitch = serializedObject.FindProperty("m_ElevatorPitch");
        m_GameIdentity = serializedObject.FindProperty("m_GameIdentity");
        m_GameLoop = serializedObject.FindProperty("m_GameLoop");
        m_DesignPillars = serializedObject.FindProperty("m_DesignPillars");
        m_MagicMoments = serializedObject.FindProperty("m_MagicMoments");
        m_Features = serializedObject.FindProperty("m_Features");
        m_ArtStyle = serializedObject.FindProperty("m_ArtStyle");
        m_InterfaceAndControls = serializedObject.FindProperty("m_InterfaceAndControls");
        m_MusicSound = serializedObject.FindProperty("m_MusicSound");
        m_TargetPlatforms = serializedObject.FindProperty("m_TargetPlatforms");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.HelpBox("Describe what makes your game great in just a few sentences. (Example: FIFA18: Authentic football action using the stars and teams that you know from around the world today.Unparalleled strategic realism lets you control your team with incredible precision.)", MessageType.Info);
        EditorGUILayout.PropertyField(m_ElevatorPitch);

        EditorGUILayout.HelpBox("A single sentence description of the game that you will use to guide design decisions. (Example: Stylized action platformer about a meatball fighting the dinner table.)", MessageType.Info);
        EditorGUILayout.PropertyField(m_GameIdentity);

        EditorGUILayout.HelpBox("Describe the central process that is repeated through the game.This is the crux of the user experience.Identify the means of retention through rewards in the loop. (Example: Clash of Clans: Collect Resources, Build and Train Troops, Defeat Enemies)", MessageType.Info);
        EditorGUILayout.PropertyField(m_GameLoop);

        StringArrayGUI("Design Pillars", "List up to 3 words/phrases that convey the feeling or emotion you want the player to experience. They are what the player will think of first when thinking of your game. (Example: Fast.Action - packed.Mayhem.)", m_DesignPillars);

        StringArrayGUI("Features", "List the cool features or unique elements that you want to include in your game.This is a complete list of all key features and what matters most for each feature. It helps form a holistic view of what the game needs to succeed and thus cannot be compromised and helps guide decision making during development. (Example: Grand Theft Auto: Intuitive and accessible map feature providing contextual information for the players immediate surroundings while driving or whole world during Garage time.Easily \"failure free\" driving system that provides fun way to move between missions but does not hamper the player with unnecessary challenges.", m_Features);

        StringArrayGUI("Magic Moments", "3 to 5 specific points in gameplay that delight the player.They are shareable moments in the form of a story, they trigger emotion. Describe each moment in a single, very specific sentence. (Example: Mario Kart: Cross the finish line and the Kart zooms off into the distance. Hitting a player with a collectable and they are visibly delayed.Landing a perfect drift to get a major speed boost)", m_MagicMoments);

        StringArrayGUI("Art Style", "Include references to images, models and games that have a similar aesthetic to what you're trying to achieve.", m_ArtStyle);

        StringArrayGUI("Music and Sound", "Include links to music and sound design similar to what you're trying to achieve. You can also list the emotional responses that the sound should invoke in the player.", m_MusicSound);

        StringArrayGUI("Interface and Controls", "List the player input method, the controls, and how the player interacts with your game.", m_InterfaceAndControls);

        StringArrayGUI("Target Platforms", "Which platforms is the game to be delivered on?", m_TargetPlatforms);

        serializedObject.ApplyModifiedProperties();
    }

    private void StringArrayGUI(string label, string help, SerializedProperty array)
    {
        EditorGUILayout.LabelField(label);
        EditorGUILayout.HelpBox(help, MessageType.Info);
        for (int i = 0; i < array.arraySize; i++)
        {
            EditorGUILayout.PropertyField(array.GetArrayElementAtIndex(i), new GUIContent());
        }
    }
}
