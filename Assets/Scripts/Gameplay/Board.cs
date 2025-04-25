using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using UnityEngine;
using UnityEngine.UIElements;

public class Board
{
    Square[,] squares;

    public Board(int width, int height, GameObject whiteSquarePrefab, GameObject blackSquarePrefab)
    {
        squares = new Square[width, height];

        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                if ((i + j) % 2 == 0)
                {
                    squares[i, j] = new Square(new Vector2Int(i, j), whiteSquarePrefab);
                }
                else
                {
                    squares[i, j] = new Square(new Vector2Int(i, j), blackSquarePrefab);
                }
            }
        }
    }
}
