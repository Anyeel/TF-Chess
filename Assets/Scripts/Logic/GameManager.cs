using UnityEngine;
using UnityEngine.UI; // Para Image
using System.Collections;
using System.Collections.Generic; // Para List

public class GameManager : MonoBehaviour
{
    [Header("Board Setup")]
    [SerializeField] int boardWidth = 5;
    [SerializeField] int boardHeight = 5;
    [SerializeField] GameObject whiteSquarePrefab;
    [SerializeField] GameObject blackSquarePrefab;
    [SerializeField] GameObject cursorPrefab;
    [SerializeField] Transform boardParentTransform; // Opcional: para organizar casillas en la jerarquía

    [Header("Player Settings")]
    [SerializeField] Color whitePlayerColor = Color.red; // Visualmente es el P1 (WASD)
    [SerializeField] Color blackPlayerColor = Color.blue; // Visualmente es el P2 (Flechas)
    [SerializeField] GameObject whitePiecePrefab; // Piezas del jugador "Blanco" (P1)
    [SerializeField] GameObject blackPiecePrefab; // Piezas del jugador "Negro" (P2)
    [SerializeField] float YOffsetForPieces = 0.5f; // Altura de las piezas sobre la casilla

    [Header("Turn Mechanics")]
    [SerializeField] float turnTime = 15f; // Tiempo por turno
    [SerializeField] Image timerBar;
    [SerializeField] bool startWithRandomPlayer = true;

    // --- Internal State ---
    private Board board;
    private CursorLogic cursorLogic;
    private CursorVisual cursorVisual;
    private List<Piece> allPieces = new List<Piece>();

    private float timeLeftInTurn;
    private bool isBlacksTurn; // true si es el turno del jugador "Negro" (P2 - Flechas)

    // Offset para la posición visual del cursor respecto al centro de la casilla
    private Vector3 cursorVisualOffset = new Vector3(-0.5f, 0.1f, 0); // Ajusta esto según tu prefab de cursor

    void Start()
    {
        // Crear el tablero lógico y visual
        Transform parentForSquares = boardParentTransform != null ? boardParentTransform : transform;
        board = new Board(boardWidth, boardHeight, whiteSquarePrefab, blackSquarePrefab, parentForSquares);

        // Configurar Cursor Lógico
        Vector2Int cursorStartPos = new Vector2Int(boardWidth / 2, boardHeight / 2);
        cursorLogic = new CursorLogic(boardWidth, boardHeight, cursorStartPos, board);

        // Instanciar y configurar Cursor Visual
        Square startSquareForCursorVisual = board.GetSquareAtPosition(cursorStartPos.x, cursorStartPos.y);
        if (startSquareForCursorVisual != null && startSquareForCursorVisual.Instance != null)
        {
            Vector3 visualCursorPos = startSquareForCursorVisual.Instance.transform.position + cursorVisualOffset;
            GameObject cursorObject = Instantiate(cursorPrefab, visualCursorPos, Quaternion.identity);
            cursorVisual = cursorObject.GetComponent<CursorVisual>();
        }
        else
        {
            Debug.LogError("No se pudo obtener la casilla inicial para el cursor visual.");
            // Fallback por si acaso
            GameObject cursorObject = Instantiate(cursorPrefab, new Vector3(cursorStartPos.x, cursorVisualOffset.y, cursorStartPos.y), Quaternion.identity);
            cursorVisual = cursorObject.GetComponent<CursorVisual>();
        }

        // Inicializar piezas
        InitializePieces();

        // Decidir jugador inicial
        if (startWithRandomPlayer)
        {
            isBlacksTurn = Random.Range(0, 2) == 0;
        }
        else
        {
            isBlacksTurn = false; // Blanco (P1) empieza por defecto
        }

        // Iniciar primer turno
        StartNewTurn();
    }

    void Update()
    {
        if (timeLeftInTurn > 0) // Solo procesar input si el turno está activo
        {
            HandlePlayerInput(); // Combina movimiento, interacción y ataque
        }

        UpdateTurnTimerVisuals();
        TickPieceCooldowns();
    }

