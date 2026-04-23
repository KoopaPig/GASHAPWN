using UnityEngine;

namespace GASHAPWN.Utility {
    public static class FigureResizeHelper
    {
        // Given a figure object, scale it up or down in relation to the parent transform
        public static void ResizeFigureObject(GameObject obj, Transform parentTransform, float scaleFactor = 1f)
        {
            obj.transform.SetParent(parentTransform, false);

            obj.transform.localPosition = Vector3.zero;
            obj.transform.localRotation = Quaternion.identity;
            obj.transform.localScale = obj.transform.localScale * scaleFactor;
        }
    }
}

