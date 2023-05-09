using UnityEngine;
using UnityEngine.UI;
using Mirror;

[RequireComponent(typeof(Scrollbar))]
public class CameraScroll : MonoBehaviour
{
    private Transform _playerCam;
    private BoardManager _board;
    private Scrollbar _scrollbar;

    [SerializeField] private bool _isVertical = false;

    private void Start()
    {
        _playerCam = Camera.main.transform;
        _board = FindObjectOfType<BoardManager>();
        _scrollbar = GetComponent<Scrollbar>();
    }

    private void Update()
    {
        float input;

        if (_isVertical)
            input = Input.GetAxis("Vertical");
        else
            input = Input.GetAxis("Horizontal");

        if (!Mathf.Approximately(input, 0f))
        {
            _scrollbar.value += input * Time.deltaTime;
            _scrollbar.value = Mathf.Clamp01(_scrollbar.value);
            ScrollBarChanged(_scrollbar.value);
        }
    }

    public void ScrollBarChanged(float newVal)
    {
        if (!_board)
        {
            _board = FindObjectOfType<BoardManager>();
            if (!_board) return;
        }

        float height = _board.GetHeight() * .2f, width = _board.GetWidth() * .2f;

        if (_isVertical)
        {
            float lerp = Mathf.Lerp(-height, height, newVal);
            _playerCam.position = new Vector3(_playerCam.position.x, lerp, -3f);
        }
        else
        {
            float lerp = Mathf.Lerp(-width, width, newVal);
            _playerCam.position = new Vector3(lerp, _playerCam.position.y, -3f);
        }
    }
}
