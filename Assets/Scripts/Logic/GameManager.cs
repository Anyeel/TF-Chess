using UnityEngine;
using UnityEngine.UI; 
using System.Collections.Generic; 

public class GameManager : MonoBehaviour
{
    [SerializeField] int boardWidth = 5;
    [SerializeField] int boardHeight = 5;
    [SerializeField] GameObject whiteSquarePrefab;
    [SerializeField] GameObject blackSquarePrefab;
    [SerializeField] GameObject cursorPrefab;
    [SerializeField] Transform boardParentTransform;

    [SerializeField] Color whitePlayerColor = Color.red;
    [SerializeField] Color blackPlayerColor = Color.blue;
    [SerializeField] GameObject whitePiecePrefab; 
    [SerializeField] GameObject blackPiecePrefab;
    [SerializeField] float YOffsetForPieces = 0.5f;
    [SerializeField] int pieceNum = 3;
    [SerializeField] int damage = 1; 

    [SerializeField] float turnTime = 15f;
    [SerializeField] Image timerBar;
    [SerializeField] bool startWithRandomPlayer = true;

    private Board board;
    private CursorLogic cursorLogic;
    private CursorVisual cursorVisual;
    private List<Piece> allPieces = new List<Piece>();

    private float timeLeftInTurn;
    private bool isBlacksTurn;
    private bool gameOver = false;

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

        if (startWithRandomPlayer) isBlacksTurn = Random.Range(0, 2) == 0;
        else isBlacksTurn = false;
        
