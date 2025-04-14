using UnityEngine;

public class RotateAround : MonoBehaviour
{
    public float speed = 10f; // Speed of rotation
    public float radius = 5f; // Distance from the center of the circular path

    private float angle = 0f;
    private Vector3 centerPoint;

    void Start()
    {
        // Store the initial position as the center point
        centerPoint = transform.position;
    }

    void Update()
    {
        angle += speed * Time.deltaTime; // Increase the angle over time
        float x = Mathf.Cos(angle) * radius;
        float z = Mathf.Sin(angle) * radius;

        // Update position relative to the center point
        transform.position = centerPoint + new Vector3(x, 0, z);
    }
}
