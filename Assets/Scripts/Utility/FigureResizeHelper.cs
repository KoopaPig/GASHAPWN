using UnityEngine;

namespace GASHAPWN {
    public static class FigureResizeHelper
    {
        // Given an object, resize it to worldspace scale according the parent's scale
        public static void ResizeFigureObject(GameObject obj, Transform parentTransform, float scaleFactor = 1f)
        {
            Vector3 originalScale = obj.transform.lossyScale;
            obj.transform.SetParent(parentTransform, false);
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
            obj.transform.localScale *= scaleFactor;
        }
    }
}

