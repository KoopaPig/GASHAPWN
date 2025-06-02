using UnityEngine;
using UnityEngine.UI;

namespace GASHAPWN.UI
{
    /// <summary>
    /// Controls scrolling background
    /// </summary>
    public class Scrolling : MonoBehaviour
    {
        [Tooltip("Image to scroll")]
        [SerializeField] private RawImage _img;
        
        // Scroll increment
        [SerializeField] private float _x, _y;

        void Update()
        {
            _img.uvRect = new Rect(_img.uvRect.position + new Vector2(_x, _y) * Time.deltaTime, _img.uvRect.size);
        }
    }
}