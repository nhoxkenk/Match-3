using UnityEngine;

namespace CandyCrush
{
    [RequireComponent(typeof(AudioSource))]
    public class AudioManager : MonoBehaviour
    {
        [SerializeField] private AudioClip click;
        [SerializeField] private AudioClip deselect;
        [SerializeField] private AudioClip match;
        [SerializeField] private AudioClip noMatch;
        [SerializeField] private AudioClip woosh;
        [SerializeField] private AudioClip pop;

        [SerializeField] private AudioSource audioSource;

        private void OnValidate()
        {
            if(audioSource == null) audioSource = GetComponent<AudioSource>();
        }

        public void PlayClick() => audioSource.PlayOneShot(click);
        public void PlayDeselect() => audioSource.PlayOneShot(deselect);
        public void PlayMatch() => audioSource.PlayOneShot(match);
        public void PlayNoMatch() => audioSource.PlayOneShot(noMatch);
        public void PlayWoosh() => PlayRandomPitch(woosh);
        public void PlayPop() => PlayRandomPitch(pop);

        private void PlayRandomPitch(AudioClip clip)
        {
            audioSource.pitch = Random.Range(0.8f, 1.1f);
            audioSource.PlayOneShot(clip);
            audioSource.pitch = 1;
        }

    }
}
