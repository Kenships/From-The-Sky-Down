using System;
using CharacterController;
using Obvious.Soap;
using UnityEngine;
using UnityEngine.InputSystem;
public class PlayerInput : MonoBehaviour
{
    [SerializeField] private InputEventsSO inputEvents;
    private InputSystem_Actions inputSystem;
    

    private void OnEnable()
    {
        inputSystem = new InputSystem_Actions();
        inputSystem.Player.Enable();

        inputSystem.Player.Move.performed += SetDirection;
        inputSystem.Player.Move.canceled += SetDirection;
        inputSystem.Player.Jump.started += Jump;
        inputSystem.Player.Dash.started += Dash;
    }

    private void OnDisable()
    {
        inputSystem.Player.Move.performed -= SetDirection;
        inputSystem.Player.Move.canceled -= SetDirection;
        inputSystem.Player.Jump.started -= Jump;
        inputSystem.Player.Dash.started -= Dash;

        inputSystem.Player.Disable();
    }

    private void Dash(InputAction.CallbackContext obj)
    {
        inputEvents.dashEvent.Raise();
    }

    private void Jump(InputAction.CallbackContext obj)
    {
        inputEvents.jumpEvent.Raise();
    }

    private void SetDirection(InputAction.CallbackContext obj)
    {
        inputEvents.inputDirection.Value = obj.ReadValue<Vector2>();
    }
    
    
}
