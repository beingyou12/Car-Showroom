using UnityEngine;
using TMPro;
using System.Text;

public class MusicPlayerController : MonoBehaviour
{
    [Header("References")]
    public PlaySoundsFromList playlist;
    public GameObject tvCanvas;
    public TextMeshProUGUI nowPlayingText;
    public TextMeshProUGUI playlistText;

    [Header("Automation")]
    public bool autoScanFolder = true;
    public string musicFolderPath = "Assets/UAS/music";

    private bool isPowerOn = false;

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (autoScanFolder && !Application.isPlaying)
        {
            // Delay call to avoid "SendMessage cannot be called during Awake, CheckConsistency, or OnValidate" if any events trigger
            UnityEditor.EditorApplication.delayCall += RefreshPlaylist;
        }
    }

    [ContextMenu("Refresh Playlist Now")]
    public void RefreshPlaylist()
    {
        if (this == null || playlist == null || string.IsNullOrEmpty(musicFolderPath)) return;
        
        string[] guids = UnityEditor.AssetDatabase.FindAssets("t:AudioClip", new[] { musicFolderPath });
        
        // Only update if the list has changed to avoid constant dirtying
        bool changed = false;
        if (playlist.audioClips.Count != guids.Length)
        {
            changed = true;
        }
        else
        {
            // Simple check
            for (int i = 0; i < guids.Length; i++)
            {
                string path = UnityEditor.AssetDatabase.GUIDToAssetPath(guids[i]);
                if (playlist.audioClips[i] == null || playlist.audioClips[i].name != System.IO.Path.GetFileNameWithoutExtension(path))
                {
                    changed = true;
                    break;
                }
            }
        }

        if (changed)
        {
            playlist.audioClips.Clear();
            foreach (string guid in guids)
            {
                string path = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
                AudioClip clip = UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>(path);
                if (clip != null)
                {
                    playlist.audioClips.Add(clip);
                }
            }
            UnityEditor.EditorUtility.SetDirty(playlist);
            UpdateUI();
        }
    }
#endif

    private void Start()
    {
        if (tvCanvas != null)
            tvCanvas.SetActive(isPowerOn);
        
        UpdateUI();
    }

    public void TogglePower()
    {
        isPowerOn = !isPowerOn;
        Debug.Log("[MusicPlayer] TogglePower: " + isPowerOn);
        if (tvCanvas != null)
            tvCanvas.SetActive(isPowerOn);

        if (isPowerOn)
        {
            if (playlist != null)
            {
                Debug.Log("[MusicPlayer] Playing current clip");
                playlist.PlayCurrentClip();
            }
            UpdateUI();
        }
        else
        {
            if (playlist != null)
                playlist.StopClip();
        }
    }

    public void NextTrack()
    {
        Debug.Log("[MusicPlayer] NextTrack called. Power: " + isPowerOn);
        if (!isPowerOn)
        {
            TogglePower();
            return;
        }

        if (playlist != null)
        {
            Debug.Log("[MusicPlayer] Cycling to next clip");
            playlist.NextClip();
            UpdateUI();
        }
    }

    public void NextTrackXRI(UnityEngine.XR.Interaction.Toolkit.ActivateEventArgs args)
    {
        NextTrack();
    }

    public void PreviousTrack()
    {
        if (!isPowerOn) return;

        if (playlist != null)
        {
            playlist.PreviousClip();
            UpdateUI();
        }
    }

    private void UpdateUI()
    {
        if (playlist == null || playlist.audioClips.Count == 0)
        {
            if (nowPlayingText != null) nowPlayingText.text = "No tracks available";
            return;
        }

        int currentIndex = playlist.CurrentIndex;
        // In PlaySoundsFromList, index can be negative due to % operator in some C# versions if not handled, 
        // but the script uses Mathf.Abs(index) in PlayClip(). 
        // However, CurrentIndex returns index. Let's be safe.
        int displayIndex = Mathf.Abs(currentIndex) % playlist.audioClips.Count;
        AudioClip currentClip = playlist.audioClips[displayIndex];

        if (nowPlayingText != null)
            nowPlayingText.text = "Now Playing: " + currentClip.name;

        if (playlistText != null)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Playlist:");
            for (int i = 0; i < playlist.audioClips.Count; i++)
            {
                if (i == displayIndex)
                    sb.AppendLine("> " + playlist.audioClips[i].name);
                else
                    sb.AppendLine("  " + playlist.audioClips[i].name);
            }
            playlistText.text = sb.ToString();
        }
    }
}
