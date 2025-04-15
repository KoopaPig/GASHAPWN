using UnityEngine;

namespace GASHAPWN {
    public class PlayerAttachedFigure : MonoBehaviour
    {   
        private Figure attachedFigure;
        [SerializeField] private Transform capsPos;

        public void SetFigureInCapsule(Figure figure) {
            attachedFigure = figure;

            // All of this below ensures the scale of the model matches the world scale

            var obj = Instantiate(figure.capsuleModelPrefab);

            Vector3 originalScale = obj.transform.lossyScale;
            obj.transform.SetParent(capsPos, false);
            obj.transform.localPosition = Vector3.zero;
            obj.transform.localRotation = Quaternion.identity;

            // Restore world scale by recalculating local scale relative to parent
            Transform parent = obj.transform.parent;
            Vector3 parentScale = parent.lossyScale;
            obj.transform.localScale = new Vector3(
                originalScale.x / parentScale.x,
                originalScale.y / parentScale.y,
                originalScale.z / parentScale.z
            );
        }

        public Figure GetAttachedFigure() { return attachedFigure; }
    }
}


