// Adapted and modified from Unity-Object-Fade scripts by jhutchines via GitHub

using System.Collections;
using System.Collections.Generic;
using GASHAPWN;
using UnityEngine;

public class ObjectFade : MonoBehaviour
{

    [SerializeField] public enum FadeType
    {
        Single,
        Whole,
        Transparent
    }

    public FadeType fadeType;
    public float fadeDuration = 0.2f;
    public bool customFade = false;
    [Range(0.0f, 1.0f)]
    [SerializeField] public float alphaFadeToValue = 0f;
    
    bool faded;
    FadeCheck fadeCheck;
    private List<FadeCheck> fadeChecks = new();
    Renderer objectRenderer;
    Color color;
    GameObject highestParent;

    private float originalAlpha = -1f;
    private Coroutine fadeCoroutine;

    public void SetFaded(bool shouldBeFaded)
    {
        switch (fadeType)
        {
            case FadeType.Transparent:
                if (shouldBeFaded && !faded)
                {
                    TriggerFade(true);
                    faded = true;
                }
                else if (!shouldBeFaded && faded)
                {
                    TriggerFade(false);
                    faded = false;
                }
                break;
            case FadeType.Single:
                if (shouldBeFaded && !faded)
                {
                    foreach (Material material in objectRenderer.materials)
                    {
                        MaterialObjectFade.MakeFade(material);
                        Color newColor = material.color;
                        if (customFade) newColor.a = alphaFadeToValue;
                        else newColor.a = fadeCheck.fadeTo;
                        material.color = newColor;
                    }
                    faded = true;
                }
                else if (!shouldBeFaded && faded)
                {
                    foreach (Material material in objectRenderer.materials)
                    {
                        MaterialObjectFade.MakeOpaque(material);
                        Color newColor = material.color;
                        newColor.a = 1;
                        material.color = newColor;
                    }
                    faded = false;
                }
                break;
            case FadeType.Whole:
                if (shouldBeFaded && !faded)
                {
                    GameObject findParent;
                    if (transform.parent != null) findParent = transform.parent.gameObject;
                    else findParent = null;
                    while (findParent != null)
                    {
                        if (findParent.GetComponent<Renderer>() != null)
                        {
                            foreach (Material material in findParent.GetComponent<Renderer>().materials)
                            {
                                MaterialObjectFade.MakeFade(material);
                                Color newColor = material.color;
                                if (customFade) newColor.a = alphaFadeToValue;
                                else newColor.a = fadeCheck.fadeTo;
                                material.color = newColor;
                            }
                        }
                        for (int i = 0; i < findParent.transform.childCount; i++)
                        {
                            foreach (Material material in findParent.transform.GetChild(i).GetComponent<Renderer>().materials)
                            {
                                MaterialObjectFade.MakeFade(material);
                                Color newColor = material.color;
                                if (customFade) newColor.a = alphaFadeToValue;
                                else newColor.a = fadeCheck.fadeTo;
                                material.color = newColor;
                            }
                        }
                        if (findParent.transform.parent != null)
                        {
                            findParent = findParent.transform.parent.gameObject;
                            if (findParent.GetComponent<Renderer>() == null) findParent = null;
                        }
                        else findParent = null;
                    }
                    faded = true;
                }
                else if (!shouldBeFaded && faded)
                {
                    GameObject findParent;
                    if (transform.parent != null) findParent = transform.parent.gameObject;
                    else findParent = null;
                    while (findParent != null)
                    {
                        if (findParent.GetComponent<Renderer>() != null)
                        {
                            foreach (Material material in findParent.GetComponent<Renderer>().materials)
                            {
                                MaterialObjectFade.MakeOpaque(material);
                                Color newColor = material.color;
                                newColor.a = 1f;
                                material.color = newColor;
                            }
                        }
                        for (int i = 0; i < findParent.transform.childCount; i++)
                        {
                            foreach (Material material in findParent.transform.GetChild(i).GetComponent<Renderer>().materials)
                            {
                                MaterialObjectFade.MakeOpaque(material);
                                Color newColor = material.color;
                                newColor.a = 1f;
                                material.color = newColor;
                            }
                        }
                        if (findParent.transform.parent != null)
                        {
                            findParent = findParent.transform.parent.gameObject;
                            if (findParent.GetComponent<Renderer>() == null) findParent = null;
                        }
                        else findParent = null;
                    }
                    faded = false;
                }
                break;
            default:
                break;
        }
    }

    private void OnDisable()
    {
        if (ObjectFadeManager.Instance != null)
        {
            ObjectFadeManager.Instance.Unregister(this);
        }
    }

    private void Start()
    {
        if (ObjectFadeManager.Instance != null)
        {
            ObjectFadeManager.Instance.Register(this);
        }
        objectRenderer = GetComponent<Renderer>();
        color = objectRenderer.material.color;
        highestParent = gameObject;
        while (highestParent.transform.parent != null)
        {
            highestParent = highestParent.transform.parent.gameObject;
        }

        originalAlpha = objectRenderer.material.color.a;
    }

    private void TriggerFade(bool fadeOut)
    {
        if (fadeCoroutine != null)
            StopCoroutine(fadeCoroutine);

        float targetAlpha = fadeOut ? (customFade ? 0f : alphaFadeToValue) : originalAlpha;

        fadeCoroutine = StartCoroutine(TransFadeTo(targetAlpha));
    }

    private IEnumerator TransFadeTo(float targetAlpha)
    {
        float startAlpha = objectRenderer.material.color.a;
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

        if (targetAlpha >= 0.99f)
        {
            foreach (var mat in objectRenderer.materials)
                MaterialObjectFade.MakeOpaque(mat);
        }

        if (targetAlpha <= 0.01f)
        {
            foreach (var mat in objectRenderer.materials)
                MaterialObjectFade.MakeFade(mat);
        }

        fadeCoroutine = null;
        faded = targetAlpha < 1f;
    }
}
