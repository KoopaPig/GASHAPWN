using UnityEngine;
using GASHAPWN.Utility;

namespace GASHAPWN {
    public class PlayerAttachedFigure : MonoBehaviour
    {   
        private Figure attachedFigure;
        [SerializeField] private Transform capsPos;

        public void SetFigureInCapsule(Figure figure, float scaleFactor = 1f) {
            attachedFigure = figure;

            // Ensure the scale of the model matches the world scale
            var obj = Instantiate(figure.capsuleModelPrefab);
            FigureResizeHelper.ResizeFigureObject(obj, capsPos, scaleFactor);
        }

        public Figure GetAttachedFigure() { return attachedFigure; }
    }
}