        StartNewTurn();
    }

    void Update()
    {
        if (timeLeftInTurn > 0) 
        {
            HandlePlayerInput();
        }

        UpdateTurnTimerVisuals();
        UpdatePieceCooldowns();
    }

    void HandlePlayerInput()
    {
        Vector2Int moveDirection = Vector2Int.zero;
        bool interactPressed = false;
        bool attackPressed = false;

        if (isBlacksTurn) 
        {
            if (Input.GetKeyDown(KeyCode.UpArrow)) moveDirection = Vector2Int.up;
            else if (Input.GetKeyDown(KeyCode.DownArrow)) moveDirection = Vector2Int.down;
            else if (Input.GetKeyDown(KeyCode.LeftArrow)) moveDirection = Vector2Int.left;
            else if (Input.GetKeyDown(KeyCode.RightArrow)) moveDirection = Vector2Int.right;

            if (Input.GetKeyDown(KeyCode.RightShift)) interactPressed = true;
            if (Input.GetKeyDown(KeyCode.Return)) attackPressed = true;
        }
        else 
        {
            if (Input.GetKeyDown(KeyCode.W)) moveDirection = Vector2Int.up;
            else if (Input.GetKeyDown(KeyCode.S)) moveDirection = Vector2Int.down;
            else if (Input.GetKeyDown(KeyCode.A)) moveDirection = Vector2Int.left;
            else if (Input.GetKeyDown(KeyCode.D)) moveDirection = Vector2Int.right;

            if (Input.GetKeyDown(KeyCode.LeftShift)) interactPressed = true;
            if (Input.GetKeyDown(KeyCode.Space)) attackPressed = true;
        }

        if (moveDirection != Vector2Int.zero)
        {
            cursorLogic.Move(moveDirection);
            UpdateCursorVisualPosition();
        }

        if (interactPressed)
        {
            cursorLogic.HandlePieceInteraction(isBlacksTurn);
        }

        if (attackPressed)
        {
            ExecuteAttack();
        }
    }

    void UpdateCursorVisualPosition()
    {
        if (cursorVisual == null || board == null || cursorLogic == null) return;

        Square currentCursorSquare = board.GetSquareAtPosition(cursorLogic.currentPosition.x, cursorLogic.currentPosition.y);
        if (currentCursorSquare != null && currentCursorSquare.instance != null)
        {
            cursorVisual.UpdatePosition(currentCursorSquare.instance.transform.position + cursorVisualOffset);
        }
    }


    void InitializePieces()
    {
        allPieces.Clear();

        int midX = boardWidth / 2;
        Vector2Int[] whiteStartPositions = {new Vector2Int(midX -1, 0), new Vector2Int(midX, 0), new Vector2Int(midX + 1, 0)};
        Vector2Int[] blackStartPositions = {new Vector2Int(midX -1, boardHeight -1), new Vector2Int(midX, boardHeight -1), new Vector2Int(midX + 1, boardHeight-1)};

        CreateInitialPieces(whiteStartPositions, whitePiecePrefab, false);
        CreateInitialPieces(blackStartPositions, blackPiecePrefab, true);
    }

    void CreateInitialPieces(Vector2Int[] startPositions, GameObject piecePrefab, bool isBlack)
    {
        for (int i = 0; i < pieceNum; i++)
        {
            Vector2Int pos = startPositions[i];
            Square pieceSquare = board.GetSquareAtPosition(pos.x, pos.y);
            if (pieceSquare != null && pieceSquare.instance != null)
            {
                Vector3 visualPos = pieceSquare.instance.transform.position + new Vector3(0, YOffsetForPieces, 0);
                GameObject pieceObj = Instantiate(piecePrefab, visualPos, Quaternion.identity);
                PieceVisual pv = pieceObj.GetComponent<PieceVisual>();

                Piece piece = new Piece(pos, isBlack, pieceObj, YOffsetForPieces, pv);
                allPieces.Add(piece);
                board.SetEntityAtPosition(pos, piece);
            }
        }
    }


    void ExecuteAttack()
    {
        if (cursorLogic.IsHoldingPiece())
        {
            Piece attackingPiece = cursorLogic.GetHeldPiece();
            if (attackingPiece != null && !attackingPiece.isOnAttackCooldown)
            {
                if (attackingPiece.isBlack == isBlacksTurn) 
                {
                    Vector2Int attackOrigin = attackingPiece.position;
                    Vector2Int[] attackDirections = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };

                    foreach (Vector2Int dir in attackDirections)
                    {
                        Vector2Int targetPos = attackOrigin + dir;

                        if (targetPos.x >= 0 && targetPos.x < boardWidth &&
                            targetPos.y >= 0 && targetPos.y < boardHeight)
                        {
                            IGameEntity entityOnSquare = board.GetEntityAtPosition(targetPos);

                            if (entityOnSquare != null) 
                            {
                                if (entityOnSquare is IAttackable attackableTarget)
                                {
                                    attackableTarget.TakeDamage(damage);
                                }
                            }
                        }
                    }
                    attackingPiece.StartAttackCooldown();
                }
            }
        }
    }

    void UpdatePieceCooldowns()
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

    void StartNewTurn()
    {
        timeLeftInTurn = turnTime;

        cursorVisual.UpdateMaterial(isBlacksTurn); 

        UpdateTimerBarColor();
    }

    void UpdateTurnTimerVisuals()
    {
        if (timerBar != null)
        {
            timerBar.fillAmount = Mathf.Clamp01(timeLeftInTurn / turnTime);
        }
    }

    void UpdateTimerBarColor()
    {
        if (timerBar != null)
        {
            timerBar.color = isBlacksTurn ? blackPlayerColor : whitePlayerColor;
            timerBar.fillOrigin = isBlacksTurn ? (int)Image.OriginHorizontal.Right : (int)Image.OriginHorizontal.Left;
        }
    }

    void FixedUpdate() 
    {
        if (timeLeftInTurn > 0)
        {
            timeLeftInTurn -= Time.fixedDeltaTime; 
            if (timeLeftInTurn <= 0)
            {
                timeLeftInTurn = 0;
                EndTurn();
            }
        }
    }

    void EndTurn()
    {

        if (cursorLogic != null)
        {
            cursorLogic.ForceDropSelectedPiece(); 
        }

        isBlacksTurn = !isBlacksTurn;
        CheckWinCondition();
        if (!IsGameOver()) 
        {
            StartNewTurn();
        }
    }

    void CheckWinCondition()
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
        else if (whitePiecesLeft == 0 && blackPiecesLeft == 0)
        {
            gameOver = true;
        }
    }

    void DeclareWinner(bool blackPlayerWins)
    {
        gameOver = true;
        string winner = blackPlayerWins ? "Jugador Negro" : "Jugador Blanco";
        Debug.Log($"¡JUEGO TERMINADO! El ganador es: {winner}");
        Time.timeScale = 0;
    }

    bool IsGameOver()
    {
        return gameOver;
    }
}