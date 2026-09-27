using System;
using System.Collections.Generic;
using UnityEngine;
namespace DigGame
{
    /// <summary>일반 생성 계약을 유지하며 지정된 단기 효과만 재사용한다. 풀은 게임 세션에 귀속된다.</summary>
    public sealed class PooledGameObjectFactory : IGameObjectFactory
    {
        sealed class Bucket
        {
            public readonly Stack<PooledEffect> Available = new Stack<PooledEffect>();
            public readonly int Limit;
            public readonly Action<PooledEffect> Return;
            readonly Transform _storage;
            public Bucket(int limit, Transform storage) { Limit = limit; _storage = storage; Return = Recycle; }
            void Recycle(PooledEffect effect)
            {
                effect.gameObject.SetActive(false);
                if (_storage == null || Available.Count >= Limit)
                {
                    if (Application.isPlaying) UnityEngine.Object.Destroy(effect.gameObject);
                    else UnityEngine.Object.DestroyImmediate(effect.gameObject);
                    return;
                }
                effect.transform.SetParent(_storage, false);
                Available.Push(effect);
            }
        }
        readonly IGameObjectFactory _inner;
        readonly Transform _storage;
        readonly Dictionary<PrefabId, Bucket> _buckets = new Dictionary<PrefabId, Bucket>();
        public int CreatedCount { get; private set; }
        public int ReusedCount { get; private set; }
        public PooledGameObjectFactory(IGameObjectFactory inner, Transform owner)
        {
            _inner = inner ?? throw new ArgumentNullException(nameof(inner));
            if (owner == null) throw new ArgumentNullException(nameof(owner));
            var storage = new GameObject("RuntimeEffectPool");
            storage.transform.SetParent(owner, false); storage.SetActive(false); _storage = storage.transform;
            _buckets.Add(PrefabId.RecyclingOutput, new Bucket(16, _storage));
            _buckets.Add(PrefabId.UpgradeEffect, new Bucket(8, _storage));
        }
        public T Create<T>(PrefabId id, Transform parent, string name = null) where T : Component
        {
            if (!_buckets.TryGetValue(id, out var bucket)) return _inner.Create<T>(id, parent, name);
            PooledEffect effect = null;
            while (bucket.Available.Count > 0)
            {
                var candidate = bucket.Available.Peek();
                if (candidate == null) { bucket.Available.Pop(); continue; }
                if (candidate.GetComponent<T>() == null)
                    throw new InvalidOperationException($"{id} 프리팹에 {typeof(T).Name}이 없습니다.");
                effect = bucket.Available.Pop(); ReusedCount++; break;
            }
            if (effect == null)
            {
                var component = _inner.Create<T>(id, _storage, name);
                effect = component.GetComponent<PooledEffect>();
                if (effect == null) throw new InvalidOperationException($"{id}에 PooledEffect가 없습니다.");
                CreatedCount++;
            }
            effect.Lease(bucket.Return);
            effect.transform.SetParent(parent, false);
            effect.name = name ?? id.ToString();
            effect.gameObject.SetActive(true);
            return effect.GetComponent<T>();
        }
        public int AvailableCount(PrefabId id) => _buckets.TryGetValue(id, out var bucket) ? bucket.Available.Count : 0;
    }
}
