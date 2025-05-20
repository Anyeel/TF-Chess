using UnityEngine;

public class Board
{
    public int width { get; private set; }
    public int height { get; private set; }
    private Square[,] squares;

    public Board(int width, int height, GameObject whiteSquarePrefab, GameObject blackSquarePrefab, Transform boardParentTransform = null)
    {
        this.width = width;
        this.height = height;
        squares = new Square[this.width, this.height];

        for (int x = 0; x < this.width; x++)
        {
            for (int y = 0; y < this.height; y++)
            {
                GameObject prefabToUse = ((x + y) % 2 == 0) ? whiteSquarePrefab : blackSquarePrefab;
                squares[x, y] = new Square(new Vector2Int(x, y), prefabToUse, boardParentTransform);
            }
        }
    }

    public Square GetSquareAtPosition(int x, int y)
    {
        if (x < 0 || x >= width || y < 0 || y >= height)
        {
            return null;
        }
        return this[x, y];
    }

    public GameEntity GetEntityAtPosition(Vector2Int position)
    {
        Square square = this[position.x, position.y];
        return square?.containedEntity;
    }

    public void SetEntityAtPosition(Vector2Int position, GameEntity entity)
    {
        Square square = this[position.x, position.y];
        square.containedEntity = entity;
        if (entity != null)
        {
            entity.position = position;
        }
    }

    public bool IsOutOfBounds(Vector2Int position)
    {
        return position.x < 0 || position.x >= width || position.y < 0 || position.y >= height;
    }

    public Square this[int x, int y]
    {
        get => squares[x, y];
        set => squares[x, y] = value;
    }
}
