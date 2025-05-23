using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthPickUpLogic : GameEntity
{
    public GameObject healthPickUpGameObject { get; private set; }
    public int healAmount { get; private set; } = 1;
    public Type typeHealthPickUp { get; private set; } = Type.HealthPickUp;

    public override Vector2Int position
    {
        get => base.position;
        set { base.position = value; }
    }

    public HealthPickUpLogic(Vector2Int position, GameObject healthPickUpGameObject, int healAmount = 1) : base(healthPickUpGameObject, position)
    {
        this.healthPickUpGameObject = healthPickUpGameObject;
        this.healAmount = healAmount;
    }

    public void TryHealPiece(Piece piece)
    {
        piece.Heal(healAmount);
        healthPickUpGameObject.SetActive(false);
    }

}