    void HandlePlayerInput()
    {
        // --- Movimiento del Cursor ---
        Vector2Int moveDirection = Vector2Int.zero;
        // --- Interacción (Coger/Soltar Pieza) ---
        bool interactPressed = false;
        // --- Ataque ---
        bool attackPressed = false;

        if (isBlacksTurn) // Jugador Negro (P2 - Flechas, RCtrl, Enter)
        {
            if (Input.GetKeyDown(KeyCode.UpArrow)) moveDirection = Vector2Int.up;
            else if (Input.GetKeyDown(KeyCode.DownArrow)) moveDirection = Vector2Int.down;
            else if (Input.GetKeyDown(KeyCode.LeftArrow)) moveDirection = Vector2Int.left;
            else if (Input.GetKeyDown(KeyCode.RightArrow)) moveDirection = Vector2Int.right;

            if (Input.GetKeyDown(KeyCode.RightControl) || Input.GetKeyDown(KeyCode.RightShift)) interactPressed = true;
            if (Input.GetKeyDown(KeyCode.KeypadEnter) || Input.GetKeyDown(KeyCode.Return)) attackPressed = true;
        }
        else // Jugador Blanco (P1 - WASD, LCtrl, Space)
        {
            if (Input.GetKeyDown(KeyCode.W)) moveDirection = Vector2Int.up;
            else if (Input.GetKeyDown(KeyCode.S)) moveDirection = Vector2Int.down;
            else if (Input.GetKeyDown(KeyCode.A)) moveDirection = Vector2Int.left;
            else if (Input.GetKeyDown(KeyCode.D)) moveDirection = Vector2Int.right;

            if (Input.GetKeyDown(KeyCode.LeftControl) || Input.GetKeyDown(KeyCode.LeftShift)) interactPressed = true;
            if (Input.GetKeyDown(KeyCode.Space)) attackPressed = true;
        }

        // Procesar movimiento
        if (moveDirection != Vector2Int.zero)
        {
            cursorLogic.Move(moveDirection);
            UpdateCursorVisualPosition();
        }

        // Procesar interacción
        if (interactPressed)
        {
            cursorLogic.HandlePieceInteraction(isBlacksTurn);
            // Si se cogió o soltó una pieza, su posición visual es manejada por Piece.UpdatePosition y Piece.Place
        }

        // Procesar ataque
        if (attackPressed)
        {
            TryPerformAttack();
        }
    }

    void UpdateCursorVisualPosition()
    {
        if (cursorVisual == null || board == null || cursorLogic == null) return;

        Square currentCursorSquare = board.GetSquareAtPosition(cursorLogic.CurrentPosition.x, cursorLogic.CurrentPosition.y);
        if (currentCursorSquare != null && currentCursorSquare.Instance != null)
        {
            cursorVisual.UpdatePosition(currentCursorSquare.Instance.transform.position + cursorVisualOffset);
        }
        else
        {
            // Fallback si la casilla no se encuentra (no debería pasar en un tablero bien definido)
            cursorVisual.UpdatePosition(new Vector3(cursorLogic.CurrentPosition.x, cursorVisualOffset.y, cursorLogic.CurrentPosition.y) + cursorVisualOffset);
        }
    }


    void InitializePieces()
    {
        allPieces.Clear();

        // Piezas Blancas (Jugador 1 - no "negras" en el sentido de color de ajedrez, sino P1)
        // Colocadas en las 3 casillas más cercanas, asumiendo que el jugador 1 está "abajo" o "izquierda"
        // Ejemplo: fila 0, columnas 1, 2, 3 (si boardWidth >= 3)
        // O como dice el spec: "starting on each player's 3 closest squares."
        // Asumamos que el jugador blanco (P1) está en la fila 0 (y=0)
        // y el jugador negro (P2) está en la fila boardHeight-1 (y=boardHeight-1)
        // Las 3 piezas centrales de esa fila.
        int midX = boardWidth / 2;
        Vector2Int[] whiteStartPositions = {
            new Vector2Int(midX -1, 0), new Vector2Int(midX, 0), new Vector2Int(midX + 1, 0)
        };
        // Asegurar que las posiciones son válidas (especialmente si el tablero es muy pequeño)
        for (int i = 0; i < 3; i++)
        {
            Vector2Int pos = whiteStartPositions[i];
            if (pos.x < 0 || pos.x >= boardWidth) continue; // Saltar si está fuera de rango

            Square pieceSquare = board.GetSquareAtPosition(pos.x, pos.y);
            if (pieceSquare != null && pieceSquare.Instance != null)
            {
                Vector3 visualPos = pieceSquare.Instance.transform.position + new Vector3(0, YOffsetForPieces, 0);
                GameObject pieceObj = Instantiate(whitePiecePrefab, visualPos, Quaternion.identity);
                PieceVisual pv = pieceObj.GetComponent<PieceVisual>();
                if (pv == null) Debug.LogError($"Prefab {whitePiecePrefab.name} no tiene PieceVisual.");

                Piece piece = new Piece(pos, false, pieceObj, YOffsetForPieces, pv); // false para IsBlack -> pieza del jugador P1/Blanco
                allPieces.Add(piece);
                board.SetEntityAtPosition(pos, piece);
            }
        }

        // Piezas Negras (Jugador 2)
        Vector2Int[] blackStartPositions = {
            new Vector2Int(midX -1, boardHeight -1), new Vector2Int(midX, boardHeight -1), new Vector2Int(midX + 1, boardHeight-1)
        };
        for (int i = 0; i < 3; i++)
        {
            Vector2Int pos = blackStartPositions[i];
            if (pos.x < 0 || pos.x >= boardWidth) continue;

            Square pieceSquare = board.GetSquareAtPosition(pos.x, pos.y);
            if (pieceSquare != null && pieceSquare.Instance != null)
            {
                Vector3 visualPos = pieceSquare.Instance.transform.position + new Vector3(0, YOffsetForPieces, 0);
                GameObject pieceObj = Instantiate(blackPiecePrefab, visualPos, Quaternion.identity);
                PieceVisual pv = pieceObj.GetComponent<PieceVisual>();
                if (pv == null) Debug.LogError($"Prefab {blackPiecePrefab.name} no tiene PieceVisual.");

                Piece piece = new Piece(pos, true, pieceObj, YOffsetForPieces, pv); // true para IsBlack -> pieza del jugador P2/Negro
                allPieces.Add(piece);
                board.SetEntityAtPosition(pos, piece);
            }
        }
    }

