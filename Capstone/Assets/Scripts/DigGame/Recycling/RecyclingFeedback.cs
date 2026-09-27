using UnityEngine;
namespace DigGame
{
    public abstract class RecyclingFeedback : MonoBehaviour
    {
        public abstract void Play(RecyclingRoundResult result);
    }
}
