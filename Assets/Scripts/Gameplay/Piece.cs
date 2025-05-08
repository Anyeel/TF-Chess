using UnityEngine;

public class Piece
{
    public Vector2Int Position { get; private set; } // Posici�n l�gica en el tablero
    public bool IsBlack { get; private set; }
    public GameObject PieceGameObject { get; private set; } // Referencia al prefab instanciado
    private float yOffsetOnBoard; // El YOffset para la pieza en el tablero

    // Constructor usado por GameManager
    public Piece(Vector2Int startPosition, bool isBlack, GameObject pieceGameObject, float yOffset)
    {
        Position = startPosition;
        IsBlack = isBlack;
        PieceGameObject = pieceGameObject;
        this.yOffsetOnBoard = yOffset;

        // GameManager ya establece la posici�n visual inicial al instanciar.
        // Si quisieras asegurar la posici�n aqu� tambi�n:
        // PieceGameObject.transform.position = board.GetSquareAtPosition(startPosition.x, startPosition.y).Instance.transform.position + new Vector3(0, yOffsetOnBoard, 0);
        // Pero esto requerir�a pasar 'board' al constructor, lo cual es posible pero no estrictamente necesario si GameManager lo maneja bien.
    }

    // Actualiza la posici�n l�gica y visual de la pieza
    public void UpdatePosition(Vector2Int newLogicalPosition, Board boardReference)
    {
        Position = newLogicalPosition; // Actualizar posici�n l�gica

        if (PieceGameObject != null && boardReference != null)
        {
            Square targetSquare = boardReference.GetSquareAtPosition(newLogicalPosition.x, newLogicalPosition.y);
            if (targetSquare != null && targetSquare.Instance != null)
            {
                // Mover el GameObject a la nueva posici�n en el tablero, aplicando el YOffset
                PieceGameObject.transform.position = targetSquare.Instance.transform.position + new Vector3(0, yOffsetOnBoard, 0);
            }
            else
            {
                Debug.LogError($"Pieza: Casilla objetivo en {newLogicalPosition} no encontrada o su instancia es null.");
            }
        }
    }

    // Llamado cuando la pieza es recogida (para feedback visual)
    public void PickUp()
    {
        if (PieceGameObject != null)
        {
            // Ejemplo: Elevar la pieza un poco m�s para indicar que est� seleccionada
            // PieceGameObject.transform.position += new Vector3(0, 0.2f, 0); // Ajusta este valor como necesites
        }
        Debug.Log($"Pieza en {Position} recogida.");
    }

    // Llamado cuando la pieza es colocada o deseleccionada (para feedback visual)
    public void Place(Board boardReference)
    {
        if (PieceGameObject != null && boardReference != null)
        {
            // Asegurar que la pieza est� en su altura correcta sobre la casilla actual
            Square currentSquare = boardReference.GetSquareAtPosition(Position.x, Position.y);
            if (currentSquare != null && currentSquare.Instance != null)
            {
                PieceGameObject.transform.position = currentSquare.Instance.transform.position + new Vector3(0, yOffsetOnBoard, 0);
            }
        }
        Debug.Log($"Pieza en {Position} colocada/deseleccionada.");
    }
}