    void TryPerformAttack()
    {
        if (cursorLogic.IsHoldingPiece())
        {
            Piece attackingPiece = cursorLogic.GetHeldPiece();
            if (attackingPiece != null && !attackingPiece.IsOnAttackCooldown)
            {
                if (attackingPiece.IsBlack == isBlacksTurn) // Verificar que la pieza es del jugador actual
                {
                    PerformActualAttack(attackingPiece);
                }
                else Debug.Log("No puedes atacar con una pieza del oponente.");
            }
            else if (attackingPiece != null && attackingPiece.IsOnAttackCooldown)
            {
                Debug.Log("La pieza atacante está en cooldown.");
            }
        }
        else Debug.Log("Debes sostener una pieza para atacar.");
    }

    // En GameManager.PerformActualAttack()
    // En GameManager.cs

    void PerformActualAttack(Piece attacker) // attacker es la pieza que realiza el ataque
    {
        Debug.Log($"{(attacker.IsBlack ? "Jugador Negro (P2)" : "Jugador Blanco (P1)")} ataca desde {attacker.Position}");

        // --- DEFINICIONES QUE FALTABAN O ESTABAN IMPLÍCITAS ---
        Vector2Int attackOrigin = attacker.Position; // La posición del atacante es el origen del ataque
        int damage = 1; // Cantidad de daño a aplicar
        Vector2Int[] attackDirections = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };
        // --- FIN DE DEFINICIONES ---

