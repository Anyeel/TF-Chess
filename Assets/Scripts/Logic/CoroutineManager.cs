using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MyCoroutineManager : MonoBehaviour
{
    [SerializeField] GameManager gameManager;


    private void Start()
    {
        //StartCoroutine(gameManager.UpdatePieceCooldowns());
    }
}
