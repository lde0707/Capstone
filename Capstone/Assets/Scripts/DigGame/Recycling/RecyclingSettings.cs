using UnityEngine;
namespace DigGame
{
    [CreateAssetMenu(menuName = "DigGame/Recycling Settings")]
    public sealed class RecyclingSettings : ScriptableObject
    {
        [SerializeField, Range(.3f, .6f)] float _transitionSeconds = .45f;
        [SerializeField, Range(1f, 5f)] float _answerSeconds = 2f;
        public float TransitionSeconds => Mathf.Clamp(_transitionSeconds, .3f, .6f);
        public float AnswerSeconds => Mathf.Clamp(_answerSeconds, 1f, 5f);
        public int MistakePollution => 1;
    }
}
