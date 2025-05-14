using UnityEngine;

public class Square
{
    public Vector2Int position { get; private set; }
    public GameObject instance { get; private set; }
    public Vector2Int index { get; private set; }
    public IGameEntity containedEntity { get; set; }

    public Square(Vector2Int index, GameObject squarePrefab, Transform parentTransform)
    {
        this.index = index;
        this.position = new Vector2Int(index.x, index.y);
        instance = GameObject.Instantiate(squarePrefab, new Vector3(this.position.x, 0, this.position.y), Quaternion.identity);
        if (parentTransform != null) instance.transform.SetParent(parentTransform);
        instance.name = $"Square_{index.x}_{index.y}";
    }
}
