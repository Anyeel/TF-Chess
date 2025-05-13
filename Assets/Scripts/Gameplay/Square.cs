using UnityEngine;

public class Square
{
    public Vector2Int Position { get; private set; }
    public GameObject Instance { get; private set; }
    public Vector2Int Index { get; private set; }
    public IGameEntity ContainedEntity { get; set; }

    public Square(Vector2Int index, GameObject squarePrefab, Transform parentTransform)
    {
        this.Index = index;
        this.Position = new Vector2Int(index.x, index.y);
        Instance = GameObject.Instantiate(squarePrefab, new Vector3(this.Position.x, 0, this.Position.y), Quaternion.identity);
        if (parentTransform != null) Instance.transform.SetParent(parentTransform);
        Instance.name = $"Square_{index.x}_{index.y}";
    }
}
