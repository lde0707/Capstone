using System;
using System.Collections.Generic;
using UnityEngine;

namespace DigGame
{
    [CreateAssetMenu(menuName = "DigGame/Prefab Catalog")]
    public sealed class GamePrefabCatalog : ScriptableObject
    {
        public const string ResourcePath = "DigGame/GamePrefabCatalog";

        [Serializable]
        public sealed class Entry
        {
            [SerializeField] PrefabId _id;
            [SerializeField] GameObject _prefab;
            public PrefabId Id => _id;
            public GameObject Prefab => _prefab;
        }

        [SerializeField] Entry[] _entries = Array.Empty<Entry>();

        public GameObject Get(PrefabId id)
        {
            foreach (var entry in _entries)
                if (entry != null && entry.Id == id && entry.Prefab != null) return entry.Prefab;
            throw new InvalidOperationException($"프리팹 카탈로그에 {id}가 없습니다.");
        }

        public void Validate()
        {
            var ids = new HashSet<PrefabId>();
            foreach (var entry in _entries)
            {
                if (entry == null || entry.Prefab == null || !ids.Add(entry.Id))
                    throw new InvalidOperationException("프리팹 카탈로그에 누락 또는 중복 항목이 있습니다.");
            }
            foreach (PrefabId id in Enum.GetValues(typeof(PrefabId))) Get(id);
        }
    }
}