        foreach (Vector2Int dir in attackDirections)
        {
            Vector2Int targetPos = attackOrigin + dir;

            // Importante: Ataque NO es toroidal
            if (targetPos.x >= 0 && targetPos.x < boardWidth &&
                targetPos.y >= 0 && targetPos.y < boardHeight)
            {
                // Usar el nuevo método del tablero para obtener la entidad genérica
                IGameEntity entityOnSquare = board.GetEntityAtPosition(targetPos);

                if (entityOnSquare != null) // Primero verificar que hay *algo*
                {
                    if (entityOnSquare is IAttackable attackableTarget) // Luego, verificar si es atacable
                    {
                        // Opcional: Evitar fuego amigo si la entidad es una pieza
                        // if (entityOnSquare is Piece targetPiece && targetPiece.IsBlack == attacker.IsBlack)
                        // {
                        //     Debug.Log($"Ataque sobre pieza aliada en {targetPos} omitido.");
                        //     continue; // Saltar al siguiente objetivo en la dirección
                        // }

                        Debug.Log($"Atacando entidad en {targetPos}");
                        attackableTarget.TakeDamage(damage); // Llamar a TakeDamage de la interfaz
                    }
                    // else
                    // {
                    //    Debug.Log($"Entidad en {targetPos} no es IAttackable.");
                    // }
                }
            }
        }
        attacker.StartAttackCooldown();
        // Opcional: efecto visual de ataque
    }


    void TickPieceCooldowns()
    {
        float dt = Time.deltaTime;
        for (int i = allPieces.Count - 1; i >= 0; i--) // Iterar hacia atrás si se pueden remover elementos
        {
            Piece piece = allPieces[i];
            if (piece.PieceGameObject.activeInHierarchy)
            {
                piece.TickCooldown(dt);
            }
            else // Si la pieza fue desactivada (ej. por HandleDestruction)
            {
                // Removerla de la lista de piezas activas y del tablero lógico
                if (board.GetEntityAtPosition(piece.Position) == piece) // Verificar que sigue ahí lógicamente
                {
                    board.SetEntityAtPosition(piece.Position, null);
                }
                allPieces.RemoveAt(i);
                Debug.Log($"Pieza {piece.Position} removida de la gestión del GameManager (destruida).");
                CheckWinCondition(); // Comprobar si alguien ganó
            }
        }
    }

    void StartNewTurn()
    {
        timeLeftInTurn = turnTime;
        Debug.Log($"Comienza el turno de {(isBlacksTurn ? "Jugador Negro (P2)" : "Jugador Blanco (P1)")}. Tiempo: {timeLeftInTurn}s");

        // Actualizar visuales del cursor y timer
        if (cursorVisual != null)
        {
            cursorVisual.UpdateMaterial(isBlacksTurn); // Actualiza color del cursor
        }
        UpdateTimerBarColor();

        // La especificación dice: "At the end of the turn, the cursor stays where it was, changes color [...]
        // and only the current player can move it now."
        // Esto ya se maneja por el control de input basado en isBlacksTurn.
        // El cursor visual se actualiza aquí. La posición lógica del cursor la mantiene CursorLogic.
        // UpdateCursorVisualPosition(); // Asegurar que el cursor visual está donde debe
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

    // Esto reemplaza la Coroutine de TurnTimer para un control más directo
    void FixedUpdate() // O Update, pero FixedUpdate es más regular para la lógica de tiempo de juego
    {
        if (timeLeftInTurn > 0)
        {
            timeLeftInTurn -= Time.fixedDeltaTime; // O Time.deltaTime si usas Update
            if (timeLeftInTurn <= 0)
            {
                timeLeftInTurn = 0;
                EndTurn();
            }
        }
    }

    void EndTurn()
    {
        Debug.Log($"Fin del turno para {(isBlacksTurn ? "Jugador Negro (P2)" : "Jugador Blanco (P1)")}.");

        if (cursorLogic != null)
        {
            cursorLogic.ForceDropSelectedPiece(); // Soltar pieza si el jugador tenía una cogida
        }

        isBlacksTurn = !isBlacksTurn; // Cambiar de jugador
        CheckWinCondition(); // Comprobar si el juego terminó antes de empezar el nuevo turno
        if (!IsGameOver()) // Si el juego no ha terminado
        {
            StartNewTurn();
        }
    }

    // --- Lógica de Victoria/Derrota ---
    private bool gameOver = false;
    void CheckWinCondition()
    {
        if (gameOver) return;

        int whitePiecesLeft = 0;
        int blackPiecesLeft = 0;

        foreach (Piece piece in allPieces)
        {
            if (piece.PieceGameObject.activeInHierarchy && piece.CurrentHealth > 0)
            {
                if (piece.IsBlack) blackPiecesLeft++;
                else whitePiecesLeft++;
            }
        }

        Debug.Log($"Piezas restantes: Blancas (P1) = {whitePiecesLeft}, Negras (P2) = {blackPiecesLeft}");

        if (whitePiecesLeft == 0 && blackPiecesLeft > 0) // Asumiendo que se necesitan piezas para seguir jugando
        {
            DeclareWinner(true); // Gana el Negro (P2)
        }
        else if (blackPiecesLeft == 0 && whitePiecesLeft > 0)
        {
            DeclareWinner(false); // Gana el Blanco (P1)
        }
        else if (whitePiecesLeft == 0 && blackPiecesLeft == 0)
        {
            // Empate, o una condición que no debería ocurrir si el juego termina cuando un jugador pierde todas.
            Debug.Log("¡Empate! Ambas armadas destruidas.");
            gameOver = true;
            // Aquí podrías mostrar UI de Empate
        }
    }

    void DeclareWinner(bool blackPlayerWins)
    {
        gameOver = true;
        string winner = blackPlayerWins ? "Jugador Negro (P2 - Flechas)" : "Jugador Blanco (P1 - WASD)";
        Debug.Log($"¡JUEGO TERMINADO! El ganador es: {winner}");
        // Aquí mostrarías la UI de victoria, detendrías el input, etc.
        Time.timeScale = 0; // Pausa simple del juego
    }

    bool IsGameOver()
    {
        return gameOver;
    }

    // Para ser llamado desde Piece.HandleDestruction (o similar)
    public void OnPieceDestroyed(Piece destroyedPiece)
    {
        // La lógica de remover de `allPieces` y del tablero ya está en TickPieceCooldowns
        // al detectar un GameObject inactivo.
        // Aquí solo necesitamos asegurar que se compruebe la condición de victoria.
        Debug.Log($"GameManager notificado de la destrucción de la pieza en {destroyedPiece.Position}");
        // CheckWinCondition(); // Se llamará al final del turno o en TickPieceCooldowns
    }
}