using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class Player : NetworkBehaviour
{
    [SerializeField] private GameObject _tile;
    [SerializeField] private Transform _board;

    private float _xPos = 0f;

    private void Start()
    {
        _board = GameObject.FindGameObjectWithTag("Board").transform;
    }

    private void Update()
    {
        if (isLocalPlayer)
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                SpawnTile();
            }
        }
    }

    [Command]
    private void SpawnTile()
    {
        GameObject newTile = Instantiate(_tile);
        newTile.transform.SetParent(_board, false);
        newTile.transform.localPosition = new Vector2(_xPos, 0f);

        NetworkServer.Spawn(newTile);

        _xPos += 45f;
    }
}
