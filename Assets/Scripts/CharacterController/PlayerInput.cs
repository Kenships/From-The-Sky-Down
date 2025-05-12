using System;
using Obvious.Soap;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInput : MonoBehaviour
{
    [SerializeField] private Vector2Variable movementDirection;
    [SerializeField] private ScriptableEventNoParam jumpEvent;
    private InputSystem_Actions inputSystem;
    

    private void Awake()
    {
        inputSystem = new InputSystem_Actions();
        inputSystem.Player.Enable();

        inputSystem.Player.Move.performed += SetDirection;
        inputSystem.Player.Move.canceled += SetDirection;
        inputSystem.Player.Jump.started += Jump;
    }

    private void Jump(InputAction.CallbackContext obj)
    {
        jumpEvent.Raise();
    }

    private void SetDirection(InputAction.CallbackContext obj)
    {
        movementDirection.Value = obj.ReadValue<Vector2>().normalized;
    }
    
    
}
