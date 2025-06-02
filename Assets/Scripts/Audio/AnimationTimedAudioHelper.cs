using UnityEngine;
using UnityEngine.Audio;

namespace GASHAPWN.Audio
{
    /// <summary>
    /// Defines set of functions to play sounds via Animation Events
    /// </summary>
    public class AnimationTimedAudioHelper : MonoBehaviour
    {
        public void Play_CapsuleOpen()
        {
            AudioManager.Instance.PlaySound("SFX_Firework_Explosion_1");
            AudioManager.Instance.PlaySound("SFX_Firework_Explosion_3");
        }

        public void Play_Jingle()
        {
            AudioManager.Instance.PlaySound("SFX_STGR_Win_Reaction");
        }

        public void Play_Countdown_Tick()
        {
            AudioManager.Instance.PlaySound("SFX_UI_Countdown_Clock_Tick"); 
        }

        public void Play_Capsule_Stretch() 
        {
            AudioManager.Instance.PlaySound("SFX_Capsule_Stretch");
        }

        public void Play_Pwn_Start() 
        {
            AudioManager.Instance.PlaySound("SFX_UI_Pwn_Start");
        }
    }
}