using UnityEngine;
using Mirror;
using TMPro;
using UnityEngine.UI;
using System;

public class BoardManager : NetworkBehaviour
{
    private NetworkManagerSweeper _networkSweeper;

    [SerializeField] private GameObject _tile;
    [SerializeField] private TMP_InputField _heightInput, _widthInput, _mineInput;
    [SerializeField] private Image _emoticon;
    [SerializeField] private Sprite[] _sprites;
    [SerializeField] private AudioClip _explosion, _tada;
    [SerializeField] private AudioSource _gameSource;

    [SyncVar] private byte _playerTurn = 1;

    [SyncVar] private byte _width, _height;
    [SyncVar] private ushort _revealedSafeTiles, _safeTiles, _minesAmount;

    [SyncVar] private bool _gameOver = false, _minesGenerated = false;

    [SyncVar] private int _generationSeed;

    private Tile[,] _board;
    private readonly SyncList<Tile> _syncBoard = new SyncList<Tile>();

    private readonly string[] _buttonNames = { "create", "beginner", "intermediate", "expert" };

    public enum TileSprites
    {
        Tile,
        One,
        Two,
        Three,
        Four,
        Five,
        Six,
        Seven,
        Eight,
        Empty,
        Flag,
        Mine,
        Happy,
        Dead,
        Win
    }

    public override void OnStartServer()
    {
        base.OnStartServer();
        _networkSweeper = FindObjectOfType<NetworkManagerSweeper>();
        _playerTurn = 1;
        RpcSetEmoticon(TileSprites.Happy);
    }

    // GetLength conversions:
    // _width = GetLength(0)
    // _height = GetLength(1)

