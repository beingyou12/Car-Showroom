using UnityEngine;
using UnityEngine.UI;

public class AudioMuteToggle : MonoBehaviour
{
    [Header("Audio")]
    public AudioSource audioSource;

    [Header("Button Sprites")]
    public Sprite playingSprite;   // shown when audio IS playing (click to mute)
    public Sprite mutedSprite;     // shown when audio IS muted (click to unmute)

    private Button button;
    private Image buttonImage;
    private bool isMuted = false;

    void Start()
    {
        button = GetComponent<Button>();
        buttonImage = GetComponent<Image>();

        // Listen for button clicks
        button.onClick.AddListener(ToggleMute);

        // Make sure audio is playing at start
        if (audioSource != null && !audioSource.isPlaying)
        {
            audioSource.Play();
        }

        // Set initial button sprite to "playing" state
        UpdateButtonVisual();
    }

    void ToggleMute()
    {
        isMuted = !isMuted;

        if (isMuted)
        {
            // Mute the audio
            audioSource.Pause();
        }
        else
        {
            // Unmute / resume audio
            audioSource.Play();
        }

        UpdateButtonVisual();
    }

    void UpdateButtonVisual()
    {
        if (buttonImage == null) return;

        // Swap the button sprite based on current state
        if (isMuted)
        {
            buttonImage.sprite = mutedSprite;    // show "muted" icon (click to play)
        }
        else
        {
            buttonImage.sprite = playingSprite;  // show "playing" icon (click to mute)
        }
    }
}