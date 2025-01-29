using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerControls : MonoBehaviour
{

    [Header("References")]
    public PlayerData playerData; // link this to your PlayerData

    private Rigidbody rb;
    private Vector2 inputMove;

    private void Awake()
    {
        // Get the Rigidbody attached to this sphere
        rb = GetComponent<Rigidbody>();

        // Set up physics materials, drag, etc. for floaty movement
        if (playerData != null)
        {
            rb.linearDamping = playerData.drag;
            rb.angularDamping = playerData.angularDrag;

            if (playerData.sphereMaterial != null)
            {
                var collider = GetComponent<Collider>();
                collider.material = playerData.sphereMaterial;
            }
        }
    }

    public void OnMove(InputValue value)
    {
        inputMove = value.Get<Vector2>();
    }

    private void FixedUpdate()
    {
        if (playerData == null) return;

        // Convert 2D input into 3D direction
        Vector3 force = new Vector3(inputMove.x, 0f, inputMove.y) * playerData.moveSpeed;

        // Apply force for "floaty" rolling
        rb.AddForce(force);
    }

}
