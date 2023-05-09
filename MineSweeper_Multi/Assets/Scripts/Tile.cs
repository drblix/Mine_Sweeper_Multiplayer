using Mirror;
using System;
using UnityEngine;

public class Tile : NetworkBehaviour
{
    private BoardManager _board;
    private SpriteRenderer _spriteRenderer;

    [SyncVar] [SerializeField] private Vector2Int _coordinates;

    [SerializeField] private bool _revealed = false, _hasMine = false, _flagged = false;

    private void Start()
    {
        _board = FindObjectOfType<BoardManager>();
        _spriteRenderer = GetComponent<SpriteRenderer>();
    }

    #region Rpc Functions

    [ClientRpc]
    public void RpcReveal()
    {
        if (_board == null) return;

        if (!_board.GetMineStatus())
        {
            _board.PlantMines(this);
            // due to server latency, the mines won't plant in time for the acknowledge to register that they did until next server tick
            // hence, why we are returning early
            return;
        }

        if (_revealed || _flagged) return;
        _revealed = true;


        int adjacentMines = _board.GetAdjacentMines(gameObject);
        if (_hasMine)
        {
            _spriteRenderer.sprite = _board.GetSprite(BoardManager.TileSprites.Mine);
        }
        else
        {
            if (adjacentMines > 0)
            {
                _spriteRenderer.sprite = _board.GetSprite(adjacentMines);
            }
            else
                _spriteRenderer.sprite = _board.GetSprite(BoardManager.TileSprites.Empty);
        }

        // Debug.Log(adjacentMines);
        if (adjacentMines == 0 && !_hasMine)
            _board.RevealAdjacentTiles(this);

        _board.TileRevealed(this);
    }

    [ClientRpc]
    private void RpcFlag()
    {
        if (_revealed) return;

        _flagged = !_flagged;
        _spriteRenderer.sprite = _flagged ? _board.GetSprite(BoardManager.TileSprites.Flag) : _board.GetSprite(BoardManager.TileSprites.Tile);
    }

    [ClientRpc]
    private void RpcSetColor(Color color)
    {
        if (_spriteRenderer != null)
            _spriteRenderer.color = color;
    }

    #endregion

    #region Command Functions

    [Command(requiresAuthority = false)]
    public void CmdSetColor(Color color) => RpcSetColor(color);

    [Command(requiresAuthority = false)]
    public void CmdReveal() => RpcReveal();

    [Command(requiresAuthority = false)]
    public void CmdFlag() => RpcFlag();

    #endregion

    #region Setters

    public void SetCoordinates(Vector2Int vector2) => _coordinates = vector2;
    public void SetMine(bool m) => _hasMine = m;

    #endregion

    #region Getters

    public Vector2Int GetCoordinates() => _coordinates;
    public bool GetMine() => _hasMine;
    public bool GetRevealed() => _revealed;

    #endregion
}