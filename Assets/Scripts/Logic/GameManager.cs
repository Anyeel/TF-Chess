using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Collections;

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
    [SerializeField] MyCoroutineManager coroutineManager;

    [SerializeField] GameObject cratePrefab;
    [SerializeField] float spawnCrateSeconds = 20f;
    [SerializeField] GameObject healthPickUp;

    private Board board;
    private CursorLogic cursorLogic;
    private CursorVisual cursorVisual;
    private Piece[] allPieces;

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

        GameEvents.PieceDied.AddListener(OnPieceDied);
        GameEvents.GameIsOver.AddListener(OnGameIsOver);

        InitializePieces();
        StartCoroutine(SpawnRandomCrate());
    }

    void Update()
    {
        if (gameOver) return;
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
        int totalPieces = pieceNum * 2;
        allPieces = new Piece[totalPieces];

        List<Vector2Int> whiteStartPositionsList = new List<Vector2Int>();
        List<Vector2Int> blackStartPositionsList = new List<Vector2Int>();

        for (int i = 0; i < pieceNum; i++)
        {
            int xPos = i % boardWidth;
            int yRowOffset = i / boardWidth;
            int yPos = 0 + yRowOffset;
            if (pieceNum == 3) whiteStartPositionsList.Add(new Vector2Int(xPos + 1, yPos));
            else whiteStartPositionsList.Add(new Vector2Int(xPos, yPos));
        }

        for (int i = 0; i < pieceNum; i++)
        {
            int xPos = i % boardWidth;
            int yRowOffset = i / boardWidth;
            int yPos = (boardHeight - 1) - yRowOffset;
            if (pieceNum == 3) blackStartPositionsList.Add(new Vector2Int(xPos + 1, yPos));
            else blackStartPositionsList.Add(new Vector2Int(xPos, yPos));
        }

        int idx = 0;
        idx = CreateInitialPieces(whiteStartPositionsList.ToArray(), whitePiecePrefab, Type.WhitePiece, idx);
        CreateInitialPieces(blackStartPositionsList.ToArray(), blackPiecePrefab, Type.BlackPiece, idx);
    }

    int CreateInitialPieces(Vector2Int[] startPositions, GameObject piecePrefab, Type pieceType, int startIdx)
    {
        for (int i = 0; i < startPositions.Length; i++)
        {
            Vector2Int pos = startPositions[i];

            Square pieceSquare = board.GetSquareAtPosition(pos.x, pos.y);
            if (pieceSquare != null && pieceSquare.instance != null)
            {
                Vector3 visualPos = pieceSquare.instance.transform.position + new Vector3(0, 0, 0);
                GameObject pieceObj = Instantiate(piecePrefab, visualPos, Quaternion.identity);

                Piece piece = new Piece(pos, pieceType, pieceObj);

                allPieces[startIdx + i] = piece;
                board.SetEntityAtPosition(pos, piece);
            }
        }
        return startIdx + startPositions.Length;
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
            Piece attackingPiece = cursorLogic.GetHeldPiece();
            if (attackingPiece != null && !attackingPiece.isOnAttackCooldown)
            {
                if (attackingPiece.isBlack == turnManager.isBlacksTurn)
                {
                    Vector2Int attackOrigin = attackingPiece.position;
                    Vector2Int[] attackDirections = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };

                    foreach (Vector2Int dir in attackDirections)
                    {
                        Vector2Int targetPos = attackOrigin + dir;

                        if (targetPos.x >= 0 && targetPos.x < boardWidth &&
                            targetPos.y >= 0 && targetPos.y < boardHeight)
                        {
                            GameEntity entityOnSquare = board.GetEntityAtPosition(targetPos);

                            if (entityOnSquare is IAttackable attackableTarget)
                            {
                                attackableTarget.TakeDamage(damage);
                            }
                        }
                    }

                    int alivePieces = GetAlivePiecesCount(attackingPiece.isBlack);
                    float cooldownDuration = Mathf.Max(1f, attackingPiece.attackCooldownDuration * (alivePieces / (float)pieceNum));

                    attackingPiece.StartAttackCooldown(coroutineManager, cooldownDuration);
                }
            }
        }
    }


    public void UpdatePieceCooldowns()
    {
        for (int i = 0; i < allPieces.Length; i++)
        {
            Piece piece = allPieces[i];
            if (piece != null && !piece.pieceGameObject.activeInHierarchy)
            {
                if (board.GetEntityAtPosition(piece.position) == piece)
                {
                    board.SetEntityAtPosition(piece.position, null);
                }
                RemovePiece(piece);
                GameEvents.PieceDied.Invoke();
                break; // Salir de un bucle para evitar que se siga iterando
            }
        }
    }

    public void CheckWinCondition()
    {
        if (gameOver) return;

        int whiteCount = 0;
        int blackCount = 0;

        for (int i = 0; i < allPieces.Length; i++)
        {
            Piece piece = allPieces[i];
            if (piece != null && piece.pieceGameObject.activeInHierarchy && piece.currentHealth > 0)
            {
                if (piece.isBlack) blackCount++;
                else whiteCount++;
            }
        }

        if (whiteCount == 0 || blackCount == 0)
        {
            GameEvents.GameIsOver.Invoke();
        }
    }

    void OnPieceDied()
    {

        CheckWinCondition();
    }

    void RemovePiece(Piece pieceToRemove)
    {
        Piece[] newPiecesArray = new Piece[allPieces.Length - 1];

        int newIndex = 0;

        for (int i = 0; i < allPieces.Length; i++)
        {
            if (allPieces[i] != pieceToRemove)
            {
                newPiecesArray[newIndex] = allPieces[i];
                newIndex++;
            }
        }

        allPieces = newPiecesArray;
    }

    void OnGameIsOver()
    {
        if (gameOver) return;
        gameOver = true;

        bool anyWhite = false;
        bool anyBlack = false;

        for (int i = 0; i < allPieces.Length; i++)
        {
            Piece piece = allPieces[i];
            if (piece != null && piece.pieceGameObject.activeInHierarchy && piece.currentHealth > 0)
            {
                if (piece.isBlack) anyBlack = true;
                else anyWhite = true;
            }
        }
        string winner;

        if (!anyWhite && anyBlack) winner = "Jugador Negro";
        else winner = "Jugador Blanco";

        Debug.Log($"¡JUEGO TERMINADO! El ganador es: {winner}");
    }

    IEnumerator SpawnRandomCrate()
    {
        while (!gameOver)
        {
            List<Vector2Int> availablePositions = new List<Vector2Int>();

            for (int x = 0; x < boardWidth; x++)
            {
                for (int y = 0; y < boardHeight; y++)
                {
                    Vector2Int pos = new Vector2Int(x, y);

                    if (board.GetEntityAtPosition(pos) == null)
                    {
                        availablePositions.Add(pos);
                    }
                }
            }

            if (availablePositions.Count > 0)
            {
                Vector2Int randomPos = availablePositions[Random.Range(0, availablePositions.Count)];

                Square spawnSquare = board.GetSquareAtPosition(randomPos.x, randomPos.y);
                if (spawnSquare != null && spawnSquare.instance != null)
                {
                    Vector3 spawnPosition = spawnSquare.instance.transform.position;
                    GameObject crateObj = Instantiate(cratePrefab, spawnPosition, Quaternion.identity);

                    CrateLogic crate = new CrateLogic(randomPos, crateObj, Type.Crate, healthPickUp);
                    board.SetEntityAtPosition(randomPos, crate);
                }
            }
            yield return new WaitForSeconds(spawnCrateSeconds);
        }
    }


    public void SpawnHealthPickup(Vector3 position, GameObject healthPickupPrefab, Vector2Int logicalPosition)
    {
        GameObject healthPickupObj = Instantiate(healthPickupPrefab, position, Quaternion.identity);
        HealthPickUpLogic healthPickUpLogic = new HealthPickUpLogic(logicalPosition, healthPickupObj);
        board.SetEntityAtPosition(logicalPosition, healthPickUpLogic);
    }

    public Board GetBoard()
    {
        return board;
    }

    public int GetAlivePiecesCount(bool isBlack)
    {
        int count = 0;
        for (int i = 0; i < allPieces.Length; i++)
        {
            Piece piece = allPieces[i];
            if (piece != null && piece.pieceGameObject.activeInHierarchy && piece.isBlack == isBlack)
            {
                count++;
            }
        }
        return count;
    }

}
