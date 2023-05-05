using UnityEngine;

public class Player : MonoBehaviour
{
    [SerializeField] private AudioClip[] _soundClips;
    [SerializeField] private AudioSource[] _audioSources;

    public enum SoundClips
    {
        MouseDown,
        MouseUp,
        Select,
        Explosion,
        Tada
    }

    public enum Sources
    {
        Mouse,
        Board
    }

    private void Update()
    {
        ClickSound();
    }

    /// <summary>
    /// Handles clicking sounds from user's mouse
    /// </summary>
    private void ClickSound()
    {
        if ((Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1)))
            PlaySound(SoundClips.MouseDown, Sources.Mouse);
        else if (Input.GetMouseButtonUp(0) || Input.GetMouseButtonUp(1))
            PlaySound(SoundClips.MouseUp, Sources.Mouse);
    }

    /// <summary>
    /// Plays a sound
    /// </summary>
    /// <param name="clip">Clip to play</param>
    /// <param name="src">Source to play clip from</param>
    public void PlaySound(SoundClips clip, Sources src)
    {
        AudioSource source = _audioSources[(int)src];
        source.clip = _soundClips[(int)clip];
        source.Play();
    }
}
