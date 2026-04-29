using UnityEngine;
using DG.Tweening;

public class GraphicEffect : MonoBehaviour
{
    private float _lifetime = 0.3f;
    private GameObject _instance;

    #region SPAWN

        public GameObject SpawnAtTransform(Transform target, float lifetime = 0, bool matchRotation = false)
        {
            Quaternion rot = matchRotation ? target.rotation : Quaternion.identity;
            _instance = Instantiate(this.gameObject, target.position, rot);
            if (lifetime > 0) this._lifetime = lifetime;
            return _instance;
        }

        public GameObject SpawnAtContact(ContactPoint contact, Transform other = null, float lifetime = 0, bool faceOther = false)
        {
            GameObject obj = Instantiate(this.gameObject, contact.point, Quaternion.identity);

            // Offset slightly off the surface
            obj.transform.position += contact.normal * 0.002f;

            // Rotation logic
            if (faceOther && other != null)
            {
                Vector3 dir = (other.position - contact.point).normalized;
                obj.transform.rotation = Quaternion.LookRotation(dir);
            }
            else
            {
                obj.transform.rotation = Quaternion.LookRotation(-contact.normal);
            }
            if (lifetime > 0) this._lifetime = lifetime;

            _instance = obj;
            return obj;
        }

    #endregion

    #region EFFECTS
        
        /// <summary>
        /// Rapidly shrinks and fades graphic; if light attached, flash it
        /// </summary>
        /// <param name="shrinkFactor"></param>
        public void FlashAndShrink(float shrinkFactor = 0.5f)
        {
            if (!HasInstance()) return;
            var sr = _instance.GetComponent<SpriteRenderer>();
            if (sr == null)
            {
                Debug.LogError($"{nameof(GraphicEffect)}: Could not call FlashAndShrink, no SpriteRenderer on object.");
                return;
            }
    
            Vector3 endSize = _instance.transform.localScale * shrinkFactor;
            var light = _instance.GetComponent<Light>();

            var seq = DG.Tweening.DOTween.Sequence();

            // Scale over time
            seq.Join(_instance.transform.DOScale(endSize, _lifetime));

            // Flash light (if present)
            if (light != null)
            {
                light.intensity = 0f;

                var flashSeq = DG.Tweening.DOTween.Sequence();
                flashSeq.Append(light.DOIntensity(0.5f, 0.05f));
                flashSeq.Append(light.DOIntensity(0f, 0.25f));

                seq.Join(flashSeq);
            }

            // Fade in/out
            var fadeSeq = DG.Tweening.DOTween.Sequence();
            fadeSeq.Append(sr.DOFade(1f, _lifetime / 2).From(0f));
            fadeSeq.Append(sr.DOFade(0f, _lifetime / 2));
            seq.Join(fadeSeq);

            // Cleanup
            seq.OnComplete(() => Destroy(_instance));
        }

    #endregion

    private bool HasInstance()
    {
        if (_instance == null)
        {
            Debug.LogWarning($"{nameof(GraphicEffect)}: Tried to use effect, but instance is null.", this);
            return false;
        }
        return true;
    }
}