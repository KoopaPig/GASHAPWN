using GASHAPWN.Audio;
using UnityEngine;

namespace GASHAPWN.Environment
{
    /// <summary>
    /// Applies knockback force to colliding player object
    /// </summary>
    public class KnockbackOnTouch : MonoBehaviour
    {
        public float knockbackForce = 10f;

        private void OnCollisionEnter(Collision collision)
        {
            string tag = collision.collider.tag;

            if (tag.Contains("Player"))
            {
                Rigidbody rb = collision.collider.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    Vector3 direction = (collision.transform.position - transform.position).normalized;
                    rb.linearVelocity = Vector3.zero; // Reset movement
                    rb.AddForce(direction * knockbackForce, ForceMode.Impulse);
                }
                GAME_SFXManager.Instance.Play_Boing(collision.transform);
            }
        }
    }
}