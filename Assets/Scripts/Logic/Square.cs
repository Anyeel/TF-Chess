using UnityEngine;

public class Square
{
    public Vector2Int position { get; private set; }
    public GameObject instance { get; private set; }
    public GameEntity containedEntity { get; set; }

    public Square(Vector2Int index, GameObject squarePrefab, Transform parentTransform)
    {
        position = index;
        instance = Object.Instantiate(squarePrefab, parentTransform);
        instance.transform.position = new Vector3(index.x, 0, index.y);
        containedEntity = null;
    }
}

