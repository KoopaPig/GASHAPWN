using UnityEngine;

namespace GASHAPWN {
    public class PlayerCapsulePicker : MonoBehaviour
    {
        [Header("Capsule Prefabs")]
        [SerializeField] private GameObject player1CapsulePrefab;
        [SerializeField] private GameObject player2CapsulePrefab;

        private void Start()
        {
            SetCapsuleBasedOnTag();
        }

        private void SetCapsuleBasedOnTag()
        {
            string tag = gameObject.tag;
            Transform oldCapsule = transform.Find("PlayerCapsule");

            if (oldCapsule != null) Destroy(oldCapsule.gameObject);

            GameObject capsuleToSpawn = tag switch
            {
                "Player1" => player1CapsulePrefab,
                "Player2" => player2CapsulePrefab,
                _ => player1CapsulePrefab
            };

            if (capsuleToSpawn != null)
            {
                GameObject newCapsule = Instantiate(capsuleToSpawn, transform);
                newCapsule.name = "PlayerCapsule";

                // Assign Renderer from GlassSphere to ProgressiveCracking
                var cracking = GetComponent<ProgressiveCracking>();
                if (cracking != null)
                {
                    Transform glassSphere = newCapsule.transform.Find("GlassSphere");
                    if (glassSphere != null)
                    {
                        Renderer glassRenderer = glassSphere.GetComponent<Renderer>();
                        cracking.SetRenderer(glassRenderer);
                    }
                    else
                    {
                        Debug.LogWarning("PlayerCapsulePicker: 'GlassSphere' not found in new capsule.");
                    }
                }
            }
            else
            {
                Debug.LogWarning($"PlayerCapsulePicker: No prefab found for tag {tag}.");
            }
        }
    }
}