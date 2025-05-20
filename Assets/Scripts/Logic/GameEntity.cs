using UnityEngine;

public abstract class GameEntity
{
     public virtual GameObject entityGameObject { get; protected set; }
     public virtual Vector2Int position { get; set; }

    protected GameEntity(GameObject entityGameObject, Vector2Int position)
    {
        this.entityGameObject = entityGameObject;
        this.position = position;
    }
}
