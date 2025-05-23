using UnityEngine;

public class PieceVisual : MonoBehaviour
{
    [SerializeField] Color cooldownColor;
    [SerializeField] float cooldownTransitionTime = 3f;

    private Vector3 initialScale;
    private Material originalMaterial;
    private Renderer pieceRenderer;
    private bool isInterpolating = false;
    private float cooldownTimer;
    private Color originalColor;

    void Start()
    {
        originalColor = originalMaterial.color;
    }

    void Update()
    {
        InterpolateMaterial();
    }

    void Awake()
    {
        
        initialScale = transform.localScale;

        pieceRenderer = GetComponent<Renderer>();

        pieceRenderer = GetComponentInChildren<Renderer>();

        originalMaterial = pieceRenderer.material;    
    }

    public void UpdateHealthVisual(int currentHealth, int _)
    {
        float baseHeight = initialScale.y;
        float newHeight = baseHeight + (currentHealth - 3) * 0.5f;
        transform.localScale = new Vector3(initialScale.x, Mathf.Max(newHeight, 0.1f), initialScale.z);
    }

    public void UpdateCooldownVisual(bool isOnCooldown)
    {
        if (isOnCooldown)
        {
            isInterpolating = true;
            cooldownTimer = 0f;
        }
    }

    void InterpolateMaterial() 
    {
        if (isInterpolating)
        {
            cooldownTimer += Time.deltaTime;
            float t = Mathf.Clamp01(cooldownTimer / cooldownTransitionTime);

            pieceRenderer.material.color = Color.Lerp(cooldownColor, originalColor, t);

            if (t >= 1f)
            {
                isInterpolating = false;
            }
        }
    }
}