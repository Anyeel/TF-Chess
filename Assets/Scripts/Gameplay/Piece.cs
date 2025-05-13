using UnityEngine;

public class Piece : IAttackable, IGameEntity
{
    public bool IsBlack { get; private set; }
    public GameObject PieceGameObject { get; private set; }
    public GameObject EntityGameObject => PieceGameObject;
    public Vector2Int Position { get; set; }
    public int CurrentHealth { get; private set; }
    public int MaxHealth { get; private set; } = 3; 
    public bool IsOnAttackCooldown { get; private set; }
    private float currentAttackCooldownTimer;
    public float AttackCooldownDuration { get; set; } = 2f; 
    public PieceVisual Visual { get; private set; } 
    public float yOffsetOnBoard;

    public Piece(Vector2Int startPosition, bool isBlack, GameObject pieceGameObject, float yOffset, PieceVisual visualComponent)
    {
        Position = startPosition;
        IsBlack = isBlack;
        PieceGameObject = pieceGameObject;
        this.yOffsetOnBoard = yOffset;
        this.Visual = visualComponent;

        CurrentHealth = MaxHealth;
        IsOnAttackCooldown = false;
        currentAttackCooldownTimer = 0f;

        Visual?.UpdateHealthVisual(CurrentHealth, MaxHealth);
        Visual?.UpdateCooldownVisual(IsOnAttackCooldown);
    }

    public void UpdatePosition(Vector2Int newLogicalPosition, Board boardReference)
    {
        Position = newLogicalPosition;
        if (PieceGameObject != null && boardReference != null)
        {
            Square targetSquare = boardReference.GetSquareAtPosition(newLogicalPosition.x, newLogicalPosition.y);
            if (targetSquare != null && targetSquare.Instance != null)
            {
                PieceGameObject.transform.position = targetSquare.Instance.transform.position + new Vector3(0, yOffsetOnBoard, 0);
            }
        }
    }

    public void TakeDamage(int amount)
    {
        CurrentHealth -= amount;
        if (CurrentHealth < 0) CurrentHealth = 0;

        Visual?.UpdateHealthVisual(CurrentHealth, MaxHealth);

        if (CurrentHealth <= 0)
        {
            HandleDestruction();
        }
    }

    public void Heal(int amount)
    {
        CurrentHealth += amount;
        if (CurrentHealth > MaxHealth) CurrentHealth = MaxHealth;

        Visual?.UpdateHealthVisual(CurrentHealth, MaxHealth);
    }

    public void StartAttackCooldown()
    {
        if (!IsOnAttackCooldown)
        {
            IsOnAttackCooldown = true;
            currentAttackCooldownTimer = AttackCooldownDuration;
            Visual?.UpdateCooldownVisual(true);
        }
    }

    public void TickCooldown(float deltaTime)
    {
        if (IsOnAttackCooldown)
        {
            currentAttackCooldownTimer -= deltaTime;
            if (currentAttackCooldownTimer <= 0)
            {
                IsOnAttackCooldown = false;
                currentAttackCooldownTimer = 0;
                Visual?.UpdateCooldownVisual(false);
            }
        }
    }

    private void HandleDestruction()
    {
        if (PieceGameObject != null)
        {
            PieceGameObject.SetActive(false); 
        }

        GameManager gm = Object.FindObjectOfType<GameManager>(); 
        if (gm != null)
        {
            gm.OnPieceDestroyed(this);
        }
    }
}