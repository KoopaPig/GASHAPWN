using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour
{
    private Rigidbody rb;
    private Vector2 moveInput;

    [SerializeField] private PlayerData playerData;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();

        if (playerData != null)
        {
            rb.linearDamping = playerData.drag;
            rb.angularDamping = playerData.angularDrag;

            if (playerData.sphereMaterial != null)
            {
                var coll = GetComponent<Collider>();
                coll.material = playerData.sphereMaterial;
            }
        }
    }

    public void OnMovement(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
    }

    private void FixedUpdate()
    {
        if (playerData == null) return;

        // Convert 2D input into a 3D force
        Vector3 force = new Vector3(moveInput.x, 0f, moveInput.y) * playerData.moveSpeed;

        // Apply force to roll the sphere
        rb.AddForce(force);
    }
}
