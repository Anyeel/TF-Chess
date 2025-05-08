using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    [SerializeField] int boardWidth = 5;
    [SerializeField] int boardHeight = 5;
    [SerializeField] GameObject whiteSquarePrefab;
    [SerializeField] GameObject blackSquarePrefab;
    [SerializeField] GameObject cursorPrefab;
    [SerializeField] float turnTime = 5f;
    [SerializeField] Image timerBar;

    [SerializeField] Color whitePlayerColor = Color.red;
    [SerializeField] Color blackPlayerColor = Color.blue;

    [SerializeField] GameObject whitePiecePrefab;
    [SerializeField] GameObject blackPiecePrefab;

    private Board board; // Asegúrate que esta clase tiene GetSquareAtPosition que devuelve un Square con una propiedad Instance (GameObject)
    private CursorLogic cursorLogic;
    private CursorVisual cursorVisual;

    private float timeLeft;
    private bool isBlackPlayerTurn;
    private Vector3 offset = new Vector3(-0.5f, 0, 0); // Offset para el cursor visual
    private float YOffset = 0.5f; // YOffset para las piezas

    void Start()
    {
        board = new Board(boardWidth, boardHeight, whiteSquarePrefab, blackSquarePrefab);

        Vector2Int startPosition = new Vector2Int(2, 2);
        // Modificación: Pasar la referencia 'board' a CursorLogic
        cursorLogic = new CursorLogic(boardWidth, boardHeight, startPosition, board);

        // Asumimos que board.GetSquareAtPosition(2,2) devuelve algo con .Instance.transform.position
        Square startSquareForCursor = board.GetSquareAtPosition(startPosition.x, startPosition.y);
        if (startSquareForCursor != null && startSquareForCursor.Instance != null)
        {
            GameObject cursorObject = Instantiate(cursorPrefab, startSquareForCursor.Instance.transform.position + offset, Quaternion.identity);
            cursorVisual = cursorObject.GetComponent<CursorVisual>();
        }
        else
        {
            Debug.LogError("No se pudo obtener la casilla inicial para el cursor o su instancia es null.");
            // Considera una posición por defecto o manejar el error
            GameObject cursorObject = Instantiate(cursorPrefab, new Vector3(startPosition.x, 0, startPosition.y) + offset, Quaternion.identity);
            cursorVisual = cursorObject.GetComponent<CursorVisual>();
        }


        isBlackPlayerTurn = Random.Range(0, 2) == 0;

        StartCoroutine(DelayedUpdateMaterial());
        UpdateTimerColor();
        StartCoroutine(TurnTimer());

        InitializePieces();
    }

    IEnumerator DelayedUpdateMaterial()
    {
        yield return new WaitForEndOfFrame();
        if (cursorVisual != null) // Añadir comprobación por si falla la instanciación
        {
            cursorVisual.UpdateMaterial(isBlackPlayerTurn);
        }
    }

    void Update()
    {
        HandleCursorMovement();
    }

    void InitializePieces()
    {
        // Piezas blancas
        for (int i = 0; i < 3; i++)
        {
            Vector2Int position = new Vector2Int(0, i + 1);
            Square pieceSquare = board.GetSquareAtPosition(position.x, position.y);
            if (pieceSquare != null && pieceSquare.Instance != null)
            {
                Vector3 visualPosition = pieceSquare.Instance.transform.position + new Vector3(0, YOffset, 0);
                GameObject whitePieceObject = Instantiate(whitePiecePrefab, visualPosition, Quaternion.identity);
                // Modificación: Pasar el GameObject Y el YOffset al constructor de Piece
                Piece whitePiece = new Piece(position, false, whitePieceObject, YOffset);
                cursorLogic.AddPiece(whitePiece);
            }
            else
            {
                Debug.LogError($"No se pudo obtener la casilla para la pieza blanca en {position} o su instancia es null.");
            }
        }

        // Piezas negras
        for (int i = 0; i < 3; i++)
        {
            Vector2Int position = new Vector2Int(boardWidth - 1, i + 1);
            Square pieceSquare = board.GetSquareAtPosition(position.x, position.y);
            if (pieceSquare != null && pieceSquare.Instance != null)
            {
                Vector3 visualPosition = pieceSquare.Instance.transform.position + new Vector3(0, YOffset, 0);
                GameObject blackPieceObject = Instantiate(blackPiecePrefab, visualPosition, Quaternion.identity);
                // Modificación: Pasar el GameObject Y el YOffset al constructor de Piece
                Piece blackPiece = new Piece(position, true, blackPieceObject, YOffset);
                cursorLogic.AddPiece(blackPiece);
            }
            else
            {
                Debug.LogError($"No se pudo obtener la casilla para la pieza negra en {position} o su instancia es null.");
            }
        }
    }

    void HandleCursorMovement()
    {
        Vector2Int direction = Vector2Int.zero;

        // Lógica de input (sin cambios)
        if (isBlackPlayerTurn)
        {
            if (Input.GetKeyDown(KeyCode.UpArrow)) direction = Vector2Int.up;
            if (Input.GetKeyDown(KeyCode.DownArrow)) direction = Vector2Int.down;
            if (Input.GetKeyDown(KeyCode.LeftArrow)) direction = Vector2Int.left;
            if (Input.GetKeyDown(KeyCode.RightArrow)) direction = Vector2Int.right;
        }
        else
        {
            if (Input.GetKeyDown(KeyCode.W)) direction = Vector2Int.up;
            if (Input.GetKeyDown(KeyCode.S)) direction = Vector2Int.down;
            if (Input.GetKeyDown(KeyCode.A)) direction = Vector2Int.left;
            if (Input.GetKeyDown(KeyCode.D)) direction = Vector2Int.right;
        }

        if (direction != Vector2Int.zero)
        {
            cursorLogic.Move(direction); // Lógica de movimiento y pieza

            // Actualizar posición visual del cursor
            Square targetSquare = board.GetSquareAtPosition(cursorLogic.CurrentPosition.x, cursorLogic.CurrentPosition.y);
            if (targetSquare != null && targetSquare.Instance != null && cursorVisual != null)
            {
                Vector3 newPosition = targetSquare.Instance.transform.position + offset;
                cursorVisual.UpdatePosition(newPosition);
            }
            else if (cursorVisual != null) // Si no se encuentra la casilla, al menos mover el cursor a la posición lógica
            {
                Debug.LogWarning($"Casilla en {cursorLogic.CurrentPosition} no encontrada para el cursor, usando posición lógica.");
                cursorVisual.UpdatePosition(new Vector3(cursorLogic.CurrentPosition.x, 0, cursorLogic.CurrentPosition.y) + offset);
            }
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            cursorLogic.HandlePieceInteraction(isBlackPlayerTurn);
            // No necesitas actualizar la posición de la pieza aquí, HandlePieceInteraction
            // y Move (si la pieza se mueve) ya se encargan de llamar a los métodos de la pieza
            // que actualizan su GameObject.
        }
    }

    IEnumerator TurnTimer()
    {
        while (true)
        {
            timeLeft = turnTime;
            while (timeLeft > 0)
            {
                timeLeft -= Time.deltaTime;
                timerBar.fillAmount = timeLeft / turnTime;
                yield return null;
            }

            // El tiempo se acabó para el jugador actual

            // --- Acciones de Fin de Turno ---

            // 1. Forzar que se suelte cualquier pieza seleccionada por el jugador cuyo turno acaba de terminar.
            if (cursorLogic != null) // Buena práctica: comprobar si cursorLogic existe
            {
                cursorLogic.ForceDropSelectedPiece();
            }

            // 2. Cambiar el turno
            isBlackPlayerTurn = !isBlackPlayerTurn;

            // 3. Actualizar los elementos visuales para el nuevo turno
            UpdateTimerColor();
            if (cursorVisual != null)
            {
                cursorVisual.UpdateMaterial(isBlackPlayerTurn);
            }
        }
    }

    void UpdateTimerColor()
    {
        timerBar.color = isBlackPlayerTurn ? blackPlayerColor : whitePlayerColor;
        timerBar.fillOrigin = isBlackPlayerTurn ? (int)Image.OriginHorizontal.Right : (int)Image.OriginHorizontal.Left;
    }
}
