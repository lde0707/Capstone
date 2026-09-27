using System;
using UnityEngine;
namespace DigGame
{
    /// <summary>필요할 때 한 번 생성하고 닫을 때 비활성화하는 세션 소유 프리팹 인스턴스.</summary>
    public sealed class LazyPrefabInstance<T> where T : Component
    {
        readonly T _prefab;
        readonly Transform _parent;
        readonly Action<T> _initialize;
        T _instance;
        public bool IsCreated => _instance != null;
        public LazyPrefabInstance(T prefab, Transform parent, Action<T> initialize)
        {
            _prefab = prefab != null ? prefab : throw new ArgumentNullException(nameof(prefab));
            _parent = parent; _initialize = initialize;
        }
        public T Show()
        {
            if (_instance == null)
            {
                _instance = UnityEngine.Object.Instantiate(_prefab, _parent, false);
                _instance.name = _prefab.name;
                _initialize?.Invoke(_instance);
            }
            _instance.gameObject.SetActive(true);
            return _instance;
        }
        public void Hide() { if (_instance != null) _instance.gameObject.SetActive(false); }
    }
}
