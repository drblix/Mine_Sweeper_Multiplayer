using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class Tile : MonoBehaviour
{
    private Board _board;
    private Player _player;
    private Image _image;

    [SerializeField] private Color _hoverColor;

    [SerializeField] private bool _hasMine = false, _revealed = false, _flagged = false;

    private Vector2Int _coordinates;

    private void Start()
    {
        _board = FindObjectOfType<Board>();
        _player = FindObjectOfType<Player>();
        _image = GetComponent<Image>();
    }

    /// <summary>
    /// Reveals the tile and all nearby tiles if prerequisites met;
    /// Handles sprite choosing as well
    /// </summary>
    public void Reveal()
    {
        // Reveal tile
        // If mined, game over
        // If not mined, reveal and assign sprite number if nearby mines
        // Reveal nearby tiles if no adjacent mines

        if (_revealed) return;
        _revealed = true;

        if (_hasMine)
        {
            _image.sprite = _board.GetSprite(Board.Sprites.Mine);

            if (!_board.GameOver)
                _image.color = Color.red;
        }
        else
        {
            int adjacentMines = GetAdjacentMines();

            if (adjacentMines > 0)
                _image.sprite = _board.GetSprite(adjacentMines);
            else
                _image.sprite = _board.GetSprite(Board.Sprites.Empty);
        }

        if (GetAdjacentMines() == 0 && !_hasMine)
            _board.RevealAdjacentTiles(this);

        _board.TileRevealed(this);
    }

    #region Mouse Events
   
    /// <summary>
    /// Method to handle mouse click event
    /// </summary>
    public void MouseClick(BaseEventData data)
    {
        if (_revealed || _board.GameOver) return;

        if (!_board.MinesGenerated) _board.PlantMines(this);

        PointerEventData pointerData = (PointerEventData)data;

        // Debug.Log(pointerData.button);
        if (pointerData.button == PointerEventData.InputButton.Left && !_flagged)
        {
            Reveal();
        }
        else if (pointerData.button == PointerEventData.InputButton.Right)
        {
            // Toggle flag placement
            _flagged = !_flagged;
            _player.PlaySound(Player.SoundClips.Select, Player.Sources.Board);
            _image.sprite = _flagged ? _board.GetSprite(Board.Sprites.Flag) : _board.GetSprite(Board.Sprites.Tile);
        }
    }

    /// <summary>
    /// Method to handle the mouse entering a tile
    /// </summary>
    public void MouseEnter()
    {
        if (_board.GameOver)
        {
            _image.color = Color.white;
            return;
        }

        if (!_revealed)
            _image.color = _hoverColor;
    }

    /// <summary>
    /// Method to handle the mouse exiting a tile
    /// </summary>
    public void MouseExit()
    {
        if (!_board.GameOver)
            _image.color = Color.white;
    }

    #endregion

    #region Getters

    public int GetAdjacentMines() => Board.GetAdjacentMines(this);
    public bool GetMine() => _hasMine;
    public bool GetRevealed() => _revealed;
    public Vector2Int GetCoordinates() => _coordinates;

    #endregion

    #region Setters

    public void SetMine(bool b) => _hasMine = b;
    public void SetRevealed(bool b) => _revealed = b;
    public void SetCoordinates(Vector2Int v) => _coordinates = v;

    #endregion
}
