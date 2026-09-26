using System;
using UnityEngine;

namespace DigGame
{
    /// <summary>카탈로그 에셋을 인스턴스로 생성한다. 런타임에서 컴포넌트를 보충하지 않는다.</summary>
    public sealed class PrefabGameObjectFactory : IGameObjectFactory
    {
        readonly GamePrefabCatalog _catalog;

        public PrefabGameObjectFactory(GamePrefabCatalog catalog)
        {
            _catalog = catalog != null ? catalog : throw new ArgumentNullException(nameof(catalog));
            _catalog.Validate();
        }

        public T Create<T>(PrefabId id, Transform parent, string name = null) where T : Component
        {
            var prefab = _catalog.Get(id);
            if (prefab.GetComponent<T>() == null)
                throw new InvalidOperationException($"{id} 프리팹에 {typeof(T).Name}이 없습니다.");
            var instance = UnityEngine.Object.Instantiate(prefab, parent, false);
            if (!string.IsNullOrEmpty(name)) instance.name = name;
            return instance.GetComponent<T>();
        }
    }
}
