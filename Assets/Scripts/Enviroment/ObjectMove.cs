using UnityEngine;

public class RotateAround : MonoBehaviour
{
    public float speed = 10f; // Speed of rotation
    public float radius = 5f; // Distance from the origin

    private float angle = 0f;

    void Update()
    {
        angle += speed * Time.deltaTime; // Increase the angle over time
        float x = Mathf.Cos(angle) * radius;
        float z = Mathf.Sin(angle) * radius;
        transform.position = new Vector3(x, transform.position.y, z);
    }
}
