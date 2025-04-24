using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class TurnManager : MonoBehaviour
{
    [SerializeField] float turnTime = 5f;
    [SerializeField] Image timerBar;
    [SerializeField] Color whitePlayerColor = Color.red;
    [SerializeField] Color blackPlayerColor = Color.blue;

    private float timeLeft = 0f;
    private int playerIndex = 0;

    void Start()
    {
        ChooseRandomPlayer();
        UpdateTimerColor();
        StartCoroutine(ChangePlayer());
        StartCoroutine(Timer());
    }

    void Update()
    {

    }

    void ChooseRandomPlayer()
    {
        playerIndex = Random.Range(0, 2);
    }

    IEnumerator ChangePlayer()
    {
        while (true)
        {
            yield return new WaitForSeconds(turnTime);

            playerIndex = (playerIndex == 0) ? 1 : 0;

            UpdateTimerColor();
        }
    }

    void UpdateTimerColor()
    {
        if (playerIndex == 1)
        {
            timerBar.color = blackPlayerColor;
            timerBar.fillOrigin = (int)Image.OriginHorizontal.Right;
        }
        else
        {
            timerBar.color = whitePlayerColor;
            timerBar.fillOrigin = (int)Image.OriginHorizontal.Left;
        }
    }

    IEnumerator Timer()
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
            timerBar.fillAmount = 0;
        }
    }

    public int RandomPlayer
    {
        get { return playerIndex; }
    }
}
