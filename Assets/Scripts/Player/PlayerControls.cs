using System.Collections;
using GASHAPWN.Audio;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour
{
    private Rigidbody rb;
    private Vector2 moveInput;
    private Vector2 rotationInput;

    [SerializeField] private PlayerData playerData;
    [SerializeField] private float groundCheckDistance = 0.6f; // Slightly more than sphere radius
    [SerializeField] private LayerMask groundLayer; // Set this in inspector to your ground layer

    private ChargeRollIndicator chargeIndicator;
    private float chargeStartTime;
    private Quaternion chargeRotation;

    // Reference to the initial position for quick break defensive stance
    private Vector3 quickBreakPosition;
    private bool isInDefensiveStance = false;

    private void Awake()
    {
        Physics.gravity = new Vector3(0, -9.81f * 3f, 0);

        rb = GetComponent<Rigidbody>();
        chargeIndicator = GetComponentInChildren<ChargeRollIndicator>();

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
        // Always store the input value
        moveInput = context.ReadValue<Vector2>();

        // Check if player is in defensive stance and trying to move
        if (isInDefensiveStance && moveInput.sqrMagnitude > 0.1f)
        {
            EndDefensiveStance();
        }

        // Don't override input processing if charging
        if (playerData.isCharging)
        {
            moveInput = Vector2.zero;
        }
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        if (playerData != null && (!playerData.controlsEnabled || playerData.isCharging))
            return;

        // Exit defensive stance if jump is pressed
        if (isInDefensiveStance && context.performed)
        {
            EndDefensiveStance();
            return;
        }

        if (context.performed && playerData != null && playerData.isGrounded)
        {
            if (playerData.currentStamina >= 1f)
            {
                rb.AddForce(Vector3.up * playerData.jumpForce, ForceMode.Impulse);
                playerData.currentStamina -= 1f;
                playerData.OnStaminaChanged.Invoke(playerData.currentStamina);
                GAME_SFXManager.Instance.Play_Jump(this.transform);
            }
            else
            {
                playerData.OnLowStamina.Invoke(playerData.currentStamina);
                Debug.Log("Not enough stamina to jump");
            }
        }
    }

    public void OnSlam(InputAction.CallbackContext context)
    {
        if (playerData == null || !playerData.controlsEnabled || playerData.isCharging)
            return;

        // Exit defensive stance if slam is pressed
        if (isInDefensiveStance && context.performed)
        {
            EndDefensiveStance();
            return;
        }

        if (context.performed && !playerData.isGrounded && !playerData.hasSlammed)
        {
            if (playerData.currentStamina >= 1f)
            {
                StartCoroutine(SlamCoroutine());
                playerData.currentStamina -= 1f;
                playerData.OnStaminaChanged.Invoke(playerData.currentStamina);

                // Activate attack boost for slam
                playerData.ActivateAttackBoost(1.0f, 1.5f); // 1 second of 50% damage boost
            }
            else
            {
                playerData.OnLowStamina.Invoke(playerData.currentStamina);
                Debug.Log("Not enough stamina to slam");
            }
        }
    }

    private IEnumerator SlamCoroutine()
    {
        playerData.controlsEnabled = false;

        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        rb.useGravity = false;

        yield return new WaitForSeconds(playerData.slamAirborneTime);
        GAME_SFXManager.Instance.Play_Drop(this.transform);
        rb.useGravity = true;
        rb.AddForce(Vector3.down * playerData.slamForce, ForceMode.Impulse);

        playerData.hasSlammed = true;
        playerData.UpdateAbilityFlags();
    }

    public void OnQuickBreak(InputAction.CallbackContext context)
    {
        if (playerData == null || !playerData.controlsEnabled || playerData.isCharging || !playerData.isGrounded)
            return;

        // Exit defensive stance if another quickbreak is pressed
        if (isInDefensiveStance && context.performed)
        {
            EndDefensiveStance();
            return;
        }

        if (context.performed)
        {
            if (playerData.currentStamina >= 2f)
            {
                StartCoroutine(QuickBreakCoroutine());
                playerData.currentStamina -= 2f;
                playerData.OnStaminaChanged.Invoke(playerData.currentStamina);

                // Activate defensive buff after quick break - This line was only in the 'main' branch part of the conflict marker,
                // but it logically belongs here when the QuickBreak is initiated, not in the marker itself.
                // Assuming it was intended to be part of the Quick Break action:
                // playerData.ActivateDefense(playerData.quickBreakDefenseDuration, 0.5f); // 50% damage reduction
                // --- However, the coroutine seems to handle the final defensive stance activation.
                // --- Let's keep the original structure where the coroutine handles the final state.
            }
            else
            {
                playerData.OnLowStamina.Invoke(playerData.currentStamina);
                Debug.Log("Not enough stamina for quick break");
            }
        }
    }

    private IEnumerator QuickBreakCoroutine()
    {
        playerData.controlsEnabled = false;

        Vector3 initialVelocity = rb.linearVelocity;
        Vector3 initialAngularVelocity = rb.angularVelocity;

        Quaternion initialRotation = transform.rotation;
        Quaternion targetRotation = Quaternion.FromToRotation(-transform.up, Vector3.up) * transform.rotation;

        float elapsed = 0f;
        while (elapsed < playerData.quickBreakDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / playerData.quickBreakDuration);

            rb.linearVelocity = Vector3.Lerp(initialVelocity, Vector3.zero, t);
            rb.angularVelocity = Vector3.Lerp(initialAngularVelocity, Vector3.zero, t);

            transform.rotation = Quaternion.Slerp(initialRotation, targetRotation, t);

            yield return null;
        }

        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        transform.rotation = targetRotation;

        // Store position for defensive stance
        quickBreakPosition = transform.position;

        // Enter defensive stance (100% damage reduction)
        isInDefensiveStance = true;
        playerData.ActivateDefense(playerData.quickBreakDefenseDuration, 1.0f); // Apply full defense here

        // Re-enable player control so they can move to exit the stance
        playerData.controlsEnabled = true;
    }

    // Method to end defensive stance when player moves
    private void EndDefensiveStance()
    {
        if (!isInDefensiveStance)
            return;

        Debug.Log("Exited defensive stance due to movement");

        isInDefensiveStance = false;
        playerData.defenseTimer = 0f; // Assuming PlayerData handles deactivation based on timer or this call
        playerData.damageReduction = 0f; // Explicitly reset reduction
        playerData.OnDefenseDeactivated.Invoke();
        playerData.UpdateAbilityFlags(); // Update flags after stance ends
    }

    public void OnRotateChargeDirection(InputAction.CallbackContext context)
    {
        if (playerData.isCharging)
            rotationInput = context.ReadValue<Vector2>();
    }

    public void OnChargeRoll(InputAction.CallbackContext context)
    {
        if (context.started && !playerData.isCharging && !playerData.hasCharged && playerData.controlsEnabled)
        {
            // Exit defensive stance if charge roll is pressed
            if (isInDefensiveStance)
            {
                EndDefensiveStance();
                return;
            }

            if (playerData.currentStamina >= 4f)
            {
                playerData.currentStamina -= 4f;
                playerData.OnStaminaChanged.Invoke(playerData.currentStamina);
                StartCoroutine(ChargeRollCoroutine());
            }
            else
            {
                playerData.OnLowStamina.Invoke(playerData.currentStamina);
                Debug.Log("Not enough stamina to charge");
            }
        }
        else if (context.canceled && playerData.isCharging)
        {
            ReleaseCharge();
        }
    }

    private IEnumerator ChargeRollCoroutine()
    {
        playerData.isCharging = true;
        playerData.controlsEnabled = false;
        playerData.OnChargeRoll.Invoke(playerData.isCharging);

        Vector3 baseDirection = new Vector3(transform.forward.x, 0, transform.forward.z).normalized;
        chargeRotation = Quaternion.LookRotation(baseDirection);

        bool originalGravity = rb.useGravity;
        float originalDrag = rb.linearDamping;
        float originalAngularDrag = rb.angularDamping;

        float stopDuration = 0.3f;
        float elapsed = 0f;
        Vector3 initialVelocity = rb.linearVelocity;
        Vector3 initialAngularVelocity = rb.angularVelocity;
        while (elapsed < stopDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / stopDuration;
            rb.linearVelocity = Vector3.Lerp(initialVelocity, Vector3.zero, t);
            rb.angularVelocity = Vector3.Lerp(initialAngularVelocity, Vector3.zero, t);
            yield return null;
        }

        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        rb.useGravity = false;
        rb.linearDamping = 5f;
        rb.angularDamping = 5f;
        chargeStartTime = Time.time;

        while (playerData.isCharging)
        {
            float chargeTime = Time.time - chargeStartTime;
            float chargePercent = Mathf.Clamp01(chargeTime / playerData.chargeRollMaxDuration);

            float yRotation = rotationInput.x * 100f * Time.deltaTime;
            float xzRotation = rotationInput.y * 100f * Time.deltaTime;
            chargeRotation *= Quaternion.Euler(xzRotation, yRotation, 0f);

            Vector3 targetDirection = chargeRotation * Vector3.forward;
            Vector3 torqueDirection = Vector3.Cross(transform.forward, targetDirection);
            rb.AddTorque(torqueDirection * playerData.chargeRollSpinSpeed * chargePercent);

            chargeIndicator.UpdateIndicator(chargePercent, targetDirection);
            yield return null;
        }

        rb.useGravity = originalGravity;
        rb.linearDamping = originalDrag;
        rb.angularDamping = originalAngularDrag;
        chargeIndicator.HideIndicator();
        playerData.controlsEnabled = true;
        playerData.hasCharged = true;

        // Activate attack boost after charge roll
        float chargeBoost = Mathf.Lerp(1.2f, 2.0f, Mathf.Clamp01((Time.time - chargeStartTime) / playerData.chargeRollMaxDuration));
        playerData.ActivateAttackBoost(playerData.chargeRollAttackBoostDuration, chargeBoost);
    }

    private void ReleaseCharge()
    {
        playerData.isCharging = false;
        playerData.OnChargeRoll.Invoke(playerData.isCharging);
        float chargeDuration = Time.time - chargeStartTime;
        float chargePercent = Mathf.Clamp01(chargeDuration / playerData.chargeRollMaxDuration);

        transform.rotation = chargeRotation;

        float forceMagnitude = Mathf.Lerp(playerData.chargeRollMinForce, playerData.chargeRollMaxForce, chargePercent);

        rb.AddForce(transform.forward * forceMagnitude, ForceMode.Impulse);
    }

    public void OnBurst(InputAction.CallbackContext context)
    {
        if (playerData == null || !playerData.controlsEnabled || playerData.isCharging)
            return;

        // Exit defensive stance if burst is pressed
        if (isInDefensiveStance && context.performed)
        {
            EndDefensiveStance();
            return;
        }

        if (context.performed)
        {
            if (playerData.currentStamina >= 6f)
            {
                StartCoroutine(BurstCoroutine());
                playerData.currentStamina -= 6f;
                playerData.OnStaminaChanged.Invoke(playerData.currentStamina);
            }
            else
            {
                playerData.OnLowStamina.Invoke(playerData.currentStamina);
                Debug.Log("Not enough stamina for burst");
            }
        }
    }

    private IEnumerator BurstCoroutine()
    {
        playerData.controlsEnabled = false;

        // Activate invincibility for burst
        playerData.ActivateBurstInvincibility();

        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        rb.AddForce(Vector3.up * 20f, ForceMode.Impulse);

        Quaternion targetRotation = Quaternion.FromToRotation(transform.up, Vector3.up) * transform.rotation;
        float rotationDuration = 0.5f;
        float elapsed = 0f;
        while (elapsed < rotationDuration)
        {
            elapsed += Time.deltaTime;
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, elapsed / rotationDuration);
            yield return null;
        }

        yield return new WaitForSeconds(0.2f);

        float shockwaveRadius = 20f;
        float knockbackForce = 80f;
        Collider[] colliders = Physics.OverlapSphere(transform.position, shockwaveRadius);
        foreach (Collider col in colliders)
        {
            if ((col.CompareTag("Player1") || col.CompareTag("Player2")) && col.gameObject != gameObject)
            {
                Rigidbody otherRb = col.GetComponent<Rigidbody>();
                if (otherRb != null)
                {
                    Vector3 direction = (otherRb.position - transform.position).normalized;
                    otherRb.AddForce(direction * knockbackForce, ForceMode.Impulse);
                }
            }
        }

        yield return new WaitForSeconds(0.5f);
        playerData.controlsEnabled = true;
    }

    private void FixedUpdate()
    {
        if (playerData == null)
            return;

        playerData.isGrounded = IsGrounded();

        // --- RESOLVED CONFLICT AREA START ---

        // Check defensive stance first (from HEAD)
        if (isInDefensiveStance)
        {
            // Keep the player in place when in defensive stance
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            transform.position = quickBreakPosition; // Lock position
            return; // Exit FixedUpdate early if in defensive stance
        }

        // Play slam sound if isGrounded and hasSlammed (from main)
        // This check needs to happen *before* hasSlammed is reset below.
        if (playerData.isGrounded && playerData.hasSlammed)
        {
            GAME_SFXManager.Instance.Play_SlamImpact(this.transform);
            // Note: hasSlammed is reset later in this function when grounded.
        }

        // --- RESOLVED CONFLICT AREA END ---

        if (playerData.isGrounded && !playerData.isCharging)
        {
            rb.linearDamping = playerData.drag;
            rb.angularDamping = playerData.angularDrag;
            playerData.controlsEnabled = true;
            playerData.hasSlammed = false; // Reset slam state now that we've landed (and potentially played the sound)
            playerData.hasCharged = false;
            Vector3 force = new Vector3(moveInput.x, 0f, moveInput.y) * playerData.moveSpeed;
            rb.AddForce(force);

            // Regenerate stamina only when moving on the ground
            if (moveInput.sqrMagnitude > 0.01f)
            {
                playerData.currentStamina += playerData.staminaRegenRate * Time.fixedDeltaTime;
                playerData.currentStamina = Mathf.Clamp(playerData.currentStamina, 0f, playerData.maxStamina);
                playerData.OnStaminaChanged.Invoke(playerData.currentStamina);
            }
        }
        else // Player is airborne or charging
        {
            // Reduce drag when airborne or charging
            rb.linearDamping = 0f;
            rb.angularDamping = 0f;
        }

        // Apply air/ground torque based on movement input (even if not moving forward, allows spinning)
        // Note: This torque applies even when charging according to this logic placement.
        Vector3 torque = new Vector3(moveInput.y, 0f, -moveInput.x) * playerData.airTorque; // Might want separate ground/air torque values
        rb.AddTorque(torque);
    }

    private bool IsGrounded()
    {
        Ray ray = new Ray(transform.position, Vector3.down);
        Debug.DrawRay(transform.position, Vector3.down * groundCheckDistance, Color.red);
        return Physics.Raycast(ray, groundCheckDistance, groundLayer);
    }
}