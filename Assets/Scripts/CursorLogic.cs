using UnityEngine;

public class CursorLogic
{
    public Vector2Int CurrentPosition { get; private set; }
    private int boardWidth;
    private int boardHeight;

    public CursorLogic(int boardWidth, int boardHeight, Vector2Int startPosition)
    {
        this.boardWidth = boardWidth;
        this.boardHeight = boardHeight;
        CurrentPosition = startPosition;
    }

    public void Move(Vector2Int direction)
    {
        // Actualizar la posición del cursor
        Vector2Int newPosition = CurrentPosition + direction;

        // Aplicar la lógica toroidal
        if (newPosition.x < 0)
            newPosition.x = boardWidth - 1; // Salir por la izquierda, reaparecer en la derecha
        else if (newPosition.x >= boardWidth)
            newPosition.x = 0; // Salir por la derecha, reaparecer en la izquierda

        if (newPosition.y < 0)
            newPosition.y = boardHeight - 1; // Salir por abajo, reaparecer arriba
        else if (newPosition.y >= boardHeight)
            newPosition.y = 0; // Salir por arriba, reaparecer abajo

        // Asignar la nueva posición al cursor
        CurrentPosition = newPosition;
    }
}


