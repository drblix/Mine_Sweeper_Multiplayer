using UnityEngine;
using Mirror;
using TMPro;

public class Player : NetworkBehaviour
{
    [SerializeField] private GameObject _tile;

    private BoardManager _board;

    [SerializeField] [SyncVar] private byte _playerNum = 0;

    [SyncVar] private Color _playerColor;

    private Tile _currentTile;

    private TextMeshProUGUI _turnPrompt;
    private AudioSource _turnSource;
    private bool _turnChanged = false;

    private void Start()
    {
        _board = FindObjectOfType<BoardManager>();
        _turnPrompt = GameObject.Find("TurnPrompt").GetComponent<TextMeshProUGUI>();
        _turnSource = _turnPrompt.GetComponent<AudioSource>();
    }

    private void Update()
    {
        if (isLocalPlayer)
        {
            TileInteraction();
            PlayerInput();
            TurnPromptUpdate();
        }
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

    private void TurnPromptUpdate()
    {
        _turnPrompt.color = _playerColor;

        if (IsTurn())
        {
            if (_turnChanged)
                _turnSource.Play();

            _turnPrompt.SetText("It's your turn!");
            _turnChanged = false;
        }
        else
        {
            _turnChanged = true;
            _turnPrompt.SetText($"It's Player {_board.GetPlayerTurn()}'s turn!");
        }
    }

    private bool IsTurn() => _board.GetPlayerTurn() == _playerNum;
    public byte GetPlayerNum() => _playerNum;


    [Server]
    public void SetPlayerNum(byte num)
    {
        _playerNum = num;

        switch (_playerNum)
        {
            case 1:
                _playerColor = Color.red;
                break;
            case 2:
                _playerColor = Color.blue;
                break;
            case 3:
                _playerColor = Color.green;
                break;
            case 4:
                _playerColor = Color.yellow;
                break;
            default:
                _playerColor = Color.white;
                break;
        }
    }

}
