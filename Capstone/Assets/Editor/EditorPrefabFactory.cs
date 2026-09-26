using System;
using UnityEditor;
using UnityEngine;

namespace DigGame.Editor
{
    /// <summary>미리보기에서도 원본 프리팹 연결을 보존한다. 게임 코드는 같은 인터페이스를 사용한다.</summary>
    public sealed class EditorPrefabFactory : IGameObjectFactory
    {
        readonly GamePrefabCatalog _catalog;
        public EditorPrefabFactory(GamePrefabCatalog catalog)
        {
            _catalog = catalog != null ? catalog : throw new ArgumentNullException(nameof(catalog));
            _catalog.Validate();
        }

        public T Create<T>(PrefabId id, Transform parent, string name = null) where T : Component
        {
            var asset = _catalog.Get(id);
            if (asset.GetComponent<T>() == null)
                throw new InvalidOperationException($"{id}: {typeof(T).Name} 컴포넌트가 없습니다.");
            var go = (GameObject)PrefabUtility.InstantiatePrefab(asset, parent);
            if (!string.IsNullOrEmpty(name)) go.name = name;
            return go.GetComponent<T>();
        }
    }
}
