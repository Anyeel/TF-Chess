using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public struct InputKeys
{
    public KeyCode up;
    public KeyCode down;
    public KeyCode left;
    public KeyCode right;
    public KeyCode interact;
    public KeyCode attack;

    public InputKeys(KeyCode up, KeyCode down, KeyCode left, KeyCode right, KeyCode interact, KeyCode attack)
    {
        this.up = up;
        this.down = down;
        this.left = left;
        this.right = right;
        this.interact = interact;
        this.attack = attack;
    }
}

public class GameManager : MonoBehaviour
{
    [SerializeField] int boardWidth = 5;
    [SerializeField] int boardHeight = 5;
    [SerializeField] GameObject whiteSquarePrefab;
    [SerializeField] GameObject blackSquarePrefab;
    [SerializeField] GameObject cursorPrefab;
    [SerializeField] Transform boardParentTransform;

    [SerializeField] GameObject whitePiecePrefab;
    [SerializeField] GameObject blackPiecePrefab;
    [SerializeField] int pieceNum = 3;
    [SerializeField] int damage = 1;

    [SerializeField] TurnManager turnManager;

    private Board board;
    private CursorLogic cursorLogic;
    private CursorVisual cursorVisual;
    private List<Piece> allPieces = new List<Piece>();

    private InputKeys blackKeys = new InputKeys(KeyCode.UpArrow, KeyCode.DownArrow, KeyCode.LeftArrow, KeyCode.RightArrow, KeyCode.RightShift, KeyCode.Return);
    private InputKeys whiteKeys = new InputKeys(KeyCode.W, KeyCode.S, KeyCode.A, KeyCode.D, KeyCode.LeftShift, KeyCode.Space);

    private bool gameOver = false;
    public bool IsGameOver() => gameOver;

    private Vector3 cursorVisualOffset = new Vector3(-0.5f, 0.1f, 0);

    void Start()
    {
        Transform parentForSquares = boardParentTransform != null ? boardParentTransform : transform;
        board = new Board(boardWidth, boardHeight, whiteSquarePrefab, blackSquarePrefab, parentForSquares);

        Vector2Int cursorStartPos = new Vector2Int(boardWidth / 2, boardHeight / 2);
        cursorLogic = new CursorLogic(boardWidth, boardHeight, cursorStartPos, board);

        Square startSquareForCursorVisual = board.GetSquareAtPosition(cursorStartPos.x, cursorStartPos.y);

        Vector3 visualCursorPos = startSquareForCursorVisual.instance.transform.position + cursorVisualOffset;
        GameObject cursorObject = Instantiate(cursorPrefab, visualCursorPos, Quaternion.identity);
        cursorVisual = cursorObject.GetComponent<CursorVisual>();

        InitializePieces();
    }

    void Update()
    {
        HandlePlayerInput();
        UpdatePieceCooldowns();
    }

    void HandlePlayerInput()
    {
        InputKeys keys = turnManager.isBlacksTurn ? blackKeys : whiteKeys;

        Vector2Int moveDirection = Vector2Int.zero;
        bool interactPressed = false;
        bool attackPressed = false;

        if (Input.GetKeyDown(keys.up)) moveDirection = Vector2Int.up;
        else if (Input.GetKeyDown(keys.down)) moveDirection = Vector2Int.down;
        else if (Input.GetKeyDown(keys.left)) moveDirection = Vector2Int.left;
        else if (Input.GetKeyDown(keys.right)) moveDirection = Vector2Int.right;

        if (Input.GetKeyDown(keys.interact)) interactPressed = true;
        if (Input.GetKeyDown(keys.attack)) attackPressed = true;

        if (moveDirection != Vector2Int.zero)
        {
            cursorLogic.Move(moveDirection);
            UpdateCursorVisualPosition();
        }
        if (interactPressed)
        {
            cursorLogic.HandlePieceInteraction(turnManager.isBlacksTurn);
        }
        if (attackPressed)
        {
            ExecuteAttack();
        }
    }

    void UpdateCursorVisualPosition()
    {
        Square currentCursorSquare = board.GetSquareAtPosition(cursorLogic.currentPosition.x, cursorLogic.currentPosition.y);
        cursorVisual.UpdatePosition(currentCursorSquare.instance.transform.position + cursorVisualOffset);
    }

