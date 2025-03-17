using UnityEngine;

public class DisappearAfterAnimation : MonoBehaviour
{
    private Animator animator;

    void Start()
    {
        animator = GetComponent<Animator>();
        if (animator == null)
        {
            Debug.LogError("Animator component missing from object.");
        }
    }

    // This function should be called at the end of the animation via an Animation Event
    public void HideObject()
    {
        gameObject.SetActive(false);
    }
}
