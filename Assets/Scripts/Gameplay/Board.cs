using UnityEngine;

public class Board
{
    public int Width { get; private set; }
    public int Height { get; private set; }
    private Square[,] squares;

    public Board(int width, int height, GameObject whiteSquarePrefab, GameObject blackSquarePrefab, Transform boardParentTransform = null)
    {
        Width = width;
        Height = height;
        squares = new Square[Width, Height];

        for (int x = 0; x < Width; x++)
        {
            for (int y = 0; y < Height; y++)
            {
                GameObject prefabToUse = ((x + y) % 2 == 0) ? whiteSquarePrefab : blackSquarePrefab;
                squares[x, y] = new Square(new Vector2Int(x, y), prefabToUse, boardParentTransform);
            }
        }
    }

    public Square GetSquareAtPosition(int x, int y)
    {
        if (x < 0 || x >= Width || y < 0 || y >= Height)
        {
            Debug.LogWarning($"Intento de acceder a Square fuera de límites: ({x},{y})");
            return null;
        }
        return squares[x, y];
    }

    public IGameEntity GetEntityAtPosition(Vector2Int position)
    {
        Square square = GetSquareAtPosition(position.x, position.y);
        return square?.ContainedEntity;
    }

    public void SetEntityAtPosition(Vector2Int position, IGameEntity entity)
    {
        Square square = GetSquareAtPosition(position.x, position.y);
        if (square != null)
        {
            square.ContainedEntity = entity;
            if (entity != null)
            {
                entity.Position = position; 
            }
        }
    }
}