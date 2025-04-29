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

    private Board board;
    private CursorLogic cursorLogic;
    private CursorVisual cursorVisual;

    private float timeLeft;
    private bool isBlackPlayerTurn;
    private Vector3 offset = new Vector3(-0.5f, 0, 0);

    void Start()
    {
        board = new Board(boardWidth, boardHeight, whiteSquarePrefab, blackSquarePrefab);

        // Inicializar el cursor lógico
        Vector2Int startPosition = new Vector2Int(2, 2);
        cursorLogic = new CursorLogic(boardWidth, boardHeight, startPosition);

        // Inicializar el cursor visual
        GameObject cursorObject = Instantiate(cursorPrefab, board.GetSquareAtPosition(2, 2).Instance.transform.position + offset, Quaternion.identity);
        cursorVisual = cursorObject.GetComponent<CursorVisual>();

        // Inicializar turnos
        isBlackPlayerTurn = Random.Range(0, 2) == 0;

        // Actualizar el color del cursor inmediatamente
        //cursorVisual.UpdateMaterial(isBlackPlayerTurn);
        StartCoroutine(DelayedUpdateMaterial());

        // Actualizar el color del temporizador
        UpdateTimerColor();

        // Iniciar el temporizador de turnos
        StartCoroutine(TurnTimer());
    }

    IEnumerator DelayedUpdateMaterial()
    {
        yield return new WaitForEndOfFrame();
        cursorVisual.UpdateMaterial(isBlackPlayerTurn);
    }


    void Update()
    {
        HandleCursorMovement();
    }

    void HandleCursorMovement()
    {
        Vector2Int direction = Vector2Int.zero;

        // Detectar las teclas de movimiento según el turno
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

        // Si se detecta una dirección, mover el cursor
        if (direction != Vector2Int.zero)
        {
            cursorLogic.Move(direction);

            // Obtener la nueva posición del cursor en el tablero
            Square targetSquare = board.GetSquareAtPosition(cursorLogic.CurrentPosition.x, cursorLogic.CurrentPosition.y);

            // Actualizar la posición visual del cursor
            Vector3 newPosition = targetSquare.Instance.transform.position + offset;
            cursorVisual.UpdatePosition(newPosition);
            
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

            // Cambiar de turno
            isBlackPlayerTurn = !isBlackPlayerTurn;
            UpdateTimerColor();
            cursorVisual.UpdateMaterial(isBlackPlayerTurn);
        }
    }

    void UpdateTimerColor()
    {
        // Cambiar el color de la barra según el turno
        timerBar.color = isBlackPlayerTurn ? blackPlayerColor : whitePlayerColor;

        // Cambiar la dirección de llenado de la barra
        timerBar.fillOrigin = isBlackPlayerTurn ? (int)Image.OriginHorizontal.Right : (int)Image.OriginHorizontal.Left;
    }

}
