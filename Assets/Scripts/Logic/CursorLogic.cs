using UnityEngine;

public class CursorLogic
{
    public Vector2Int CurrentPosition { get; private set; }
    private int boardWidth;
    private int boardHeight;
    private Piece selectedPiece = null;
    private Piece[,] pieces; // Array lógico del tablero
    private Vector2Int? initialPiecePosition = null;
    private Board board; // Referencia al objeto Board

    // Constructor modificado para recibir la referencia al Board
    public CursorLogic(int boardWidth, int boardHeight, Vector2Int startPosition, Board gameBoard)
    {
        this.boardWidth = boardWidth;
        this.boardHeight = boardHeight;
        CurrentPosition = startPosition;
        this.pieces = new Piece[boardWidth, boardHeight];
        this.board = gameBoard; // Guardar la referencia al board
    }

    public void Move(Vector2Int direction)
    {
        Vector2Int newTargetPosition = CurrentPosition + direction;

        if (selectedPiece != null)
        {
            if (initialPiecePosition.HasValue)
            {
                int distance = Mathf.Abs(newTargetPosition.x - initialPiecePosition.Value.x) +
                               Mathf.Abs(newTargetPosition.y - initialPiecePosition.Value.y);

                if (distance == 1 &&
                    newTargetPosition.x >= 0 && newTargetPosition.x < boardWidth &&
                    newTargetPosition.y >= 0 && newTargetPosition.y < boardHeight &&
                    pieces[newTargetPosition.x, newTargetPosition.y] == null)
                {
                    pieces[selectedPiece.Position.x, selectedPiece.Position.y] = null;
                    CurrentPosition = newTargetPosition;
                    pieces[CurrentPosition.x, CurrentPosition.y] = selectedPiece;

                    // Pasar la referencia del board para la actualización visual de la pieza
                    selectedPiece.UpdatePosition(CurrentPosition, board);

                    selectedPiece.Place(board); // Actualización visual al soltar
                    selectedPiece = null;
                    initialPiecePosition = null;
                }
            }
        }
        else
        {
            int wrappedX = (newTargetPosition.x + boardWidth) % boardWidth;
            int wrappedY = (newTargetPosition.y + boardHeight) % boardHeight;
            CurrentPosition = new Vector2Int(wrappedX, wrappedY);
        }
    }

    public void HandlePieceInteraction(bool isBlackPlayerTurn)
    {
        if (selectedPiece == null)
        {
            Piece pieceAtCursor = pieces[CurrentPosition.x, CurrentPosition.y];
            if (pieceAtCursor != null && pieceAtCursor.IsBlack == isBlackPlayerTurn)
            {
                selectedPiece = pieceAtCursor;
                selectedPiece.PickUp();
                initialPiecePosition = CurrentPosition;
            }
        }
        else
        {
            selectedPiece.Place(board); // Actualización visual al cancelar selección
            selectedPiece = null;
            initialPiecePosition = null;
        }
    }

    public void AddPiece(Piece piece)
    {
        if (piece.Position.x >= 0 && piece.Position.x < boardWidth &&
            piece.Position.y >= 0 && piece.Position.y < boardHeight)
        {
            pieces[piece.Position.x, piece.Position.y] = piece;
        }
        else
        {
            Debug.LogError($"Intento de añadir pieza fuera de los límites: {piece.Position}");
        }
    }
    public void ForceDropSelectedPiece()
    {
        if (selectedPiece != null)
        {
            Debug.Log($"Soltando pieza {selectedPiece.Position} automáticamente por cambio de turno.");
            // Usamos el método Place de la pieza para cualquier ajuste visual
            // La pieza está lógicamente en initialPiecePosition.Value (o selectedPiece.Position antes de moverse)
            selectedPiece.Place(board);

            selectedPiece = null;
            initialPiecePosition = null;
        }
    }
}