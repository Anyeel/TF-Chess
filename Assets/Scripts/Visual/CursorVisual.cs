using UnityEngine;

public class CursorVisual : MonoBehaviour
{
    [SerializeField] Material blackPlayerMaterial;
    [SerializeField] Material whitePlayerMaterial;

    private MeshRenderer[] cursorWalls;

    void Awake()
    {
        int childCount = transform.childCount;

        cursorWalls = new MeshRenderer[childCount];

        for (int i = 0; i < childCount; i++)
        {
            Transform childTransform = transform.GetChild(i);
            MeshRenderer renderer = childTransform.GetComponent<MeshRenderer>();
               
            cursorWalls[i] = renderer;
        }

    }

    public void UpdatePosition(Vector3 newPosition)
    {
        transform.position = newPosition;
    }

    public void UpdateMaterial(bool isBlackPlayer)
    {
        Material selectedMaterial = isBlackPlayer ? blackPlayerMaterial : whitePlayerMaterial;

        foreach (var wallRenderer in cursorWalls)
        {
            wallRenderer.material = selectedMaterial;
        }
    }
}
