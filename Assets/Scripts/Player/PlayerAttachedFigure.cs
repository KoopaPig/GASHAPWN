using UnityEngine;

namespace GASHAPWN {
    public class PlayerAttachedFigure : MonoBehaviour
    {   
        private Figure attachedFigure;
        [SerializeField] private Transform capsPos;

        public void SetFigureInCapsule(Figure figure) {
            attachedFigure = figure;
            Instantiate(figure.capsuleModelPrefab, capsPos.parent.transform);
        }

        public Figure GetAttachedFigure() { return attachedFigure; }
    }
}


