using UnityEngine;
using UnityEngine.UI;

namespace GASHAPWN
{
    [CreateAssetMenu(fileName = "Level", menuName = "Scriptable Objects/Level")]
    public class Level : ScriptableObject
    {
        [Header("Atrributes")]
        public string levelName = null;
        public string levelSceneName = null;
        public Sprite levelPreviewIcon = null;
    }
}