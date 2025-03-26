using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputHandler : MonoBehaviour
{
    private PlayerController playerController;
    private PlayerInput playerInput;
    
    public void Initialize(PlayerController controller)
    {
        playerController = controller;
        playerInput = GetComponent<PlayerInput>();
        
        if (playerInput != null)
        {
            // Connect all the input actions to the controller methods
            playerInput.actions["Move"].performed += ctx => playerController.OnMovement(ctx);
            playerInput.actions["Move"].canceled += ctx => playerController.OnMovement(ctx);
            
            playerInput.actions["Jump"].performed += ctx => playerController.OnJump(ctx);
            playerInput.actions["Jump"].canceled += ctx => playerController.OnJump(ctx);
            
            playerInput.actions["Slam"].performed += ctx => playerController.OnSlam(ctx);
            playerInput.actions["Slam"].canceled += ctx => playerController.OnSlam(ctx);
            
            playerInput.actions["QuickBreak"].performed += ctx => playerController.OnQuickBreak(ctx);
            playerInput.actions["QuickBreak"].canceled += ctx => playerController.OnQuickBreak(ctx);
            
            playerInput.actions["ChargeRoll"].performed += ctx => playerController.OnChargeRoll(ctx);
            playerInput.actions["ChargeRoll"].canceled += ctx => playerController.OnChargeRoll(ctx);
            
            playerInput.actions["RotateCharge"].performed += ctx => playerController.OnRotateChargeDirection(ctx);
            playerInput.actions["RotateCharge"].canceled += ctx => playerController.OnRotateChargeDirection(ctx);
            
            playerInput.actions["Burst"].performed += ctx => playerController.OnBurst(ctx);
            playerInput.actions["Burst"].canceled += ctx => playerController.OnBurst(ctx);
        }
        else
        {
            Debug.LogError("No PlayerInput component found on the GameObject");
        }
    }
    
    private void OnDestroy()
    {
        // Clean up event handlers when destroyed to prevent memory leaks
        if (playerInput != null)
        {
            playerInput.actions["Move"].performed -= ctx => playerController.OnMovement(ctx);
            playerInput.actions["Move"].canceled -= ctx => playerController.OnMovement(ctx);
            
            playerInput.actions["Jump"].performed -= ctx => playerController.OnJump(ctx);
            playerInput.actions["Jump"].canceled -= ctx => playerController.OnJump(ctx);
            
            playerInput.actions["Slam"].performed -= ctx => playerController.OnSlam(ctx);
            playerInput.actions["Slam"].canceled -= ctx => playerController.OnSlam(ctx);
            
            playerInput.actions["QuickBreak"].performed -= ctx => playerController.OnQuickBreak(ctx);
            playerInput.actions["QuickBreak"].canceled -= ctx => playerController.OnQuickBreak(ctx);
            
            playerInput.actions["ChargeRoll"].performed -= ctx => playerController.OnChargeRoll(ctx);
            playerInput.actions["ChargeRoll"].canceled -= ctx => playerController.OnChargeRoll(ctx);
            
            playerInput.actions["RotateCharge"].performed -= ctx => playerController.OnRotateChargeDirection(ctx);
            playerInput.actions["RotateCharge"].canceled -= ctx => playerController.OnRotateChargeDirection(ctx);
            
            playerInput.actions["Burst"].performed -= ctx => playerController.OnBurst(ctx);
            playerInput.actions["Burst"].canceled -= ctx => playerController.OnBurst(ctx);
        }
    }
}