using UnityEngine;
public interface IGameEntity
{
    GameObject EntityGameObject { get; } 
    Vector2Int Position { get; set; } 
}