using UnityEngine;
using System.Collections.Generic;

namespace GASHAPWN.Utility
{
    /// <summary>
    /// Defines set of functions to play particle effects via Animation Events
    /// </summary>
    public class ParticlePlayHelper : MonoBehaviour
    {
        // List of particle effects
        [SerializeField] private List<ParticleSystem> particleEffects = new List<ParticleSystem>();

        // Play particle event given index 
        public void PlayParticleEffect(int index)
        {
            if (particleEffects[index] != null && index < particleEffects.Count)
            {
                particleEffects[index].Play();
            }
            else Debug.LogError("ParticlePlayHelper: Particle Effects list is empty or index is out of range");
        }
    }
}