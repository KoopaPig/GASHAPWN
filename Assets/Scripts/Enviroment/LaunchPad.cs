using GASHAPWN.Audio;
using UnityEngine;

namespace GASHAPWN.Environment
{
    /// <summary>
    /// Launches colliding player object given launch vector
    /// </summary>
    public class LaunchPad : MonoBehaviour
    {
        [Tooltip("Force with which to launch the player")]
        public float launchForce = 10f;

        [Tooltip("Angle which the player will be launched")]
        // x = pitch, y = yaw, z = roll (not used)
        public Vector3 launchAngle = new Vector3(45f, 0f, 0f);

        private void OnCollisionEnter(Collision collision)
        {
            string tag = collision.gameObject.tag;
            if (tag.Contains("Player"))
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
                GAME_SFXManager.Instance.Play_BouncePad(collision.transform);
            }
        }
    }
}