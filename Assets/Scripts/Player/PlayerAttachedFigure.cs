using UnityEngine;
using GASHAPWN.Utility;

namespace GASHAPWN {
    /// <summary>
    /// Handles the Figure attached to the player
    /// </summary>
    public class PlayerAttachedFigure : MonoBehaviour
    {
        [Tooltip("Transform in capsule to spawn Figure")]
        [SerializeField] private Transform capsPos;

        private Figure attachedFigure;

        /// <summary>
        /// Sets attached Figure and instantiates it
        /// </summary>
        /// <param name="figure">Figure to place in capsule</param>
        /// <param name="scaleFactor">Scale of figure in capsule</param>
        public void SetFigureInCapsule(Figure figure, float scaleFactor = 1f) {
            attachedFigure = figure;

            var obj = Instantiate(figure.capsuleModelPrefab);
            FigureResizeHelper.ResizeFigureObject(obj, capsPos, scaleFactor);
        }

        /// <summary>
        /// Get the Figure attached to this player
        /// </summary>
        public Figure GetAttachedFigure() { return attachedFigure; }
    }
}


