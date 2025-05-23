using UnityEngine;

public class Piece : GameEntity, IAttackable
{
    public bool isBlack { get; private set; }
    public GameObject pieceGameObject { get; private set; }
    public override GameObject entityGameObject => pieceGameObject;
    public override Vector2Int position
    {
        get => base.position;
        set { base.position = value; }
    }
    public int currentHealth { get; private set; }
    public int maxHealth { get; private set; } = 3;
    public bool isOnAttackCooldown { get; private set; }
    public float attackCooldownDuration { get; set; } = 4f;
    public PieceVisual visual { get; private set; }
    public Type pieceType { get; private set; }

    public Piece(Vector2Int startPosition, Type pieceType, GameObject pieceGameObject)
        : base(pieceGameObject, startPosition)
    {
        this.pieceType = pieceType;
        this.pieceGameObject = pieceGameObject;
        this.visual = pieceGameObject.GetComponent<PieceVisual>();
        currentHealth = maxHealth;
        isOnAttackCooldown = false;
        isBlack = pieceType == Type.BlackPiece;
        visual?.UpdateHealthVisual(currentHealth, maxHealth);
        visual?.UpdateCooldownVisual(isOnAttackCooldown);
    }

    public void StartAttackCooldown(MyCoroutineManager coroutineManager)
    {
        if (!isOnAttackCooldown)
        {
            coroutineManager.StartPieceCooldown(this, attackCooldownDuration);
        }
    }

    public void BeginAttackCooldown()
    {
        isOnAttackCooldown = true;
        visual?.UpdateCooldownVisual(true);
    }

    public void EndAttackCooldown()
    {
        isOnAttackCooldown = false;
        visual?.UpdateCooldownVisual(false);
    }

    public void UpdatePosition(Vector2Int newLogicalPosition, Board boardReference)
    {
        position = newLogicalPosition;

        Square targetSquare = boardReference.GetSquareAtPosition(newLogicalPosition.x, newLogicalPosition.y);
        if (targetSquare != null && targetSquare.instance != null)
        {
            pieceGameObject.transform.position = targetSquare.instance.transform.position + new Vector3(0, 0, 0);
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
        visual.UpdateHealthVisual(currentHealth, currentHealth);
    }

    private void HandleDestruction()
    {
        pieceGameObject.SetActive(false);
    }

    public void StartAttackCooldown(MyCoroutineManager coroutineManager, float customDuration)
    {
        if (!isOnAttackCooldown)
        {
            coroutineManager.StartPieceCooldown(this, customDuration);
        }
    }

}
