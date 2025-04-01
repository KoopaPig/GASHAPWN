using GASHAPWN.UI;
using UnityEngine;

namespace GASHAPWN {
    public class CollectionCameraHelper : MonoBehaviour
    {
        public void CollectionGUISetActive() {
            FindFirstObjectByType<CollectionGUI>().CollectionGUISetActive(true);
        }
    }
}

