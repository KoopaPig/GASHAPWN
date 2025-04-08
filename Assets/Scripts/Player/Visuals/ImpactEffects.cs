using UnityEngine;

public class ImpactEffects : MonoBehaviour
{
    [Header("Impact Prefabs")]
    public GameObject hitEffectPrefab;
    public GameObject deflectEffectPrefab;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip damageSound;
    public AudioClip deflectSound;

    [Header("Spawn Settings")]
    public Transform effectSpawnPoint;
    public float destroyAfterSeconds = 2f;

    public void PlayDamageEffect()
    {
        SpawnEffect(hitEffectPrefab);
        PlaySound(damageSound);
    }

    public void PlayDeflectEffect()
    {
        SpawnEffect(deflectEffectPrefab);
        PlaySound(deflectSound);
    }

    private void SpawnEffect(GameObject prefab)
    {
        if (prefab == null) return;

        Vector3 spawnPos = effectSpawnPoint != null ? effectSpawnPoint.position : transform.position;
        Quaternion spawnRot = Quaternion.identity;

        GameObject effect = Instantiate(prefab, spawnPos, spawnRot);
        Destroy(effect, destroyAfterSeconds); // Clean up
    }

    private void PlaySound(AudioClip clip)
    {
        if (audioSource != null && clip != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }
}
