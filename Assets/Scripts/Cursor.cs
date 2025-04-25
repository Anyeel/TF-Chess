using UnityEngine;

public class Cursor : MonoBehaviour
{
    //Parte visual si monobehaviuor, parte de lógica no

    [SerializeField] Vector2Int[] boardPositions;
    [SerializeField] Vector2 centerPosition;

    [SerializeField] int boardWidth = 0;
    [SerializeField] int boardHeight = 0;

    [SerializeField] Material blackPlayer;
    [SerializeField] Material whitePlayer;

    MeshRenderer[] cursorWalls;
    private int currentSquareIndex;

    TurnManager turnManager;

    void Start()
    {
        cursorWalls = new MeshRenderer[transform.childCount];
        for (int i = 0; i < transform.childCount; i++)
        {
            cursorWalls[i] = transform.GetChild(i).GetComponent<MeshRenderer>();
        }

        turnManager = FindObjectOfType<TurnManager>();

        currentSquareIndex = System.Array.IndexOf(boardPositions, centerPosition);
        //transform.position = boardPositions[currentSquareIndex];
    }

    void Update()
    {
        Movement();
        ChangeMaterial();
    }

    void ChangeMaterial()
    {
        Material selectedMaterial = (turnManager.RandomPlayer == 0) ? blackPlayer : whitePlayer;

        for (int i = 0; i < cursorWalls.Length; i++)
        {
            cursorWalls[i].material = selectedMaterial;
        }
    }

    void Movement()
    {
        int boardWidth = Mathf.RoundToInt(Mathf.Sqrt(boardPositions.Length));

        if (turnManager.RandomPlayer == 0)
        {
            if (Input.GetKeyDown(KeyCode.W))
            {
                currentSquareIndex = (currentSquareIndex - boardWidth + boardPositions.Length) % boardPositions.Length;
            }
            else if (Input.GetKeyDown(KeyCode.S))
            {
                currentSquareIndex = (currentSquareIndex + boardWidth) % boardPositions.Length;
            }
            else if (Input.GetKeyDown(KeyCode.A))
            {
                int rowStart = (currentSquareIndex / boardWidth) * boardWidth;
                int rowEnd = rowStart + boardWidth - 1;

                if (currentSquareIndex == rowStart)
                {
                    currentSquareIndex = rowEnd;
                }
                else
                {
                    currentSquareIndex -= 1;
                }
            }
            else if (Input.GetKeyDown(KeyCode.D))
            {
                int rowStart = (currentSquareIndex / boardWidth) * boardWidth;
                int rowEnd = rowStart + boardWidth - 1;

                if (currentSquareIndex == rowEnd)
                {
                    currentSquareIndex = rowStart;
                }
                else
                {
                    currentSquareIndex += 1;
                }
            }
        }
        else
        {
            if (Input.GetKeyDown(KeyCode.UpArrow))
            {
                currentSquareIndex = (currentSquareIndex - boardWidth + boardPositions.Length) % boardPositions.Length;
            }
            else if (Input.GetKeyDown(KeyCode.DownArrow))
            {
                currentSquareIndex = (currentSquareIndex + boardWidth) % boardPositions.Length;
            }
            else if (Input.GetKeyDown(KeyCode.LeftArrow))
            {
                int rowStart = (currentSquareIndex / boardWidth) * boardWidth;
                int rowEnd = rowStart + boardWidth - 1;

                if (currentSquareIndex == rowStart)
                {
                    currentSquareIndex = rowEnd;
                }
                else
                {
                    currentSquareIndex -= 1;
                }
            }
            else if (Input.GetKeyDown(KeyCode.RightArrow))
            {
                int rowStart = (currentSquareIndex / boardWidth) * boardWidth;
                int rowEnd = rowStart + boardWidth - 1;

                if (currentSquareIndex == rowEnd)
                {
                    currentSquareIndex = rowStart;
                }
                else
                {
                    currentSquareIndex += 1;
                }
            }
        }

        //transform.position = boardPositions[currentSquareIndex];
    }
}
