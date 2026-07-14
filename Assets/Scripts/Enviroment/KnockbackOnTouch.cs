using GASHAPWN.Audio;
using System;
using UnityEngine;

namespace GASHAPWN.Environment
{
    /// <summary>
    /// Applies knockback force to colliding player object
    /// </summary>
    public class KnockbackOnTouch : MonoBehaviour
    {
        public event Action<float> OnKnockback;

        public float knockbackForce = 10f;

        private void OnCollisionEnter(Collision collision)
        {
            string tag = collision.collider.tag;

            if (tag.Contains("Player"))
            {
                Rigidbody rb = collision.collider.GetComponent<PlayerController>().rb;
                if (rb != null)
                {
                    Vector3 direction = (collision.transform.position - transform.position).normalized;
                    rb.linearVelocity = Vector3.zero; // Reset movement
                    rb.AddForce(direction * knockbackForce, ForceMode.Impulse);
                }
                GAME_SFXManager.Instance.Play_Boing(collision.transform);
                OnKnockback?.Invoke(knockbackForce);
            }
        }
    }
}