using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] int boardWidth = 5;
    [SerializeField] int boardHeight = 5;
    [SerializeField] GameObject whiteSquarePrefab;
    [SerializeField] GameObject blackSquarePrefab;
    [SerializeField] GameObject cursorPrefab;
    Vector2Int cursorPosition;

    Board board;
    Square square;
    void Start()
    {
        board = new Board(boardWidth, boardHeight, whiteSquarePrefab, blackSquarePrefab);
        //GameObject.Instantiate(cursorPrefab, );
    }

    void Update()
    {
        
    }
}
