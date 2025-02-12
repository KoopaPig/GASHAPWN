using UnityEngine;

public class ToggleVisibility : MonoBehaviour
{
    private Renderer objectRenderer;
    private Collider objectCollider;
    private bool isVisible = true;

    void Start()
    {
        objectRenderer = GetComponent<Renderer>();
        objectCollider = GetComponent<Collider>();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.T)) // Change "T" to any key you want
        {
            ToggleObject();
        }
    }

    public void ToggleObject()
    {
        isVisible = !isVisible;
        if (objectRenderer != null) objectRenderer.enabled = isVisible;
        if (objectCollider != null) objectCollider.enabled = isVisible;
    }
}
