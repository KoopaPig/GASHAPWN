using UnityEngine;

public class ParticlePlayHelper : MonoBehaviour
{
    [SerializeField] private ParticleSystem particleEffect;
    public void PlayParticleEffect()
    {
        if (particleEffect != null)
        {
            particleEffect.Play();
        }
    }
}
