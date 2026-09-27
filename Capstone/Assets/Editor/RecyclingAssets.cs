using System.IO;
using UnityEditor;
using UnityEngine;
namespace DigGame.Editor
{
    public static class RecyclingAssets
    {
        public const string PanelPath = "Assets/Prefabs/DigGame/UI/RecyclingPanel.prefab";
        const string SettingsPath = "Assets/Resources/DigGame/RecyclingSettings.asset";
        public static void EnsureSettings()
        {
            Directory.CreateDirectory("Assets/Resources/DigGame");
            AssetDatabase.Refresh();
            if (AssetDatabase.LoadAssetAtPath<RecyclingSettings>(SettingsPath) == null)
                AssetDatabase.CreateAsset(ScriptableObject.CreateInstance<RecyclingSettings>(), SettingsPath);
        }

        public static void EnsurePanel()
        {
            if (AssetDatabase.LoadAssetAtPath<GameObject>(PanelPath) != null) return;
            Directory.CreateDirectory("Assets/Prefabs/DigGame/UI"); AssetDatabase.Refresh();
            var panel = new GameObject("RecyclingPanel"); panel.AddComponent<RecyclingMinigameUI>();
            try { PrefabUtility.SaveAsPrefabAsset(panel, PanelPath); }
            finally { Object.DestroyImmediate(panel); }
        }
        public static void MigratePanel()
        {
            EnsurePanel();
            string path = PrefabAssets.PathFor(PrefabId.RecyclingStation);
            var root = PrefabUtility.LoadPrefabContents(path);
            try
            {
                var controller = new SerializedObject(root.GetComponent<RecyclingMinigameController>());
                if (controller.FindProperty("_viewPrefab").objectReferenceValue != null) return;
                var embedded = root.GetComponentInChildren<RecyclingMinigameUI>(true);
                if (embedded != null)
                {
                    PrefabUtility.SaveAsPrefabAsset(embedded.gameObject, PanelPath);
                    Object.DestroyImmediate(embedded.gameObject);
                }
                controller.FindProperty("_viewPrefab").objectReferenceValue =
                    AssetDatabase.LoadAssetAtPath<GameObject>(PanelPath).GetComponent<RecyclingMinigameUI>();
                controller.ApplyModifiedPropertiesWithoutUndo();
                PrefabUtility.SaveAsPrefabAsset(root, path);
            }
            finally { PrefabUtility.UnloadPrefabContents(root); }
        }
        public static GameObject BuildPrefab()
        {
            var root = new GameObject("RecyclingStation");
            var controller = root.AddComponent<RecyclingMinigameController>();
            var audio = root.AddComponent<AudioSource>(); audio.playOnAwake = false; audio.spatialBlend = 0;
            var feedback = root.AddComponent<RecyclingAudioFeedback>();
            EnsurePanel();
            var view = AssetDatabase.LoadAssetAtPath<GameObject>(PanelPath).GetComponent<RecyclingMinigameUI>();
            var data = new SerializedObject(controller);
            data.FindProperty("_settings").objectReferenceValue = AssetDatabase.LoadAssetAtPath<RecyclingSettings>(SettingsPath);
            data.FindProperty("_viewPrefab").objectReferenceValue = view;
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
