using UnityEngine;
using UnityEngine.UI;

public class UIAudioTester : MonoBehaviour
{
    public AudioSource audioSource;

    void Start()
    {
        GetComponent<Button>().onClick.AddListener(() =>
        {
            if (!audioSource.isPlaying)
                audioSource.Play();
        });
    }
}
