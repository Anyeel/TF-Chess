using UnityEngine;

public class Square
{
    public Vector2Int Position { get; set; }
    public GameObject Instance { get; set; }
    public Vector2Int Index { get; set; } 

    public Square(Vector2Int position, GameObject prefab, Vector2Int index)
    {
        Position = position;
        Index = index;

        Instance = GameObject.Instantiate(prefab, new Vector3(position.x, 0, position.y), Quaternion.identity);
    }
}


