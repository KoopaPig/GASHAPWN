using UnityEngine;

public class WASDMovement : MonoBehaviour
{
    public float speed = 5f;

    void Update()
    {
        float moveX = Input.GetAxis("Horizontal");
        float moveZ = Input.GetAxis("Vertical");

        Vector3 move = new Vector3(moveX, 0f, moveZ) * speed * Time.deltaTime;
        transform.Translate(move, Space.World);
    }
}
