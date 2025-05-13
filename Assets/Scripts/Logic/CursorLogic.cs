using UnityEngine;
using System.Collections.Generic;

public class CursorLogic
{
    public Vector2Int CurrentPosition { get; private set; }
    private int boardWidth;
    private int boardHeight;
    private Board boardReference; // Referencia al tablero (que usa IGameEntity)
    private Piece heldPiece;      // La pieza espec�fica que se est� sosteniendo

    // Para el movimiento restringido de "coger y mover a adyacente"
    private Vector2Int originalPickUpPosition;
    private List<Vector2Int> allowedMoveSquares;

    public CursorLogic(int width, int height, Vector2Int startPosition, Board board)
    {
        boardWidth = width;
        boardHeight = height;
        CurrentPosition = startPosition;
        this.boardReference = board;
        this.heldPiece = null;
        this.allowedMoveSquares = new List<Vector2Int>();
    }

    // AddPiece podr�a no ser necesario si GameManager maneja la lista global y CursorLogic
    // interact�a con el tablero para encontrar piezas. Si la mantienes, aseg�rate de su prop�sito.
    public void AddPiece(Piece piece)
    {
        // L�gica si es necesaria
    }

    public void Move(Vector2Int direction)
    {
        Vector2Int newPotentialPosition = CurrentPosition + direction;

        if (!IsHoldingPiece()) // Movimiento toroidal normal del cursor si no se sostiene pieza
        {
            if (newPotentialPosition.x < 0) newPotentialPosition.x = boardWidth - 1;
            else if (newPotentialPosition.x >= boardWidth) newPotentialPosition.x = 0;

            if (newPotentialPosition.y < 0) newPotentialPosition.y = boardHeight - 1;
            else if (newPotentialPosition.y >= boardHeight) newPotentialPosition.y = 0;

            CurrentPosition = newPotentialPosition;
        }
        else // Sosteniendo una pieza: movimiento restringido a 'allowedMoveSquares'
        {
            if (allowedMoveSquares.Contains(newPotentialPosition))
            {
                // Permitir movimiento si la casilla destino est� vac�a o es la originalPickUpPosition
                // (ya que l�gicamente la pieza ya no est� en originalPickUpPosition)
                IGameEntity entityAtTarget = boardReference.GetEntityAtPosition(newPotentialPosition);
                if (entityAtTarget == null || newPotentialPosition == originalPickUpPosition)
                {
                    CurrentPosition = newPotentialPosition;
                    if (heldPiece != null)
                    {
                        // Actualiza la posici�n L�GICA de la pieza y su VISUAL
                        heldPiece.UpdatePosition(CurrentPosition, boardReference);
                    }
                }
                else
                {
                    Debug.Log($"No se puede mover la pieza a {newPotentialPosition}, casilla ocupada por otra entidad.");
                }
            }
            else
            {
                Debug.Log($"No se puede mover la pieza a {newPotentialPosition}. Fuera del rango de movimiento permitido.");
            }
        }
    }

