using UnityEngine;

namespace DigGame
{
    /// <summary>게임 규칙은 에셋의 위치나 로딩 방식 대신 이 생성 계약에 의존한다.</summary>
    public interface IGameObjectFactory
    {
        T Create<T>(PrefabId id, Transform parent, string name = null) where T : Component;
    }
}
