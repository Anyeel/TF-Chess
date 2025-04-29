using System.Collections.Generic;
using UnityEngine;

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
                // Calcular el índice en formato Vector2Int (base 1)
                Vector2Int index = new Vector2Int(i + 1, j + 1);

                // Seleccionar el prefab correcto
                GameObject prefab = (i + j) % 2 == 0 ? whiteSquarePrefab : blackSquarePrefab;

                // Crear el cuadrado con su índice
                squares[i, j] = new Square(new Vector2Int(i, j), prefab, index);
            }
        }
    }

    public Square GetSquareAtPosition(int x, int y)
    {
        if (x >= 0 && x < squares.GetLength(0) && y >= 0 && y < squares.GetLength(1))
        {
            return squares[x, y];
        }
        else
        {
            return null;
        }
    }
}
