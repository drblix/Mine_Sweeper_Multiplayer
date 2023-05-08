using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class PlayerSounds : MonoBehaviour
{
    [SerializeField] private AudioClip[] _sounds;

    private AudioSource _playerSource;

    private bool _pressing = false;

    private void Start()
    {
        _playerSource = GetComponent<AudioSource>();
    }

    private void Update()
    {
        // Checks if any mouse button is down (except scroll wheel)
        if (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1))
        {
            _playerSource.clip = _sounds[0];
            _playerSource.Play();
        }
        if (Input.GetMouseButtonUp(0) || Input.GetMouseButtonUp(1))
        {
            _playerSource.clip = _sounds[1];
            _playerSource.Play();
        }

        // Checks if any key is down AND none of the mouse buttons are being used
        if (Input.anyKey && !_pressing && !(Input.GetMouseButton(0) || Input.GetMouseButton(1)))
        {
            _playerSource.clip = _sounds[2];
            _playerSource.Play();
            _pressing = true;
        }
        if (!Input.anyKey && _pressing && !(Input.GetMouseButton(0) || Input.GetMouseButton(1)))
        {
            _playerSource.clip = _sounds[3];
            _playerSource.Play();
            _pressing = false;
        }
    }
}
