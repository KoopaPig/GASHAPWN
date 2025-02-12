using System.Collections;
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
        // Increase gravity strength if needed.
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
        // Process movement only if input is enabled.
        moveInput = context.ReadValue<Vector2>();
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        if (playerData != null && !playerData.controlsEnabled)
            return;

        if (context.performed && playerData != null && playerData.isGrounded)
        {
            rb.AddForce(Vector3.up * playerData.jumpForce, ForceMode.Impulse);
        }
    }

    public void OnSlam(InputAction.CallbackContext context)
    {
        if (playerData == null || !playerData.controlsEnabled)
            return;

        if (context.performed && !playerData.isGrounded && !playerData.hasSlammed)
        {
            // Start the slam routine that makes the player float for 1 second before slamming.
            StartCoroutine(SlamCoroutine());
        }
    }

    private IEnumerator SlamCoroutine()
    {
        // Disable controls so no other actions interrupt the slam.
        playerData.controlsEnabled = false;

        // Cancel any current momentum.
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        
        // Turn off gravity so the player floats in place.
        rb.useGravity = false;
        
        // Wait for 1 second.
        yield return new WaitForSeconds(playerData.slamDelay);
        
        // Re-enable gravity.
        rb.useGravity = true;
        
        // Apply a strong downward slam force.
        rb.AddForce(Vector3.down * playerData.slamForce, ForceMode.Impulse);
        
        // Mark that the slam has been executed so it can’t be used again mid-air.
        playerData.hasSlammed = true;
    }

    private void FixedUpdate()
    {
        // Ensure proper damping values based on whether the player is grounded.
        if (playerData != null && playerData.isGrounded)
        {
            rb.linearDamping = playerData.drag;
            rb.angularDamping = playerData.angularDrag;
            
            // Reset the slam flag upon landing.
            playerData.hasSlammed = false;
        }
        else
        {
            rb.linearDamping = 0f;
            rb.angularDamping = 0f;
        }

        // If the player is grounded, re-enable controls and apply movement force.
        if (playerData != null && playerData.isGrounded)
        {
            playerData.controlsEnabled = true;
            Vector3 force = new Vector3(moveInput.x, 0f, moveInput.y) * playerData.moveSpeed;
            rb.AddForce(force);
        }

        // Always apply torque for air control.
        Vector3 torque = new Vector3(moveInput.y, 0f, -moveInput.x) * playerData.airTorque;
        rb.AddTorque(torque);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground") && playerData != null)
        {
            playerData.isGrounded = true;
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground") && playerData != null)
        {
            playerData.isGrounded = false;
        }
    }
}
