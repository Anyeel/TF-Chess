using UnityEngine;

public class PieceVisual : MonoBehaviour
{
    [SerializeField] private Transform modelPartToScaleForHealth;

    [SerializeField] private Material cooldownMaterial;

    private Vector3 initialScale;
    private Material originalMaterial;
    private Renderer pieceRenderer;

    void Awake()
    {
        modelPartToScaleForHealth = transform;
        initialScale = modelPartToScaleForHealth.localScale;

        pieceRenderer = GetComponent<Renderer>();
        originalMaterial = pieceRenderer.material; 
    }

    public void UpdateHealthVisual(int currentHealth, int maxHealth)
    {
        if (modelPartToScaleForHealth == null) return;
        if (maxHealth <= 0) return;

        modelPartToScaleForHealth.localScale = new Vector3(initialScale.x, initialScale.y * ((float)currentHealth / maxHealth), initialScale.z);
    }

    public void UpdateCooldownVisual(bool isOnCooldown)
    {
        if (isOnCooldown && cooldownMaterial != null)
        {
            pieceRenderer.material = cooldownMaterial;
        }
        else
        {
            pieceRenderer.material = originalMaterial;
        }
    }
}
