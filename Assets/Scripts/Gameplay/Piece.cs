using UnityEngine;

public class Piece : GameEntity, IAttackable
{
    public bool isBlack { get; private set; }
    public GameObject pieceGameObject { get; private set; }
    public override GameObject entityGameObject => pieceGameObject;
    public override Vector2Int position
    {
        get => base.position;
        set
        {
            base.position = value;
        }
    }
    public int currentHealth { get; private set; }
    public int maxHealth { get; private set; } = 3;
    public bool isOnAttackCooldown { get; private set; }
    private float currentAttackCooldownTimer;
    public float attackCooldownDuration { get; set; } = 2f;
    public PieceVisual visual { get; private set; }

    public Piece(Vector2Int startPosition, bool isBlack, GameObject pieceGameObject)
        : base(pieceGameObject, startPosition)
    {
        this.isBlack = isBlack;
        this.pieceGameObject = pieceGameObject;

        this.visual = pieceGameObject.GetComponent<PieceVisual>();

        currentHealth = maxHealth;
        isOnAttackCooldown = false;
        currentAttackCooldownTimer = 0f;

        visual?.UpdateHealthVisual(currentHealth, maxHealth);
        visual?.UpdateCooldownVisual(isOnAttackCooldown);
    }

    public void UpdatePosition(Vector2Int newLogicalPosition, Board boardReference)
    {
        position = newLogicalPosition;
        if (pieceGameObject != null && boardReference != null)
        {
            Square targetSquare = boardReference.GetSquareAtPosition(newLogicalPosition.x, newLogicalPosition.y);
            if (targetSquare != null && targetSquare.instance != null)
            {
                pieceGameObject.transform.position = targetSquare.instance.transform.position + new Vector3(0, 0, 0);
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
        pieceGameObject.SetActive(false);
    }
}