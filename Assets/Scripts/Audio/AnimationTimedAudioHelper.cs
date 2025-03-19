using UnityEngine;
using UnityEngine.Audio;

namespace GASHAPWN.Audio
{
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
    }
}


