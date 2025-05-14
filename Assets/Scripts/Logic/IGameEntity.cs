using UnityEngine;
public interface IGameEntity
{
    GameObject entityGameObject { get; } 
    Vector2Int position { get; set; } 
}