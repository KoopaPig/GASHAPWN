using UnityEngine;

public class LaunchPad : MonoBehaviour
{
    public float launchForce = 10f;
    public Vector3 launchAngle = new Vector3(45f, 0f, 0f); // x = pitch, y = yaw, z = roll (not used)

    private void OnCollisionEnter(Collision collision)
    {
        string tag = collision.gameObject.tag;
        if (tag == "Player1" || tag == "Player2")
        {
            Rigidbody playerRb = collision.gameObject.GetComponent<Rigidbody>();
            if (playerRb != null)
            {
                // Calculate direction from angles
                Quaternion rotation = Quaternion.Euler(launchAngle);
                Vector3 launchDirection = rotation * Vector3.forward;

                playerRb.linearVelocity = Vector3.zero;
                playerRb.AddForce(launchDirection.normalized * launchForce, ForceMode.VelocityChange);
            }
        }
    }
}
