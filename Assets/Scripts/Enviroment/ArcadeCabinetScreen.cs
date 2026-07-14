using UnityEngine;
using System.Collections;

public class ArcadeCabinetScreen : MonoBehaviour
{
    [Tooltip("The index of the material which will be modified")]
    [SerializeField] private int screenMaterialIndex = 1;

    
    [SerializeField] private Renderer targetRenderer;

    [Header("Colors")]
    [SerializeField] private Color[] colors;

    [Header("Intensity")]
    [SerializeField] private float minIntensity = 1f;
    [SerializeField] private float maxIntensity = 5f;

    [Header("Timing")]
    [SerializeField] private float minInterval = 0.05f;
    [SerializeField] private float maxInterval = 0.3f;

    private Material[] mats;

    void Start()
    {
        mats = targetRenderer.materials;

        StartCoroutine(FlashRoutine());
    }

    //IEnumerator FlashRoutine()
    //{
    //    while (true)
    //    {
    //        // Pick random color + intensity
    //        Color color = colors[Random.Range(0, colors.Length)];
    //        float intensity = Random.Range(minIntensity, maxIntensity);

    //        Color finalColor = color * intensity;

    //        mats[screenMaterialIndex].SetColor("_EmissionColor", finalColor);

    //        float wait = Random.Range(minInterval, maxInterval);
    //        yield return new WaitForSeconds(wait);
    //    }
    //}

    IEnumerator FlashRoutine()
    {
        Color current = Color.black;

        while (true)
        {
            Color target = colors[Random.Range(0, colors.Length)]
                           * Random.Range(minIntensity, maxIntensity);

            float duration = Random.Range(0.05f, 0.2f);
            float t = 0f;

            while (t < duration)
            {
                t += Time.deltaTime;
                Color c = Color.Lerp(current, target, t / duration);
                mats[screenMaterialIndex].SetColor("_EmissionColor", c);
                yield return null;
            }

            current = target;

            yield return new WaitForSeconds(Random.Range(minInterval, maxInterval));
        }
    }
}
