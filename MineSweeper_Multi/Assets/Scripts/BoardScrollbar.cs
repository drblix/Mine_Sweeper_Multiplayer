using UnityEngine;
using UnityEngine.UI;

public class BoardScrollbar : MonoBehaviour
{
    [SerializeField] private RectTransform _boardRect;

    private Board _board;
    private Scrollbar _scrollbar;

    [SerializeField] private bool _horizontal = true;

    private void Awake() 
    {
        _board = FindObjectOfType<Board>();
        _scrollbar = GetComponent<Scrollbar>();
    }

    private void Update()
    {
        float inputVal;

        if (_horizontal)
            inputVal = Input.GetAxisRaw("Horizontal");
        else
            inputVal = Input.GetAxisRaw("Vertical");

        if (!Mathf.Approximately(inputVal, 0f))
        {
            _scrollbar.value += inputVal * Time.deltaTime;
            _scrollbar.value = Mathf.Clamp01(_scrollbar.value);
            ScrollbarChanged(_scrollbar.value);
        }
    }

    public void ScrollbarChanged(float newValue)
    {
        float boardWidth = _board.GetBoardWidth() / 2f;
        float boardHeight = _board.GetBoardHeight() / 2f;

        if (_horizontal)
        {
            float lerp = Mathf.Lerp(-boardWidth, boardWidth, newValue);
            _boardRect.localPosition = new Vector2(lerp, _boardRect.localPosition.y);
        }
        else
        {
            float lerp = Mathf.Lerp(-boardHeight, boardHeight, newValue);
            _boardRect.localPosition = new Vector2(_boardRect.localPosition.x, -lerp);
        }
    }
}
