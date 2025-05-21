using UnityEngine;

public class CrateVisual : MonoBehaviour
{
    [SerializeField] float rotationSpeed = 15f;
    [SerializeField] float amplitude = 0.2f;
    [SerializeField] float speed = 2f;

    private Vector3 startPosition;

    void Start()
    {
        startPosition = transform.position;
    }

    void Update()
    {
        RotateCrate();
        FloatCrate();
    }

    void RotateCrate()
    {
        transform.Rotate(Vector3.up * rotationSpeed * Time.deltaTime);
    }

    void FloatCrate()
    {
        float newY = startPosition.y + Mathf.Sin(Time.time * speed) * amplitude;
        transform.position = new Vector3(transform.position.x, newY, transform.position.z);
    }
}

