using UnityEngine;
using System.Collections.Generic;

public class CursorLogic
{
    public Vector2Int CurrentPosition { get; private set; }
    private int boardWidth;
    private int boardHeight;
    private Board boardReference; // Referencia al tablero (que usa IGameEntity)
    private Piece heldPiece;      // La pieza específica que se está sosteniendo

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

    // AddPiece podría no ser necesario si GameManager maneja la lista global y CursorLogic
    // interactúa con el tablero para encontrar piezas. Si la mantienes, asegúrate de su propósito.
    public void AddPiece(Piece piece)
    {
        // Lógica si es necesaria
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
                // Permitir movimiento si la casilla destino está vacía o es la originalPickUpPosition
                // (ya que lógicamente la pieza ya no está en originalPickUpPosition)
                IGameEntity entityAtTarget = boardReference.GetEntityAtPosition(newPotentialPosition);
                if (entityAtTarget == null || newPotentialPosition == originalPickUpPosition)
                {
                    CurrentPosition = newPotentialPosition;
                    if (heldPiece != null)
                    {
                        // Actualiza la posición LÓGICA de la pieza y su VISUAL
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
                    originalPickUpPosition = CurrentPosition; // Guardar donde se cogió

                    // Quitar la pieza del tablero lógico desde su posición original
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
        else // Soltar la pieza que se está sosteniendo (heldPiece != null)
        {
            // La pieza se suelta en la posición ACTUAL del cursor.
            // La lógica en Move() ya debería haber asegurado que CurrentPosition es un destino válido
            // (vacío o la originalPickUpPosition).
            IGameEntity entityAtTarget = boardReference.GetEntityAtPosition(CurrentPosition);
            if (entityAtTarget == null || CurrentPosition == originalPickUpPosition) // Doble chequeo por seguridad
            {
                // Colocar la pieza lógicamente en el tablero en la nueva posición
                boardReference.SetEntityAtPosition(CurrentPosition, heldPiece);

                // Asegurar que la posición lógica de la pieza y su visual se actualicen
                // a donde se está soltando.
                if (heldPiece.Position != CurrentPosition)
                {
                    // Si el cursor (y la pieza) se movió desde originalPickUpPosition
                    heldPiece.UpdatePosition(CurrentPosition, boardReference);
                }
                Debug.Log($"Pieza soltada en {CurrentPosition}.");

                heldPiece = null; // El cursor ya no sostiene la pieza
                allowedMoveSquares.Clear(); // Limpiar la lista de movimientos permitidos
            }
            else
            {
                // Esto teóricamente no debería ocurrir si Move() restringe bien.
                Debug.LogError($"Intento de soltar pieza en {CurrentPosition} que está inesperadamente ocupada por otra entidad.");
            }
        }
    }

    private void CalculateAllowedMoveSquares()
    {
        allowedMoveSquares.Clear();
        if (heldPiece == null) return; // No debería pasar si se llama después de coger pieza

        // La casilla original donde se cogió siempre es una opción
        allowedMoveSquares.Add(originalPickUpPosition);

        Vector2Int[] directions = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };
        foreach (Vector2Int dir in directions)
        {
            Vector2Int targetPos = originalPickUpPosition + dir;

            if (targetPos.x >= 0 && targetPos.x < boardWidth &&
                targetPos.y >= 0 && targetPos.y < boardHeight)
            {
                // La casilla adyacente es un destino válido si está vacía
                // (Eventual lógica para health pickups iría aquí)
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
            // La pieza se suelta donde esté el cursor. Move() debería haber mantenido
            // el cursor en una casilla válida (original o adyacente vacía).
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
                // Fallback: si el cursor está en una casilla ocupada que no es la original,
                // devolver la pieza a su originalPickUpPosition.
                Debug.LogWarning($"ForceDrop: Cursor en {CurrentPosition} está ocupado. Devolviendo pieza a {originalPickUpPosition}.");
                boardReference.SetEntityAtPosition(originalPickUpPosition, heldPiece);
                if (heldPiece.Position != originalPickUpPosition)
                {
                    // Si la pieza estaba lógicamente en otro sitio (después de un Move), actualizarla
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

    public Piece GetHeldPiece() // Devuelve la Pieza específica
    {
        return heldPiece;
    }
}