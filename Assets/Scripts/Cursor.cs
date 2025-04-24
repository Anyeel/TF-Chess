using UnityEngine;

public class Cursor : MonoBehaviour
{
    [SerializeField] GameObject[] boardSquare;
    [SerializeField] GameObject centerBoardSquare;

    [SerializeField] Material blackPlayer;
    [SerializeField] Material whitePlayer;

    MeshRenderer[] cursorWalls;
    private Vector3 offset = new Vector3(-0.5f, 0, 0);
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

        currentSquareIndex = System.Array.IndexOf(boardSquare, centerBoardSquare);
        transform.position = boardSquare[currentSquareIndex].transform.position + offset;
    }

    void Update()
    {
        Movement();
        ChangeMaterial();
    }

    void ChangeMaterial()
    {
        Material selectedMaterial = (turnManager.RandomPlayer == 0) ? blackPlayer : whitePlayer;

        if (turnManager.RandomPlayer == 0)
        {
            selectedMaterial = blackPlayer;
        }
        else
        {
            selectedMaterial = whitePlayer;
        }

        for (int i = 0; i < cursorWalls.Length; i++)
        {
            cursorWalls[i].material = selectedMaterial;
        }
    }

    void Movement()
    {
        int boardWidth = Mathf.RoundToInt(Mathf.Sqrt(boardSquare.Length));

        if (turnManager.RandomPlayer == 0)
        {
            if (Input.GetKeyDown(KeyCode.W))
            {
                currentSquareIndex = (currentSquareIndex - boardWidth + boardSquare.Length) % boardSquare.Length;
            }
            else if (Input.GetKeyDown(KeyCode.S))
            {
                currentSquareIndex = (currentSquareIndex + boardWidth) % boardSquare.Length;
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
                currentSquareIndex = (currentSquareIndex - boardWidth + boardSquare.Length) % boardSquare.Length;
            }
            else if (Input.GetKeyDown(KeyCode.DownArrow))
            {
                currentSquareIndex = (currentSquareIndex + boardWidth) % boardSquare.Length;
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

        transform.position = boardSquare[currentSquareIndex].transform.position + offset;
    }
}
