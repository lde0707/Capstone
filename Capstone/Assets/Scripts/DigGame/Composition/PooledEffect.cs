using System;
using UnityEngine;
namespace DigGame
{
    /// <summary>효과 종료를 풀에 위임한다. 일반 팩토리로 생성된 경우에는 기존처럼 파괴한다.</summary>
    public abstract class PooledEffect : MonoBehaviour
    {
        Action<PooledEffect> _return;
        bool _released;
        bool _hasDefaults;
        Vector3 _position, _scale;
        Quaternion _rotation;
        internal void Lease(Action<PooledEffect> returnToPool)
        {
            if (!_hasDefaults)
            {
                _position = transform.localPosition; _scale = transform.localScale;
                _rotation = transform.localRotation; _hasDefaults = true;
            }
            transform.localPosition = _position; transform.localRotation = _rotation; transform.localScale = _scale;
            _return = returnToPool; _released = false;
        }
        public void Release()
        {
            if (_released) return;
            _released = true;
            if (_return != null) _return(this);
            else if (Application.isPlaying) Destroy(gameObject);
            else DestroyImmediate(gameObject);
        }
    }
}
