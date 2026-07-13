using UnityEngine;

namespace GASHAPWN
{
    [CreateAssetMenu(fileName = "Level", menuName = "Scriptable Objects/Level")]
    public class Level : ScriptableObject
    {
        [Header("Atrributes")]
        public string levelName = null;
        //public string levelSceneName = null;
        public Sprite levelPreviewIconA = null;
        public Sprite levelPreviewIconB = null;

        [Header("Level Content")]
        public GameObject levelPrefab;
    }
}