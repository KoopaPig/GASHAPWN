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
            StartCoroutine(SlamCoroutine());
        }
    }

    private IEnumerator SlamCoroutine()
    {
        // Disable controls during the slam.
        playerData.controlsEnabled = false;
        
        // Cancel momentum.
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        
        // Float by disabling gravity.
        rb.useGravity = false;
        
        // Wait for 1 second (player floats).
        yield return new WaitForSeconds(1f);
        
        // Re-enable gravity and apply a strong downward force.
        rb.useGravity = true;
        rb.AddForce(Vector3.down * playerData.slamForce, ForceMode.Impulse);
        
        playerData.hasSlammed = true;
    }

    // Quick Break mechanic: only available on the ground.
    public void OnQuickBreak(InputAction.CallbackContext context)
    {
        if (playerData == null || !playerData.controlsEnabled || !playerData.isGrounded)
            return;

        if (context.performed)
        {
            StartCoroutine(QuickBreakCoroutine());
        }
    }

    private IEnumerator QuickBreakCoroutine()
    {
        // Disable controls so that no other input interferes.
        playerData.controlsEnabled = false;

        // Capture the current momentum.
        Vector3 initialVelocity = rb.linearVelocity;
        Vector3 initialAngularVelocity = rb.angularVelocity;

        // Capture the current rotation and calculate the target rotation.
        // The target rotation makes the sphere's bottom (transform.down) face upward (Vector3.up).
        Quaternion initialRotation = transform.rotation;
        Quaternion targetRotation = Quaternion.FromToRotation(-transform.up, Vector3.up) * transform.rotation;

        float elapsed = 0f;
        while (elapsed < playerData.quickBreakDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / playerData.quickBreakDuration);

            // Gradually decelerate momentum.
            rb.linearVelocity = Vector3.Lerp(initialVelocity, Vector3.zero, t);
            rb.angularVelocity = Vector3.Lerp(initialAngularVelocity, Vector3.zero, t);

            // Smoothly rotate the sphere to the target orientation.
            transform.rotation = Quaternion.Slerp(initialRotation, targetRotation, t);

            yield return null;
        }

        // Ensure the final state is completely stopped and correctly rotated.
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        transform.rotation = targetRotation;

        // Re-enable controls after the quick break.
        playerData.controlsEnabled = true;
    }

    private void FixedUpdate()
    {
        if (playerData == null)
            return;

        if (playerData.isGrounded)
        {
            rb.linearDamping = playerData.drag;
            rb.angularDamping = playerData.angularDrag;
            // Optionally re-enable controls if no other mechanic is in play.
            playerData.controlsEnabled = true;
            playerData.hasSlammed = false;
            Vector3 force = new Vector3(moveInput.x, 0f, moveInput.y) * playerData.moveSpeed;
            rb.AddForce(force);
        }
        else
        {
            rb.linearDamping = 0f;
            rb.angularDamping = 0f;
        }

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
