using GASHAPWN.Audio;
using UnityEngine;

namespace GASHAPWN {
    [RequireComponent(typeof(PlayerEffectsHub))]
    public class Effect_DeathExplosion : MonoBehaviour
    {
        [Tooltip("Reference to figure container")]
        [SerializeField] private GameObject figurePosition;
        [Tooltip("Force of death explosion")]
        [SerializeField] private float explosionForce = 3f;

        // Get reference to higher-level player components through Player Effects Hub
        private PlayerEffectsHub _pEffectsHub;

        private GameObject _glassHemisphere;
        private GameObject _metalHemisphere;

        private void OnEnable()
        {
            _pEffectsHub = GetComponent<PlayerEffectsHub>();
            _pEffectsHub.pData.healthEvents.OnDeath.AddListener(DeathExplode);
        }
        
        private void OnDisable()
        {
            _pEffectsHub.pData.healthEvents.OnDeath.RemoveListener(DeathExplode);
        }

        private void DeathExplode(GameObject obj)
        {
            RefreshParts();
            ExplodePart(_glassHemisphere, -transform.right);
            ExplodePart(_metalHemisphere, transform.right);
            var meshObj = figurePosition.GetComponentInChildren<MeshRenderer>()?.gameObject;
            ExplodePart(meshObj, Vector3.up);
            GAME_SFXManager.Instance.Play_GlassBreak(transform);
        }

        /// <summary>
        /// Handle explosion of individual parts given Vector3 direction
        /// </summary>
        private void ExplodePart(GameObject part, Vector3 direction)
        {
            if (part == null) return;

            // If collider not present, add it
            if (part.GetComponent<Collider>() == null)
            {
                var meshFilter = part.GetComponent<MeshFilter>();
                if (meshFilter != null && meshFilter.sharedMesh != null)
                {
                    var meshCollider = part.AddComponent<MeshCollider>();
                    meshCollider.convex = true;
                }
                else
                {
                    part.AddComponent<BoxCollider>();
                }
            }
            part.layer = LayerMask.NameToLayer("Debris");

            // If rigidbody not present, add it
            if (!part.TryGetComponent(out Rigidbody rb))
            {
                rb = part.AddComponent<Rigidbody>();
            }

            // Configure rb
            rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
            rb.interpolation = RigidbodyInterpolation.Interpolate;
            rb.angularVelocity = Vector3.zero;
            rb.linearVelocity = Vector3.zero;

            // Add up and outward force
            rb.AddForce((direction + Vector3.up) * explosionForce);
        }

        // Refreshes reference to sub-objects because they can change during runtime
        private void RefreshParts()
        {
            // Find hemispheres
            _glassHemisphere = _pEffectsHub.PlayerCapsuleRoot.Find("PlayerCapsule")?.Find("GlassSphere")?.gameObject;
            _metalHemisphere = _pEffectsHub.PlayerCapsuleRoot.Find("PlayerCapsule")?.Find("MetalSphere")?.gameObject;
            if (_glassHemisphere == null || _metalHemisphere == null)
                Debug.LogWarning($"{nameof(Effect_DeathExplosion)}: Could not find hemispheres on capsule. Check if the object names match.");
        }
    }
}