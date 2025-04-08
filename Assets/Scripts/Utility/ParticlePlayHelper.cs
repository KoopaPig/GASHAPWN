using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ParticlePlayHelper : MonoBehaviour
{
    [SerializeField] private List<ParticleSystem> particleEffects = new List<ParticleSystem>();

    public void PlayParticleEffect(int index)
    {
        if (particleEffects[index] != null)
        {
            particleEffects[index].Play();
        }
    }
}
