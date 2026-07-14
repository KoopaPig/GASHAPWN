using System.Collections;
using UnityEngine;

namespace GASHAPWN.Utility
{
    public static class PlayerHelpers
    {
        /// <summary>
        /// Rotates target given up vector and duration
        /// </summary>
        public static IEnumerator RotateUpDirectionCoroutine(Rigidbody rb, Transform transform, Vector3 desiredUp, float duration)
        {
            Vector3 initialVelocity = rb.linearVelocity;
            Vector3 initialAngularVelocity = rb.angularVelocity;

            Quaternion initialRotation = transform.rotation;
            Quaternion targetRotation = Quaternion.FromToRotation(-transform.up, -desiredUp) * transform.rotation;

            // Break
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);

                rb.linearVelocity = Vector3.Lerp(initialVelocity, Vector3.zero, t);
                rb.angularVelocity = Vector3.Lerp(initialAngularVelocity, Vector3.zero, t);

                transform.rotation = Quaternion.Slerp(initialRotation, targetRotation, t);

                yield return new WaitForFixedUpdate();
            }
        }

        /// <summary>
        /// Snaps target to given up vector
        /// </summary>
        /// <param name="target"></param>
        /// <param name="desiredUp"></param>
        public static void SnapUpDirection(Transform target, Vector3 desiredUp)
        {
            Quaternion delta = Quaternion.FromToRotation(target.up, desiredUp);
            target.rotation = delta * target.rotation;
        }

        /// <summary>
        /// Destroys particle system when finished or given max lifetime
        /// </summary>
        /// <param name="particleSystem"></param>
        /// <param name="maxLifetime"></param>
        /// <returns></returns>
        public static IEnumerator DestroyParticleSystemWhenDone(ParticleSystem particleSystem, float maxLifetime = 5f)
        {
            float timer = 0f;

            while (particleSystem != null && particleSystem.IsAlive(true) && timer < maxLifetime)
            {
                timer += Time.deltaTime;
                yield return null;
            }

            if (particleSystem != null)
                GameObject.Destroy(particleSystem.gameObject);
        }
    }
}