    public void HandlePieceInteraction(bool isCurrentPlayerBlack)
    {
        if (heldPiece == null) // Intentar coger una pieza
        {
            IGameEntity entityOnSquare = boardReference.GetEntityAtPosition(CurrentPosition);
            if (entityOnSquare is Piece pieceToPick) // Comprobar si la entidad es una Pieza
            {
                if (pieceToPick.IsBlack == isCurrentPlayerBlack && pieceToPick.EntityGameObject.activeInHierarchy)
                {
                    heldPiece = pieceToPick;
                    originalPickUpPosition = CurrentPosition; // Guardar donde se cogi�

                    // Quitar la pieza del tablero l�gico desde su posici�n original
                    boardReference.SetEntityAtPosition(originalPickUpPosition, null);

                    CalculateAllowedMoveSquares(); // Calcular casillas a las que se puede mover
                    Debug.Log($"Pieza en {originalPickUpPosition} cogida. Movimientos permitidos calculados.");
                }
                else if (pieceToPick.IsBlack != isCurrentPlayerBlack)
                {
                    Debug.Log("Intentando coger una pieza del oponente.");
                }
            }
            else
            {
                Debug.Log($"No hay una pieza en {CurrentPosition} para coger, o la entidad no es una pieza.");
            }
        }
        else // Soltar la pieza que se est� sosteniendo (heldPiece != null)
        {
            // La pieza se suelta en la posici�n ACTUAL del cursor.
            // La l�gica en Move() ya deber�a haber asegurado que CurrentPosition es un destino v�lido
            // (vac�o o la originalPickUpPosition).
            IGameEntity entityAtTarget = boardReference.GetEntityAtPosition(CurrentPosition);
            if (entityAtTarget == null || CurrentPosition == originalPickUpPosition) // Doble chequeo por seguridad
            {
                // Colocar la pieza l�gicamente en el tablero en la nueva posici�n
                boardReference.SetEntityAtPosition(CurrentPosition, heldPiece);

                // Asegurar que la posici�n l�gica de la pieza y su visual se actualicen
                // a donde se est� soltando.
                if (heldPiece.Position != CurrentPosition)
                {
                    // Si el cursor (y la pieza) se movi� desde originalPickUpPosition
                    heldPiece.UpdatePosition(CurrentPosition, boardReference);
                }
                Debug.Log($"Pieza soltada en {CurrentPosition}.");

                heldPiece = null; // El cursor ya no sostiene la pieza
                allowedMoveSquares.Clear(); // Limpiar la lista de movimientos permitidos
            }
            else
            {
                // Esto te�ricamente no deber�a ocurrir si Move() restringe bien.
                Debug.LogError($"Intento de soltar pieza en {CurrentPosition} que est� inesperadamente ocupada por otra entidad.");
            }
        }
    }

    private void CalculateAllowedMoveSquares()
    {
        allowedMoveSquares.Clear();
        if (heldPiece == null) return; // No deber�a pasar si se llama despu�s de coger pieza

        // La casilla original donde se cogi� siempre es una opci�n
        allowedMoveSquares.Add(originalPickUpPosition);

        Vector2Int[] directions = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };
        foreach (Vector2Int dir in directions)
        {
            Vector2Int targetPos = originalPickUpPosition + dir;

            if (targetPos.x >= 0 && targetPos.x < boardWidth &&
                targetPos.y >= 0 && targetPos.y < boardHeight)
            {
                // La casilla adyacente es un destino v�lido si est� vac�a
                // (Eventual l�gica para health pickups ir�a aqu�)
                IGameEntity entityAtTarget = boardReference.GetEntityAtPosition(targetPos);
                if (entityAtTarget == null)
                {
                    allowedMoveSquares.Add(targetPos);
                }
            }
        }
    }

    public void ForceDropSelectedPiece()
    {
        if (heldPiece != null)
        {
            // La pieza se suelta donde est� el cursor. Move() deber�a haber mantenido
            // el cursor en una casilla v�lida (original o adyacente vac�a).
            IGameEntity entityAtTarget = boardReference.GetEntityAtPosition(CurrentPosition);

            if (entityAtTarget == null || CurrentPosition == originalPickUpPosition)
            {
                boardReference.SetEntityAtPosition(CurrentPosition, heldPiece);
                if (heldPiece.Position != CurrentPosition)
                {
                    heldPiece.UpdatePosition(CurrentPosition, boardReference);
                }
                Debug.Log($"Pieza {heldPiece.Position} soltada forzosamente en {CurrentPosition} al final del turno.");
            }
            else
            {
                // Fallback: si el cursor est� en una casilla ocupada que no es la original,
                // devolver la pieza a su originalPickUpPosition.
                Debug.LogWarning($"ForceDrop: Cursor en {CurrentPosition} est� ocupado. Devolviendo pieza a {originalPickUpPosition}.");
                boardReference.SetEntityAtPosition(originalPickUpPosition, heldPiece);
                if (heldPiece.Position != originalPickUpPosition)
                {
                    // Si la pieza estaba l�gicamente en otro sitio (despu�s de un Move), actualizarla
                    heldPiece.UpdatePosition(originalPickUpPosition, boardReference);
                }
            }

            heldPiece = null;
            allowedMoveSquares.Clear();
        }
    }

    public bool IsHoldingPiece()
    {
        return heldPiece != null;
    }

    public Piece GetHeldPiece() // Devuelve la Pieza espec�fica
    {
        return heldPiece;
    }
}