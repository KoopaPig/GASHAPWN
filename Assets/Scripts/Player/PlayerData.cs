using UnityEngine;

public class PlayerData : MonoBehaviour
{
    
    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    public float jumpForce = 5f;

    [Header("Physics Settings")]
    // Tweak these for a "floaty" feel
    public float drag = 0.0f;
    public float angularDrag = 0.05f;
    public PhysicsMaterial sphereMaterial; // optional, to reduce friction

}
