using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace DigGame.Editor
{
    /// <summary>최초 마이그레이션/누락 에셋 복구용. 기존 프리팹과 사용자 편집은 덮어쓰지 않는다.</summary>
    public static class PrefabAssets
    {
        public const string Root = "Assets/Prefabs/DigGame";
        public const string CatalogPath = "Assets/Resources/DigGame/GamePrefabCatalog.asset";

        [MenuItem("DigGame/Prefabs/Create Missing Assets")]
        public static void CreateMissingAssets()
        {
            Directory.CreateDirectory(Root + "/Actors");
            Directory.CreateDirectory(Root + "/World");
            Directory.CreateDirectory(Root + "/Effects");
            Directory.CreateDirectory("Assets/Resources/DigGame");
            AssetDatabase.Refresh();

            foreach (PrefabId id in Enum.GetValues(typeof(PrefabId)))
            {
                if (AssetDatabase.LoadAssetAtPath<GameObject>(PathFor(id)) != null) continue;
                var go = Build(id);
                try { PrefabUtility.SaveAsPrefabAsset(go, PathFor(id)); }
                finally { UnityEngine.Object.DestroyImmediate(go); }
            }

            var catalog = AssetDatabase.LoadAssetAtPath<GamePrefabCatalog>(CatalogPath);
            if (catalog == null)
            {
                catalog = ScriptableObject.CreateInstance<GamePrefabCatalog>();
                AssetDatabase.CreateAsset(catalog, CatalogPath);
            }
            var serialized = new SerializedObject(catalog);
            var entries = serialized.FindProperty("_entries");
            foreach (PrefabId id in Enum.GetValues(typeof(PrefabId)))
            {
                SerializedProperty entry = null;
                for (int i = 0; i < entries.arraySize; i++)
                {
                    var candidate = entries.GetArrayElementAtIndex(i);
                    if (candidate.FindPropertyRelative("_id").intValue == (int)id)
                    { entry = candidate; break; }
                }
                if (entry == null)
                {
                    int index = entries.arraySize++;
                    entry = entries.GetArrayElementAtIndex(index);
                    entry.FindPropertyRelative("_id").intValue = (int)id;
                    // Unity 배열 확장은 이전 요소를 복사하므로 새 참조는 반드시 덮어쓴다.
                    entry.FindPropertyRelative("_prefab").objectReferenceValue =
                        AssetDatabase.LoadAssetAtPath<GameObject>(PathFor(id));
                }
                else if (entry.FindPropertyRelative("_prefab").objectReferenceValue == null)
                    entry.FindPropertyRelative("_prefab").objectReferenceValue =
                        AssetDatabase.LoadAssetAtPath<GameObject>(PathFor(id));
            }
            serialized.ApplyModifiedPropertiesWithoutUndo();
            AssetDatabase.SaveAssets();
            catalog.Validate();
            Debug.Log("[DigGame] 프리팹 18종과 카탈로그 생성/검증 완료 (기존 편집 유지).");
        }

        public static string PathFor(PrefabId id)
        {
            string group = (int)id <= (int)PrefabId.RestoredRelic ? "Actors"
                : id == PrefabId.RecyclingOutput || id == PrefabId.UpgradeEffect || id == PrefabId.Wisp
                    ? "Effects" : "World";
            return $"{Root}/{group}/{id}.prefab";
        }

        static GameObject Build(PrefabId id)
        {
            var go = new GameObject(id.ToString());
            if (id == PrefabId.UpgradeEffect)
            {
                var effect = go.AddComponent<VillageUpgradeFx>();
                var beam = ChildSprite(go.transform, "Beam", Village.OrderFx);
                beam.transform.localPosition = new Vector3(0f, 6f, 0f);
                beam.transform.localScale = new Vector3(3f, 12f, 1f);
                var data = new SerializedObject(effect);
                data.FindProperty("_beam").objectReferenceValue = beam;
                var sparks = data.FindProperty("_sparkRenderers");
                sparks.arraySize = 14;
                for (int i = 0; i < sparks.arraySize; i++)
                    sparks.GetArrayElementAtIndex(i).objectReferenceValue =
                        ChildSprite(go.transform, $"Spark{i:00}", Village.OrderFx + 1);
                data.ApplyModifiedPropertiesWithoutUndo();
                return go;
            }

            var renderer = go.AddComponent<SpriteRenderer>();
            switch (id)
            {
                case PrefabId.Player:
                    go.AddComponent<PlayerController>();
                    renderer.sortingOrder = WorldView.PlayerSortingOrder;
                    go.transform.localScale = Vector3.one * (GameConfig.PlayerScale * 1.18f);
                    break;
                case PrefabId.Miner:
                    go.AddComponent<MinerWorker>();
                    renderer.sortingOrder = WorldView.WorkerSortingOrder;
                    go.transform.localScale = Vector3.one * (GameConfig.PlayerScale * 0.85f);
                    break;
                case PrefabId.Hauler:
                    go.AddComponent<HaulerWorker>();
                    renderer.sortingOrder = WorldView.WorkerSortingOrder;
                    go.transform.localScale = Vector3.one * (GameConfig.PlayerScale * 0.85f);
                    break;
                case PrefabId.Pet:
                    go.AddComponent<PetCompanion>();
                    renderer.sortingOrder = WorldView.PlayerSortingOrder - 1;
                    go.transform.localScale = Vector3.one * (GameConfig.PlayerScale * 0.55f);
                    break;
                case PrefabId.Villager:
                    go.AddComponent<Villager>();
                    renderer.sortingOrder = Village.OrderVillager;
                    go.transform.localScale = Vector3.one * (GameConfig.PlayerScale * 1.12f);
                    break;
                case PrefabId.RestoredRelic: go.AddComponent<RelicUnit>(); break;
                case PrefabId.Treasure:
                    go.AddComponent<Bobber>(); renderer.sortingOrder = 30; break;
                case PrefabId.RecyclingOutput:
                    go.AddComponent<RecyclingOutputFx>(); renderer.sortingOrder = 48; break;
            }
            var actor = go.GetComponent<SpriteActor>();
            if (actor != null)
            {
                var data = new SerializedObject(actor);
                data.FindProperty("_renderer").objectReferenceValue = renderer;
                data.ApplyModifiedPropertiesWithoutUndo();
            }
            return go;
        }

        static SpriteRenderer ChildSprite(Transform parent, string name, int order)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var renderer = go.AddComponent<SpriteRenderer>();
            renderer.sortingOrder = order;
            return renderer;
        }
    }
}
