using UnityEngine;

public class PlayerData : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;

    [Header("Physics Floatiness")]
    public float drag = 0f;
    public float angularDrag = 0.05f;

    [Header("Physic Material (optional)")]
    public PhysicsMaterial sphereMaterial; 
}
