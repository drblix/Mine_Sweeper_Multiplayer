using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PresetButtons : MonoBehaviour
{
    [SerializeField] private TMP_InputField _widthField, _heightField, _mineField;

    private Board _board;

    private void Start()
    {
        _board = FindObjectOfType<Board>();
    }

    public void PresetClicked(string name)
    {
        if (name.Equals("beginner"))
        {
            _board.CreateTiles(10, 10, 10);
        }
        else if (name.Equals("intermediate"))
        {
            _board.CreateTiles(15, 15, 40);
        }
        else if (name.Equals("expert"))
        {
            _board.CreateTiles(30, 16, 99);
        }
    }
}
