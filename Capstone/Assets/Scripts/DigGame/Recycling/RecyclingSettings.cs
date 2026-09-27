using UnityEngine;
namespace DigGame
{
    [CreateAssetMenu(menuName = "DigGame/Recycling Settings")]
    public sealed class RecyclingSettings : ScriptableObject
    {
        [SerializeField, Range(.3f, .6f)] float _transitionSeconds = .45f;
        [SerializeField, Range(0, 100)] int _mistakePollution = 5;
        public float TransitionSeconds => Mathf.Clamp(_transitionSeconds, .3f, .6f);
        public int MistakePollution => Mathf.Clamp(_mistakePollution, 0, 100);
    }
}
