using System.IO;
using UnityEditor;
using UnityEngine;
namespace DigGame.Editor
{
    public static class RecyclingAssets
    {
        const string SettingsPath = "Assets/Resources/DigGame/RecyclingSettings.asset";
        public static void EnsureSettings()
        {
            Directory.CreateDirectory("Assets/Resources/DigGame");
            AssetDatabase.Refresh();
            if (AssetDatabase.LoadAssetAtPath<RecyclingSettings>(SettingsPath) == null)
                AssetDatabase.CreateAsset(ScriptableObject.CreateInstance<RecyclingSettings>(), SettingsPath);
        }
        public static GameObject BuildPrefab()
        {
            var root = new GameObject("RecyclingStation");
            var controller = root.AddComponent<RecyclingMinigameController>();
            var audio = root.AddComponent<AudioSource>(); audio.playOnAwake = false; audio.spatialBlend = 0;
            var feedback = root.AddComponent<RecyclingAudioFeedback>();
            var panel = new GameObject("MinigamePanel"); panel.transform.SetParent(root.transform, false);
            var view = panel.AddComponent<RecyclingMinigameUI>();
            var data = new SerializedObject(controller);
            data.FindProperty("_settings").objectReferenceValue = AssetDatabase.LoadAssetAtPath<RecyclingSettings>(SettingsPath);
            data.FindProperty("_view").objectReferenceValue = view;
            data.FindProperty("_feedback").objectReferenceValue = feedback;
            data.ApplyModifiedPropertiesWithoutUndo();
            data = new SerializedObject(feedback);
            data.FindProperty("_success").objectReferenceValue = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/Recycling/Success.wav");
            data.FindProperty("_failure").objectReferenceValue = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/Recycling/Failure.wav");
            data.ApplyModifiedPropertiesWithoutUndo();
            return root;
        }
    }
}
