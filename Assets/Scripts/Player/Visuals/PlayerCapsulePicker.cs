using UnityEngine;

namespace GASHAPWN {
    /// <summary>
    /// Sets capsule prefab based on Player tag
    /// </summary>
    public class PlayerCapsulePicker : MonoBehaviour
    {
        [Header("Capsule Prefabs")]
        [SerializeField] private GameObject player1CapsulePrefab;
        [SerializeField] private GameObject player2CapsulePrefab;

        private ProgressiveCracking _progCracking;

        private void Awake()
        {
            _progCracking = GetComponent<ProgressiveCracking>();
        }

        private void Start()
        {
            SetCapsuleBasedOnTag();
        }

        private void SetCapsuleBasedOnTag()
        {
            string tag = transform.parent.tag;
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
                Transform glassSphere = newCapsule.transform.Find("GlassSphere");
                if (glassSphere != null)
                {
                    Renderer glassRenderer = glassSphere.GetComponent<Renderer>();
                    _progCracking.SetRenderer(glassRenderer);
                }
                else
                {
                    Debug.LogWarning("PlayerCapsulePicker: 'GlassSphere' not found in new capsule.");
                }
            }
            else
            {
                Debug.LogWarning($"PlayerCapsulePicker: No prefab found for tag {tag}.");
            }
        }
    }
}