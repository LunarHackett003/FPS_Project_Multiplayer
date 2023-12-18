using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.ProBuilder.Shapes;

public class PlayerInputCollector : MonoBehaviour
{
    public static PlayerInputCollector Instance;
    private void Awake()
    {
        if (!Instance)
            Instance = this;
        else
            Destroy(this);
        pi = GetComponent<PlayerInput>();
    }
    public bool toggleCrouch, toggleSprint;
    PlayerInput pi;
    [SerializeField] protected RigidbodyPlayerMotor rpm;
    public void GetLookInput(InputAction.CallbackContext context)
    {
        rpm.SetLookInput(this, context.ReadValue<Vector2>());
    }
    public void GetMoveInput(InputAction.CallbackContext context)
    {
        rpm.SetMoveInput(this, context.ReadValue<Vector2>());
    }
    public void CrouchTapInput(InputAction.CallbackContext context)
    {
        if (!toggleCrouch)
        {
            if (context.performed)
                rpm.PressCrouch(true);

            if (context.canceled)
                rpm.PressCrouch(false);
        }
        else
        {
            if (context.performed)
                rpm.PressCrouch(!rpm.GetSlideStatus());
        }
    }
    public void SprintTapInput(InputAction.CallbackContext context)
    {
        // if (toggleSprint || pi.currentControlScheme == "Gamepad")
        // {
        //     if (context.ReadValueAsButton() == true)
        //     {
        //         rpm.Sprint();
        //     }
        // }
        // else
        // {
        //     rpm.Sprint();
        // }

        if (!toggleSprint)
        {
            if (context.performed)
                rpm.Sprint(true);
            else
                rpm.Sprint(false);
        }
        else
        {
            if (context.performed)
                rpm.Sprint();
        }
    }

    public void JumpInput(InputAction.CallbackContext context)
    {
        if(context.started)
            rpm.Jump();
    }

}