    [Server]
    private void CreateBoard(byte height, byte width, ushort mines)
    {
        ClearBoard();

        _board = new Tile[width, height];

        _minesAmount = mines;
        _width = width;
        _height = height;
        _safeTiles = (ushort)(_width * _height - _minesAmount);

        _gameOver = false;
        _generationSeed = (int)DateTime.Now.Ticks;
        _revealedSafeTiles = 0;
        RpcSetEmoticon(TileSprites.Happy);

        float tileSize = _tile.transform.localScale.x;

        // If we didn't subtract by the tile size divided by 2, center would be on the edge of a tile
        Vector2 center = new ((tileSize * width / 2f) - (tileSize / 2f), (tileSize * height / 2f) - (tileSize / 2f));
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                GameObject newTile = Instantiate(_tile);
                newTile.transform.localPosition = new Vector2(x * tileSize - center.x, y * tileSize - center.y);
                newTile.GetComponent<Tile>().SetCoordinates(new Vector2Int(x, y));
                newTile.name = $"({x}, {y})";

                NetworkServer.Spawn(newTile);
                
                _board[x, y] = newTile.GetComponent<Tile>();
            }
        }

        _syncBoard.Clear();
        // Tile[] tempList = To1DSyncList(_board);
        SyncList<Tile> tempList = To1DSyncList(_board);
        for (int i = 0; i < tempList.Count; i++)
            _syncBoard.Add(tempList[i]);
    }

    private void ClearBoard()
    {
        if (_board != null)
        {
            foreach (Tile tile in _board)
            {
                if (tile)
                {
                    NetworkServer.Destroy(tile.gameObject);
                    // Destroy(tile.gameObject);
                    //if (isServer)
                    //    NetworkServer.Destroy(tile.gameObject);
                }
            }
        }

        _syncBoard.Clear();
    }

    /// <summary>
    /// Function to detect button presses
    /// </summary>
    /// <param name="name">Name of button pressed</param>
    [ServerCallback]
    public void ButtonPressed(string name)
    {
        // is the person who pressed button the host?
        if (isClient && isServer)
        {
            _playerTurn = 1;
            _minesGenerated = false;

            if (name.Equals(_buttonNames[0]))       // create button
            {
                // returns if any of the custom inputs are empty
                if (_heightInput.text == string.Empty
                    || _widthInput.text == string.Empty
                    || _mineInput.text == string.Empty) return;

                // create board using custom inputs of height, width, and mine count
                if (byte.TryParse(_heightInput.text, out byte height) && byte.TryParse(_widthInput.text, out byte width) && ushort.TryParse(_mineInput.text, out ushort mines))
                    CreateBoard(height, width, mines);
                else
                    CreateBoard(9, 9, 10);
            }
            else if (name.Equals(_buttonNames[1]))  // beginner button
            {
                // create 9x9 board with 10 mines 
                CreateBoard(9, 9, 10);
            }
            else if (name.Equals(_buttonNames[2]))  // intermediate button
            {
                // create 16x16 board with 40 mines
                CreateBoard(16, 16, 40);
            }
            else if (name.Equals(_buttonNames[3]))  // expert button
            {
                // create 16x30 board with 99 mines
                CreateBoard(16, 30, 99);
            }
        }
    }

    public void RevealAdjacentTiles(Tile tile)
    {
        Vector2Int coords = tile.GetCoordinates();

        for (int x = -1; x < 2; x++)
        {
            for (int y = -1; y < 2; y++)
            {
                int xPos = coords.x + x, yPos = coords.y + y;

                // Ensuring the coordinate is within bounds of the array and is not itself
                if ((xPos != coords.x || yPos != coords.y) && 
                xPos >= 0 && xPos < _width &&
                yPos >= 0 && yPos < _height)
                {
                    //Tile otherTile = _board[xPos, yPos];
                    Tile otherTile = _syncBoard[xPos * _height + yPos];

                    if (!otherTile.GetMine())
                        otherTile.CmdReveal();
                }
            }
        }
    }

    /// <summary>
    /// Gets number of adjacent tiles that are mined
    /// </summary>
    /// <param name="tile">Tile to check around</param>
    /// <returns>Number of adjacent mines</returns>
    public int GetAdjacentMines(GameObject tile)
    {
        Vector2Int coords = tile.GetComponent<Tile>().GetCoordinates();
        // Debug.Log($"Center: {coords}");

        int count = 0;

        // Iterating through all adjacent tiles (not including the calling tile) and checking if they have mines
        for (int x = -1; x < 2; x++)
        {
            for (int y = -1; y < 2; y++)
            {
                int xPos = coords.x + x, yPos = coords.y + y;

                // Ensuring the coordinate is within bounds of the array and is not itself
                if ((xPos != coords.x || yPos != coords.y) && 
                    xPos >= 0 && xPos < _width && 
                    yPos >= 0 && yPos < _height)
                {
                    // Debug.Log($"{xPos}, {yPos}");
                    if (_syncBoard[xPos * _height + yPos].GetMine())
                        count++;
                }
            }
        }

        return count;
    }

    public void PlantMines(Tile exception)
    {
        if (_minesAmount >= _width * _height)
        {
            Debug.LogWarning("Too many mines for board size");
            _minesAmount = 0;
        }

        _minesGenerated = true;

        UnityEngine.Random.InitState(_generationSeed);
        int count = 0;

        while (count < _minesAmount)
        {
            for (int x = 0; x < _width; x++)
            {
                for (int y = 0; y < _height; y++)
                {
                    if (count == _minesAmount)
                        break;

                    Tile tile = _syncBoard[x * _height + y];

                    if (UnityEngine.Random.Range(0, 13) == 0 && !tile.GetMine() && tile != exception)
                    {
                        // Debug.Log(tile.GetCoordinates() + " has been mined!");
                        tile.SetMine(true);
                        count++;
                    }
                }
            }
        }
    }

    public void TileRevealed(Tile tile)
    {
        if (tile.GetMine())
            PlayerLose();
        else
        {
            _revealedSafeTiles++;
            if (_safeTiles == _revealedSafeTiles)
                PlayerWin();
        }
    }

    private void PlayerWin()
    {
        _gameOver = true;
        _emoticon.sprite = GetSprite(TileSprites.Win);

        _gameSource.clip = _tada;
        _gameSource.Play();
    }

    private void PlayerLose()
    {
        _gameOver = true;
        _emoticon.sprite = GetSprite(TileSprites.Dead);

        _gameSource.clip = _explosion;
        _gameSource.Play();
    }

   
    private SyncList<Tile> To1DSyncList(Tile[,] input)
    {
        //Tile[] result = new Tile[input.GetLength(0) * input.GetLength(1)];
        SyncList<Tile> result = new();

        // int index = 0;
        for (int i = 0; i < input.GetLength(0); i++)
        {
            for (int z = 0; z < input.GetLength(1); z++)
            {
                result.Add(input[i, z]);
                // result[index++] = input[i, z];
            }
        }

        return result;
    }

    [ClientRpc]
    private void RpcSetEmoticon(TileSprites img) => _emoticon.sprite = GetSprite(img);

    [Command(requiresAuthority = false)]
    public void CmdPlayerPlayed()
    {
        _playerTurn++;
        if (_playerTurn > _networkSweeper.ConnectionAmount())
            _playerTurn = 1;

        Debug.Log($"Player played! It is now player {_playerTurn}'s turn!");
    }

    #region Getters

    public bool GetMineStatus() => _minesGenerated;
    public bool GetGameStatus() => _gameOver;
    public byte GetPlayerTurn() => _playerTurn;
    public byte GetHeight() => _height;
    public byte GetWidth() => _width;

    public Sprite GetSprite(TileSprites sprite) => _sprites[(int)sprite];
    public Sprite GetSprite(int sprite) => _sprites[sprite];

    #endregion
}
