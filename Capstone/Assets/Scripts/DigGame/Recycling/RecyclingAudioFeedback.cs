using UnityEngine;
namespace DigGame
{
    [RequireComponent(typeof(AudioSource))]
    public sealed class RecyclingAudioFeedback : RecyclingFeedback
    {
        [SerializeField] AudioClip _success;
        [SerializeField] AudioClip _failure;
        [SerializeField, Range(0,1)] float _volume = .35f;
        public override void Play(RecyclingRoundResult result)
        {
            var clip = result.Correct ? _success : _failure;
            if (clip != null && Application.isPlaying) GetComponent<AudioSource>().PlayOneShot(clip, _volume);
        }
    }
}
