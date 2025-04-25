using System.Collections;
using UnityEngine;

public class DisappearAfterAnimation : MonoBehaviour
{
    private Animator animator;
    private Renderer objectRenderer;

    void Start()
    {
        objectRenderer = GetComponent<Renderer>();
        animator = GetComponent<Animator>();
        if (animator == null)
        {
            Debug.LogError("Animator component missing from object.");
        }
    }

    // This function should be called at the end of the animation via an Animation Event
    public void HideObject()
    {
        StartCoroutine(FadeOut(0.2f));
    }

    private IEnumerator FadeOut(float fadeDuration)
    {
        float startAlpha = objectRenderer.material.color.a;
        float targetAlpha = 0.0f;
        float time = 0f;

        while (time < fadeDuration)
        {
            float alpha = Mathf.Lerp(startAlpha, targetAlpha, time / fadeDuration);
            foreach (var mat in objectRenderer.materials)
            {
                Color c = mat.color;
                c.a = alpha;
                mat.color = c;
            }
            time += Time.deltaTime;
            yield return null;
        }

        foreach (var mat in objectRenderer.materials)
        {
            Color c = mat.color;
            c.a = targetAlpha;
            mat.color = c;
        }

        if (targetAlpha <= 0.01f)
        {
            foreach (var mat in objectRenderer.materials)
                MaterialObjectFade.MakeFade(mat);
        }
    }
}
