using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class AudioClips : MonoBehaviour
{
    [SerializeField] private AudioClip[] audioClips;
    [SerializeField] private Vector2 pitchRange = new(0.9f, 1.1f);
    private AudioSource _audioSource;

    private void Awake()
    {
        _audioSource = GetComponent<AudioSource>();
    }

    public void PlayRandom()
    {
        if (audioClips.Length == 0)
        {
            return;
        }

        var randomIndex = Random.Range(0, audioClips.Length);
        _audioSource.pitch = Random.Range(pitchRange.x, pitchRange.y);
        _audioSource.PlayOneShot(audioClips[randomIndex]);
    }
}