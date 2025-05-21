using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CrateLogic : GameEntity, IAttackable
{
    public GameObject healthPickUp;
    public GameObject crateGameObject { get; private set; }
    public int currentHealth { get; private set; }
    public int maxHealth { get; private set; } = 1;
    public PieceVisual visual { get; private set; }
    public Type typeCrate { get; private set; }
    public override Vector2Int position
    {
        get => base.position;
        set { base.position = value; }
    }

    public CrateLogic(Vector2Int position, GameObject crateGameObject, Type typeCrate, GameObject healthPickUp) : base(crateGameObject, position)
    {
        this.crateGameObject = crateGameObject;
        this.currentHealth = maxHealth;
        this.typeCrate = typeCrate;
        this.healthPickUp = healthPickUp;

        visual?.UpdateHealthVisual(currentHealth, maxHealth);
    }

    public void TakeDamage(int amount)
    {
        currentHealth -= amount;
        if (currentHealth < 0) currentHealth = 0;

        visual?.UpdateHealthVisual(currentHealth, maxHealth);

        if (currentHealth <= 0)
        {
            HandleDestruction();
        }
    }

    private void HandleDestruction()
    {
        GameManager gameManager = GameObject.FindObjectOfType<GameManager>();

        gameManager.SpawnHealthPickup(crateGameObject.transform.position, healthPickUp);

        crateGameObject.SetActive(false);
    }

}

