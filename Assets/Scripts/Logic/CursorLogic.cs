using UnityEngine;
using System.Collections.Generic;

public class CursorLogic
{
    public Vector2Int currentPosition { get; private set; }
    private int boardWidth;
    private int boardHeight;
    private Board boardReference;
    private Piece heldPiece;

    private Vector2Int originalPickUpPosition;
    private List<Vector2Int> allowedMoveSquares;
    private const float pickUpOffset = 0.5f;


    public CursorLogic(int width, int height, Vector2Int startPosition, Board board)
    {
        boardWidth = width;
        boardHeight = height;
        currentPosition = startPosition;
        this.boardReference = board;
        this.heldPiece = null;
        this.allowedMoveSquares = new List<Vector2Int>();
    }

    public void Move(Vector2Int direction)
    {
        Vector2Int newPotentialPosition = currentPosition + direction;

        if (!IsHoldingPiece())
        {
            if (newPotentialPosition.x < 0) newPotentialPosition.x = boardWidth - 1;
            else if (newPotentialPosition.x >= boardWidth) newPotentialPosition.x = 0;

            if (newPotentialPosition.y < 0) newPotentialPosition.y = boardHeight - 1;
            else if (newPotentialPosition.y >= boardHeight) newPotentialPosition.y = 0;

            currentPosition = newPotentialPosition;
        }
        else
        {
            if (allowedMoveSquares.Contains(newPotentialPosition))
            {
                GameEntity entityAtTarget = boardReference.GetEntityAtPosition(newPotentialPosition);
                if (entityAtTarget == null || entityAtTarget is HealthPickUpLogic || newPotentialPosition == originalPickUpPosition)
                {
                    currentPosition = newPotentialPosition;

                    float currentHeight = heldPiece.pieceGameObject.transform.position.y;

                    heldPiece.UpdatePosition(currentPosition, boardReference);

                    Vector3 pos = heldPiece.pieceGameObject.transform.position;
                    heldPiece.pieceGameObject.transform.position = new Vector3(pos.x, currentHeight, pos.z);
                }
            }
        }
    }



    public void HandlePieceInteraction(bool isCurrentPlayerBlack)
    {
        if (heldPiece == null)
        {
            GameEntity entityOnSquare = boardReference.GetEntityAtPosition(currentPosition);
            if (entityOnSquare is Piece pieceToPick)
            {
                if (pieceToPick.isBlack == isCurrentPlayerBlack && pieceToPick.entityGameObject.activeInHierarchy)
                {
                    heldPiece = pieceToPick;
                    originalPickUpPosition = currentPosition;
                    boardReference.SetEntityAtPosition(originalPickUpPosition, null);

                    Vector3 currentPos = heldPiece.pieceGameObject.transform.position;
                    heldPiece.pieceGameObject.transform.position = new Vector3(currentPos.x, currentPos.y + pickUpOffset, currentPos.z);

                    CalculateAllowedMoveSquares();
                }
            }
        }
        else
        {
            GameEntity entityAtTarget = boardReference.GetEntityAtPosition(currentPosition);

            if (entityAtTarget is HealthPickUpLogic healthPickUp)
            {
                Vector3 currentPos = heldPiece.pieceGameObject.transform.position;
                heldPiece.pieceGameObject.transform.position = new Vector3(currentPos.x, currentPos.y - pickUpOffset, currentPos.z);

                healthPickUp.TryHealPiece(heldPiece);
                boardReference.SetEntityAtPosition(currentPosition, null);
                boardReference.SetEntityAtPosition(currentPosition, heldPiece);
                if (heldPiece.position != currentPosition)
                {
                    heldPiece.UpdatePosition(currentPosition, boardReference);
                }
                heldPiece = null;
                allowedMoveSquares.Clear();
                return;
            }

            if (entityAtTarget == null || currentPosition == originalPickUpPosition)
            {
                Vector3 currentPos = heldPiece.pieceGameObject.transform.position;
                heldPiece.pieceGameObject.transform.position = new Vector3(currentPos.x, currentPos.y - pickUpOffset, currentPos.z);

                boardReference.SetEntityAtPosition(currentPosition, heldPiece);
                if (heldPiece.position != currentPosition)
                {
                    heldPiece.UpdatePosition(currentPosition, boardReference);
                }
                heldPiece = null;
                allowedMoveSquares.Clear();
            }
        }
    }


    private void CalculateAllowedMoveSquares()
    {
        allowedMoveSquares.Clear();
        if (heldPiece == null) return;

        allowedMoveSquares.Add(originalPickUpPosition);

        Vector2Int[] directions = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };
        foreach (Vector2Int dir in directions)
        {
            Vector2Int targetPos = originalPickUpPosition + dir;

            if (!boardReference.IsOutOfBounds(targetPos))
            {
                GameEntity entityAtTarget = boardReference.GetEntityAtPosition(targetPos);
                if (entityAtTarget == null || entityAtTarget is HealthPickUpLogic)
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
            Vector3 currentPos = heldPiece.pieceGameObject.transform.position;
            heldPiece.pieceGameObject.transform.position = new Vector3(currentPos.x, currentPos.y - pickUpOffset, currentPos.z);

            GameEntity entityAtTarget = boardReference.GetEntityAtPosition(currentPosition);
            if (entityAtTarget == null || currentPosition == originalPickUpPosition)
            {
                boardReference.SetEntityAtPosition(currentPosition, heldPiece);
                if (heldPiece.position != currentPosition)
                {
                    heldPiece.UpdatePosition(currentPosition, boardReference);
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

    public Piece GetHeldPiece()
    {
        return heldPiece;
    }
}
