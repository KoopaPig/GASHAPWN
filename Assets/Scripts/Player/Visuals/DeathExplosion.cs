using UnityEngine;

namespace GASHAPWN {
    public class DeathExplosion : MonoBehaviour
    {
        [SerializeField] private GameObject figurePosition;
        private GameObject glassHemisphere;
        private GameObject metalHemisphere;
        private PlayerData playerData;

        [Header("Explosion Controls")]
        [SerializeField] private float explosionForce = 500f;
        [SerializeField] private float explosionRadius = 2f;

        private void OnEnable()
        {
            playerData = GetComponent<PlayerData>();
            playerData.OnDeath.AddListener(DeathExplode);
        }
        
        private void OnDisable()
        {
            playerData.OnDeath.RemoveListener(DeathExplode);
        }

        private void DeathExplode(GameObject obj)
        {
            RefreshParts();
            ExplodePart(glassHemisphere, -transform.right);
            ExplodePart(metalHemisphere, transform.right);
            ExplodePart(figurePosition, Vector3.up);
        }

        /// <summary>
        /// Handle explosion of individual parts given Vector3 direction
        /// </summary>
        /// <param name="part"></param>
        /// <param name="direction"></param>
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
            glassHemisphere = transform.Find("PlayerCapsule")?.Find("GlassSphere")?.gameObject;
            metalHemisphere = transform.Find("PlayerCapsule")?.Find("MetalSphere")?.gameObject;

            if (glassHemisphere == null || metalHemisphere == null)
                Debug.LogWarning("DeathExplosion: Could not find hemispheres on capsule. Check if the object names match.");
        }
    }
}