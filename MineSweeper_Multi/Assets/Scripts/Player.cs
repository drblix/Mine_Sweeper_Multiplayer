using UnityEngine;
using Mirror;

public class Player : NetworkBehaviour
{
    [SerializeField] private GameObject _tile;

    private BoardManager _board;

    [SerializeField] private byte _playerNum = 0;

    private Color _playerColor;
    private Tile _currentTile;

    private void Start()
    {
        _board = FindObjectOfType<BoardManager>();

        _playerColor = _playerNum == 1 ? Color.red : Color.blue;
    }

    private void Update()
    {
        if (isLocalPlayer)
        {
            TileInteraction();
            PlayerInput();

        }
        // Debug.Log(NetworkManager.singleton.networkAddress);

    }

    private void PlayerInput()
    {
        if (!IsTurn() || _board.GetGameStatus()) return;

        if (Input.GetMouseButtonDown(0))
        {
            if (_currentTile && !_currentTile.GetRevealed())
            {
                _currentTile.CmdReveal();

                if (_board.GetMineStatus())
                    _board.CmdPlayerPlayed();
            }
        }
        else if (Input.GetMouseButtonDown(1))
        {
            if (_currentTile)
            {
                _currentTile.CmdFlag();
            }
        }
    }

    private void TileInteraction()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit info, 50f) && IsTurn() && !_board.GetGameStatus())
        {
            if (info.collider.TryGetComponent(out Tile tile))
            {
                if (_currentTile != tile || !_tile)
                {
                    // GetComponent calls are assigning color on client-side to prevent visual latency over server calls

                    if (_currentTile)
                    {
                        _currentTile.GetComponent<SpriteRenderer>().color = Color.white;
                        _currentTile.CmdSetColor(Color.white);
                    }

                    _currentTile = tile;
                    tile.GetComponent<SpriteRenderer>().color = _playerColor;
                    tile.CmdSetColor(_playerColor);
                }
            }
        }
        else if (_currentTile)
        {
            _currentTile.GetComponent<SpriteRenderer>().color = Color.white;
            _currentTile.CmdSetColor(Color.white);
            _currentTile = null;
        }
    }

    private bool IsTurn() => _board.GetPlayerTurn() == _playerNum;

    [TargetRpc]
    public void SetPlayerNum(NetworkConnectionToClient _, byte num)
    {
        _playerNum = num;
        _playerColor = _playerNum == 1 ? Color.red : Color.blue;
    }
}
