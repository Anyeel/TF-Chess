using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;

public class TurnManager : MonoBehaviour
{
    [SerializeField] Color whitePlayerColor = Color.red;
    [SerializeField] Color blackPlayerColor = Color.blue;
    [SerializeField] float turnTime = 15f;
    [SerializeField] UnityEngine.UI.Image timerBar;
    [SerializeField] bool startWithRandomPlayer = true;
    [SerializeField] GameManager gameManager;
    
    private CursorVisual cursorVisual;

    private float turnTimeTotal;
    private float timeLeftInTurn;
    private bool isBlacksTurnInternal;

    public bool isBlacksTurn => isBlacksTurnInternal;

    void Awake()
    {
        turnTimeTotal = turnTime;
        if (startWithRandomPlayer) isBlacksTurnInternal = Random.Range(0, 2) == 1;
        else isBlacksTurnInternal = false;
    }

    void Start()
    {
        GameObject cursorObject = GameObject.FindWithTag("Cursor");
        cursorVisual = cursorObject.GetComponentInChildren<CursorVisual>();

        StartNewTurn();
    }

    void Update()
    {
        if (timeLeftInTurn > 0)
        {
            timeLeftInTurn -= Time.deltaTime;
            if (timeLeftInTurn <= 0)
            {
                timeLeftInTurn = 0;
                EndTurn();
            }
        }
        UpdateTurnTimerVisuals();
    }

    public void StartNewTurn()
    {
        if(gameManager.IsGameOver()) return;

        timeLeftInTurn = turnTimeTotal;
        cursorVisual.UpdateMaterial(isBlacksTurnInternal);
        UpdateTimerBarColor();
    }

    void UpdateTurnTimerVisuals()
    {
        timerBar.fillAmount = Mathf.Clamp01(timeLeftInTurn / turnTimeTotal);
    }

    void UpdateTimerBarColor()
    {
        if (timerBar != null)
        {
            timerBar.color = isBlacksTurnInternal ? blackPlayerColor : whitePlayerColor;
            timerBar.fillOrigin = isBlacksTurnInternal ? (int)UnityEngine.UI.Image.OriginHorizontal.Right : (int)UnityEngine.UI.Image.OriginHorizontal.Left;
        }
    }

    public void EndTurn()
    {
        if (gameManager.IsGameOver()) return;

        gameManager.ForceDropSelectedPieceOnCurrentPlayer();

        isBlacksTurnInternal = !isBlacksTurnInternal;
        gameManager.CheckWinCondition();

        if (!gameManager.IsGameOver())
        {
            StartNewTurn();
        }
    }
}