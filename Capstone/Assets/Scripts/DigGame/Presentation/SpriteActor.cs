using System;
using UnityEngine;

namespace DigGame
{
    /// <summary>프리팹에 구성된 렌더러를 재사용하는 시각 오브젝트의 공통 기반.</summary>
    [RequireComponent(typeof(SpriteRenderer))]
    public abstract class SpriteActor : MonoBehaviour
    {
        [SerializeField] SpriteRenderer _renderer;

        protected SpriteRenderer ActorRenderer
        {
            get
            {
                if (_renderer == null) _renderer = GetComponent<SpriteRenderer>();
                if (_renderer == null)
                    throw new InvalidOperationException($"{name}: SpriteRenderer가 필요합니다.");
                return _renderer;
            }
        }

        protected virtual void Reset() => _renderer = GetComponent<SpriteRenderer>();
    }
}
