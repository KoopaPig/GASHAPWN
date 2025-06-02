using UnityEngine;

namespace GASHAPWN {
    /// <summary>
    /// Used to test figure sizing in capsule and on shelf
    /// </summary>
    public class FigureSizingTest : MonoBehaviour
    {
        [SerializeField] public Figure selectedFigure;
        [SerializeField] private Transform shelfPos;

        private bool figureSet = false;

        public void SetFigure()
        {
            if (!figureSet)
            {
                FindFirstObjectByType<PlayerAttachedFigure>().SetFigureInCapsule(selectedFigure);
                Instantiate(selectedFigure.collectionModelPrefab, shelfPos);
                figureSet = true;
            }
            else Debug.LogWarning("Figure already set!");

        }

        private void Update()
        {
            // Pressing F sets figure
            if (Input.GetKeyDown(KeyCode.F)) SetFigure();
        }
    }
}