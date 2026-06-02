using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Play from a list of sounds using next, previous, and random
/// </summary>
[RequireComponent(typeof(AudioSource))]
public class PlaySoundsFromList : MonoBehaviour
{
    [Tooltip("Loop the currently playing sound")]
    public bool shouldLoop = false;

    [Tooltip("The list of audio clips to play from")]
    public List<AudioClip> audioClips = new List<AudioClip>();

    private AudioSource audioSource = null;
    private int index = 0;

    public int CurrentIndex => index;

    private void Awake()
    {
        EnsureAudioSource();
    }

    private void EnsureAudioSource()
    {
        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();
    }

    public void NextClip()
    {
        if (audioClips.Count == 0) return;
        index = ++index % audioClips.Count;
        PlayClip();
    }

    public void PreviousClip()
    {
        if (audioClips.Count == 0) return;
        index = --index % audioClips.Count;
        PlayClip();
    }

    public void RandomClip()
    {
        if (audioClips.Count == 0) return;
        index = Random.Range(0, audioClips.Count);
        PlayClip();
    }

    public void PlayAtIndex(int value)
    {
        if (audioClips.Count == 0) return;
        index = Mathf.Clamp(value, 0, audioClips.Count);
        PlayClip();
    }

    public void PauseClip()
    {
        EnsureAudioSource();
        audioSource.Pause();
    }

    public void StopClip()
    {
        EnsureAudioSource();
        audioSource.Stop();
    }

    public void PlayCurrentClip()
    {
        PlayClip();
    }

    private void PlayClip()
    {
        if (audioClips.Count == 0) return;
        EnsureAudioSource();
        audioSource.clip = audioClips[Mathf.Abs(index) % audioClips.Count];
        audioSource.Play();
    }

    private void OnValidate()
    {
        AudioSource audioSource = GetComponent<AudioSource>();
        audioSource.loop = shouldLoop;
    }
}
