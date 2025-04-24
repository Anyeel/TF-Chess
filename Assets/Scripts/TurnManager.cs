using System.Collections;
using TMPro;
using UnityEngine;

public class TurnManager : MonoBehaviour
{
    [SerializeField] float turnTime = 5f;
    [SerializeField] TMP_Text timer;
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
            timer.color = blackPlayerColor;
        }
        else
        {
            timer.color = whitePlayerColor;
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
                timer.text = timeLeft.ToString("F2");
                yield return null;
            }
            timer.text = "0.00";
        }
    }

    public int RandomPlayer
    {
        get { return playerIndex; }
    }
}