    void InitializePieces()
    {
        allPieces.Clear();

        List<Vector2Int> whiteStartPositionsList = new List<Vector2Int>();
        List<Vector2Int> blackStartPositionsList = new List<Vector2Int>();

        int whitePiecesPlaced = 0;
        for (int i = 0; i < pieceNum; i++)
        {
            int xPos = i % boardWidth;
            int yRowOffset = i / boardWidth;
            int yPos = 0 + yRowOffset;
            if (pieceNum == 3) whiteStartPositionsList.Add(new Vector2Int(xPos + 1, yPos));
            else whiteStartPositionsList.Add(new Vector2Int(xPos, yPos));
            whitePiecesPlaced++;
        }

        int blackPiecesPlaced = 0;
        for (int i = 0; i < pieceNum; i++)
        {
            int xPos = i % boardWidth;
            int yRowOffset = i / boardWidth;
            int yPos = (boardHeight - 1) - yRowOffset;

            if (pieceNum == 3) blackStartPositionsList.Add(new Vector2Int(xPos + 1, yPos));
            else blackStartPositionsList.Add(new Vector2Int(xPos, yPos));
            blackPiecesPlaced++;
        }

        CreateInitialPieces(whiteStartPositionsList.ToArray(), whitePiecePrefab, false);
        CreateInitialPieces(blackStartPositionsList.ToArray(), blackPiecePrefab, true);
    }

    void CreateInitialPieces(Vector2Int[] startPositions, GameObject piecePrefab, bool isBlack)
    {
        for (int i = 0; i < startPositions.Length; i++)
        {
            Vector2Int pos = startPositions[i];

            Square pieceSquare = board.GetSquareAtPosition(pos.x, pos.y);
            if (pieceSquare != null && pieceSquare.instance != null)
            {
                Vector3 visualPos = pieceSquare.instance.transform.position + new Vector3(0, 0, 0);
                GameObject pieceObj = Instantiate(piecePrefab, visualPos, Quaternion.identity);

                Piece piece = new Piece(pos, isBlack, pieceObj);
                allPieces.Add(piece);
                board.SetEntityAtPosition(pos, piece);
            }
        }
    }

    public void ForceDropSelectedPieceOnCurrentPlayer()
    {
        if (cursorLogic != null)
        {
            cursorLogic.ForceDropSelectedPiece();
        }
    }

    void ExecuteAttack()
    {
        if (cursorLogic.IsHoldingPiece())
        {
            Piece attackingPiece = cursorLogic.GetHeldPiece();// board[cursorLogic.currentPosition].containedEntity
            if (attackingPiece != null && !attackingPiece.isOnAttackCooldown)
            {
                if (attackingPiece.isBlack == turnManager.isBlacksTurn)
                {
                    Vector2Int attackOrigin = attackingPiece.position;
                    Vector2Int[] attackDirections = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };

                    foreach (Vector2Int dir in attackDirections)
                    {
                        Vector2Int targetPos = attackOrigin + dir;

                        if (targetPos.x >= 0 && targetPos.x < boardWidth && //board.IsOOB(pos)
                            targetPos.y >= 0 && targetPos.y < boardHeight)
                        {
                            GameEntity entityOnSquare = board.GetEntityAtPosition(targetPos);

                            if (entityOnSquare is IAttackable attackableTarget)
                            {
                                attackableTarget.TakeDamage(damage);
                            }
                        }
                    }
                    attackingPiece.StartAttackCooldown();
                }
            }
        }
    }
    public void UpdatePieceCooldowns()
    {
        float dt = Time.deltaTime;
        for (int i = allPieces.Count - 1; i >= 0; i--)
        {
            Piece piece = allPieces[i];
            if (piece.pieceGameObject.activeInHierarchy)
            {
                piece.Cooldown(dt);
            }
            else
            {
                if (board.GetEntityAtPosition(piece.position) == piece)
                {
                    board.SetEntityAtPosition(piece.position, null);
                }
                allPieces.RemoveAt(i);
                CheckWinCondition();
            }
        }
    }
    public void CheckWinCondition()
    {
        if (gameOver) return;

        int whitePiecesLeft = 0;
        int blackPiecesLeft = 0;

        foreach (Piece piece in allPieces)
        {
            if (piece.pieceGameObject.activeInHierarchy && piece.currentHealth > 0)
            {
                if (piece.isBlack) blackPiecesLeft++;
                else whitePiecesLeft++;
            }
        }

        if (whitePiecesLeft == 0 && blackPiecesLeft > 0)
        {
            DeclareWinner(true);
        }
        else if (blackPiecesLeft == 0 && whitePiecesLeft > 0)
        {
            DeclareWinner(false);
        }
    }

    void DeclareWinner(bool blackPlayerWins)
    {
        if (gameOver) return;
        gameOver = true;
        string winner = blackPlayerWins ? "Jugador Negro" : "Jugador Blanco";
        Debug.Log($"¡JUEGO TERMINADO! El ganador es: {winner}");
    }
}