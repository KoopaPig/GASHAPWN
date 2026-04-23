using GASHAPWN.Audio;
using UnityEngine;

namespace GASHAPWN {
    [RequireComponent(typeof(PlayerEffectsHub))]
    public class Effect_DeathExplosion : MonoBehaviour
    {
        [SerializeField] private GameObject figurePosition;
        [SerializeField] private float explosionForce = 3f;

        private GameObject glassHemisphere;
        private GameObject metalHemisphere;

        // Get reference to higher-level player components through Player Effects Hub
        private PlayerEffectsHub _pEffectsHub;

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
            ExplodePart(glassHemisphere, -transform.right);
            ExplodePart(metalHemisphere, transform.right);
            ExplodePart(figurePosition, Vector3.up);
            GAME_SFXManager.Instance.Play_GlassBreak(transform);
        }


        // ISSUE: This just isn't working correctly, colliders acting weird and too much force being applied

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

            // If rigidbody not present, add it
            if (!part.TryGetComponent(out Rigidbody rb))
                rb = part.AddComponent<Rigidbody>();

            // Add up and outward force
            rb.AddForce((direction + Vector3.up) * explosionForce);
        }

        // Refreshes reference to sub-objects because they can change during runtime
        private void RefreshParts()
        {
            // Find hemispheres
            glassHemisphere = _pEffectsHub.PlayerCapsuleRoot.Find("PlayerCapsule")?.Find("GlassSphere")?.gameObject;
            metalHemisphere = _pEffectsHub.PlayerCapsuleRoot.Find("PlayerCapsule")?.Find("MetalSphere")?.gameObject;
            if (glassHemisphere == null || metalHemisphere == null)
                Debug.LogWarning("DeathExplosion: Could not find hemispheres on capsule. Check if the object names match.");
        }
    }
}