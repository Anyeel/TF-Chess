using UnityEngine;

public class Piece : IAttackable, IGameEntity
{
    public bool isBlack { get; private set; }
    public GameObject pieceGameObject { get; private set; }
    public GameObject entityGameObject => pieceGameObject;
    public Vector2Int position { get; set; }
    public int currentHealth { get; private set; }
    public int maxHealth { get; private set; } = 3; 
    public bool isOnAttackCooldown { get; private set; }
    private float currentAttackCooldownTimer;
    public float attackCooldownDuration { get; set; } = 2f; 
    public PieceVisual visual { get; private set; } 
    public float yOffsetOnBoard;

    public Piece(Vector2Int startPosition, bool isBlack, GameObject pieceGameObject, float yOffset, PieceVisual visualComponent)
    {
        position = startPosition;
        this.isBlack = isBlack;
        this.pieceGameObject = pieceGameObject;
        this.yOffsetOnBoard = yOffset;
        this.visual = visualComponent;

        currentHealth = maxHealth;
        isOnAttackCooldown = false;
        currentAttackCooldownTimer = 0f;

        visual?.UpdateHealthVisual(currentHealth, maxHealth);
        visual?.UpdateCooldownVisual(isOnAttackCooldown);

        //GameObject.FindAnyObjectByType<CoroutineManager>().StartCoroutine
    }

    public void UpdatePosition(Vector2Int newLogicalPosition, Board boardReference)
    {
        position = newLogicalPosition;
        if (pieceGameObject != null && boardReference != null)
        {
            Square targetSquare = boardReference.GetSquareAtPosition(newLogicalPosition.x, newLogicalPosition.y);
            if (targetSquare != null && targetSquare.instance != null)
            {
                pieceGameObject.transform.position = targetSquare.instance.transform.position + new Vector3(0, yOffsetOnBoard, 0);
            }
        }
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

    public void Heal(int amount)
    {
        currentHealth += amount;
        if (currentHealth > maxHealth) currentHealth = maxHealth;

        visual?.UpdateHealthVisual(currentHealth, maxHealth);
    }

    public void StartAttackCooldown()
    {
        if (!isOnAttackCooldown)
        {
            isOnAttackCooldown = true;
            currentAttackCooldownTimer = attackCooldownDuration;
            visual?.UpdateCooldownVisual(true);
        }
    }

    public void Cooldown(float deltaTime)
    {
        if (isOnAttackCooldown)
        {
            currentAttackCooldownTimer -= deltaTime;
            if (currentAttackCooldownTimer <= 0)
            {
                isOnAttackCooldown = false;
                currentAttackCooldownTimer = 0;
                visual?.UpdateCooldownVisual(false);
            }
        }
    }

    private void HandleDestruction()
    {
        if (pieceGameObject != null)
        {
            pieceGameObject.SetActive(false); 
        }
    }
}