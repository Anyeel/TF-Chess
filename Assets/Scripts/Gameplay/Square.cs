using UnityEngine;

public class Square
{
    public Vector2Int Position { get; set; }
    public GameObject Instance { get; set; }
    public Vector2Int Index { get; set; } // Índice en formato Vector2Int

    public Square(Vector2Int position, GameObject prefab, Vector2Int index)
    {
        Position = position;
        Index = index;

        // Instanciar el prefab en la posición correspondiente
        Instance = GameObject.Instantiate(prefab, new Vector3(position.x, 0, position.y), Quaternion.identity);
    }
}


