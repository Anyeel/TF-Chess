using UnityEngine;

public class CursorVisual : MonoBehaviour
{
    [SerializeField] Material blackPlayerMaterial;
    [SerializeField] Material whitePlayerMaterial;

    private MeshRenderer[] cursorWalls;

    void Awake()
    {
        int childCount = transform.childCount;
        if (childCount == 0)
        {
            Debug.LogWarning($"CursorVisual ({gameObject.name}): No tiene GameObjects hijos. No se configurarán los muros del cursor.", this);
            cursorWalls = new MeshRenderer[0];
            return;
        }

        cursorWalls = new MeshRenderer[childCount];
        bool allRenderersFound = true;

        for (int i = 0; i < childCount; i++)
        {
            Transform childTransform = transform.GetChild(i);
            MeshRenderer renderer = childTransform.GetComponent<MeshRenderer>();

            if (renderer != null)
            {
                cursorWalls[i] = renderer;
            }
            else
            {
                Debug.LogError($"CursorVisual ({gameObject.name}): El hijo '{childTransform.name}' no tiene un componente MeshRenderer.", childTransform);
                cursorWalls[i] = null;
                allRenderersFound = false;
            }
        }

        if (!allRenderersFound)
        {
            Debug.LogError($"CursorVisual ({gameObject.name}): No todos los hijos tenían MeshRenderer.", this);
        }
    }

    public void UpdatePosition(Vector3 newPosition)
    {
        transform.position = newPosition;
    }

    public void UpdateMaterial(bool isBlackPlayer)
    {
        Material selectedMaterial = isBlackPlayer ? blackPlayerMaterial : whitePlayerMaterial;

        if (selectedMaterial == null)
        {
            Debug.LogError($"CursorVisual ({gameObject.name}): El material seleccionado es nulo. ¿Has asignado blackPlayerMaterial y whitePlayerMaterial en el Inspector?", this);
            return;
        }

        if (cursorWalls == null)
        {
            Debug.LogError($"CursorVisual ({gameObject.name}): cursorWalls no está inicializado. ¿Se ejecutó Start correctamente?", this);
            return;
        }

        foreach (var wallRenderer in cursorWalls) // Cambiado el nombre de la variable para claridad
        {
            if (wallRenderer != null) // IMPORTANTE: Saltar si el renderer es nulo
            {
                wallRenderer.material = selectedMaterial;
            }
        }
    }
}
