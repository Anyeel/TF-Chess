using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Cursor : MonoBehaviour
{
    [SerializeField] GameObject[] square;
    [SerializeField] Material blackPlayer;
    [SerializeField] Material whitePlayer;

    MeshRenderer[] cursorWalls;
    private Vector3 offset = new Vector3(-0.5f, 0, 0);

    void Start()
    {
        cursorWalls = new MeshRenderer[transform.childCount];
        for (int i = 0; i < transform.childCount; i++)
        {
            cursorWalls[i] = transform.GetChild(i).GetComponent<MeshRenderer>();
        }

        transform.position = square[12].transform.position + offset;
        ChooseRandomPlayer();
    }

    void Update()
    {

    }

    void ChooseRandomPlayer()
    {
        int randomPlayer = Random.Range(0, 2);
        Material selectedMaterial = (randomPlayer == 0) ? blackPlayer : whitePlayer;

        for (int i = 0; i < cursorWalls.Length; i++)
        {
            cursorWalls[i].material = selectedMaterial;
        }
    }
}
