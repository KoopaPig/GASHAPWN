using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour
{
    private Rigidbody rb;
    private Vector2 moveInput;

    [SerializeField] private PlayerData playerData;
    [SerializeField] private PlayerStates playerStates;

    private void Awake()
    {
            Physics.gravity = new Vector3(0, -9.81f * 3f, 0);

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

    public void OnJump(InputAction.CallbackContext context)
    {
        // Only process jump if controls are enabled.
        if (playerStates != null && !playerStates.controlsEnabled)
            return;

        // Check that the action is performed and the player is grounded.
        if (context.performed && playerStates != null && playerStates.isGrounded)
        {
            if (playerData != null)
            {
                rb.AddForce(Vector3.up * playerData.jumpForce, ForceMode.Impulse);
            }
        }
    }

    private void FixedUpdate()
    {
        if (playerData == null && playerStates != null && !playerStates.controlsEnabled) return;

        if (playerStates != null && playerStates.isGrounded)
        {
            // When on the ground, use the defined damping values.
            rb.linearDamping = playerData.drag;
            rb.angularDamping = playerData.angularDrag;
        }
        else
        {
            // When airborne, remove air resistance.
            rb.linearDamping = 0f;
            rb.angularDamping = 0f;
        }

        if (playerStates.isGrounded)
        {
            Vector3 force = new Vector3(moveInput.x, 0f, moveInput.y) * playerData.moveSpeed;
            rb.AddForce(force);
        }

        Vector3 torque = new Vector3(moveInput.y, 0f, -moveInput.x) * playerData.airTorque;
        rb.AddTorque(torque);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            if (playerStates != null)
            {
                playerStates.isGrounded = true;
            }
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            if (playerStates != null)
            {
                playerStates.isGrounded = false;
            }
        }
    }
}
