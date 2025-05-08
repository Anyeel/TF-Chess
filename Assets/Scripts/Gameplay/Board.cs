using UnityEngine;

public class Board
{
    private Square[,] squares;

    public int Width { get; private set; }
    public int Height { get; private set; }

    public Board(int width, int height, GameObject whiteSquarePrefab, GameObject blackSquarePrefab)
    {
        if (width <= 0 || height <= 0)
        {
            throw new System.ArgumentException("El ancho y alto del tablero deben ser mayores que 0.");
        }

        Width = width;
        Height = height;

        squares = new Square[width, height];

        InitializeSquares(whiteSquarePrefab, blackSquarePrefab);
    }

    private void InitializeSquares(GameObject whiteSquarePrefab, GameObject blackSquarePrefab)
    {
        for (int i = 0; i < Width; i++)
        {
            for (int j = 0; j < Height; j++)
            {
                squares[i, j] = CreateSquare(i, j, whiteSquarePrefab, blackSquarePrefab);
            }
        }
    }

    private Square CreateSquare(int x, int y, GameObject whiteSquarePrefab, GameObject blackSquarePrefab)
    {
        Vector2Int index = new Vector2Int(x + 1, y + 1);
        GameObject prefab = (x + y) % 2 == 0 ? whiteSquarePrefab : blackSquarePrefab;
        return new Square(new Vector2Int(x, y), prefab, index);
    }

    public Square GetSquareAtPosition(int x, int y)
    {
        return IsValidPosition(x, y) ? squares[x, y] : null;
    }

    private bool IsValidPosition(int x, int y)
    {
        return x >= 0 && x < Width && y >= 0 && y < Height;
    }
}
