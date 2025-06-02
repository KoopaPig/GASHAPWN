using UnityEngine;
using UnityEngine.UI;

namespace GASHAPWN.UI
{
    /// <summary>
    /// Controller for displaying Figure rarity in the form of stars
    /// </summary>
    [RequireComponent(typeof(HorizontalLayoutGroup))]
    public class StarsGUI : MonoBehaviour
    {
        [Tooltip("Prefab for filled star UI element")]
        [SerializeField] private GameObject filledStarPrefab;
        [Tooltip("Prefab for empty star UI element")]
        [SerializeField] private GameObject emptyStarPrefab;

        // Determines what star values correspond to a given rarity
        private int GetStarCount(float rarity)
        {
            if (rarity >= 0.25f) return 1;
            if (rarity >= 0.15f) return 2;
            if (rarity >= 0.1f) return 3;
            if (rarity >= 0.05f) return 4;
            return 5;
        }

        /// <summary>
        /// Sets amount of stars given Figure
        /// </summary>
        /// <param name="figure"></param>
        public void SetStars(Figure figure)
        {
            // Clear previous stars
            foreach (Transform child in this.transform)
            {
                Destroy(child.gameObject);
            }

            int starCount = GetStarCount(figure.Rarity);
            int emptyCount = 5 - starCount;

            // Filled stars
            for (int i = 0; i < starCount; i++)
            {
                Instantiate(filledStarPrefab, this.transform);
            }

            // Empty stars
            for (int i = 0; i < emptyCount; i++)
            {
                Instantiate(emptyStarPrefab, this.transform);
            }
        }
    }
}