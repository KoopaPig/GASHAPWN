using UnityEngine;

public class KnockbackOnTouch3D : MonoBehaviour
{
    public float knockbackForce = 10f;

    private void OnCollisionEnter(Collision collision)
    {
        string tag = collision.collider.tag;

        if (tag == "Player1" || tag == "Player2")
        {
            Rigidbody rb = collision.collider.GetComponent<Rigidbody>();
            if (rb != null)
            {
                Vector3 direction = (collision.transform.position - transform.position).normalized;
                rb.linearVelocity = Vector3.zero; // Reset movement
                rb.AddForce(direction * knockbackForce, ForceMode.Impulse);
            }
        }
    }
}
