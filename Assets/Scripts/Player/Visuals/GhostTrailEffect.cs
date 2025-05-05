using UnityEngine;

public class GhostTrailEffect : MonoBehaviour
{
    public ParticleSystem ghostTrail;

    private void Start()
    {
        if (ghostTrail != null)
        {
            ghostTrail.Stop();
        }
    }

    public void StartTrail()
    {
        if (ghostTrail != null && !ghostTrail.isPlaying)
        {
            ghostTrail.Play();
        }
    }

    public void StopTrail()
    {
        if (ghostTrail != null && ghostTrail.isPlaying)
        {
            ghostTrail.Stop();
        }
    }
}
