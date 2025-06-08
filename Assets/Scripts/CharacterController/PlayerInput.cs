using System;
using CharacterController;
using Obvious.Soap;
using Unity.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
public class PlayerInput : MonoBehaviour
{
    [SerializeField] private InputEventsSO inputEvents;
    private InputSystem_Actions _inputSystem;
    [SerializeField] private ScriptableEventNoParam toggleActionMapChange;
    [SerializeField] private ScriptableEventNoParam requestNextDialogue;
    [ReadOnly, SerializeField] private string currentActionMap;
    

    private void OnEnable()
    {
        _inputSystem = new InputSystem_Actions();
        _inputSystem.Player.Enable();
        currentActionMap = _inputSystem.Player.ToString();

        toggleActionMapChange.OnRaised += ToggleActionMapChange;
        
        _inputSystem.UI.Click.performed += _ => requestNextDialogue.Raise();
        _inputSystem.UI.Submit.performed += _ => requestNextDialogue.Raise();

        _inputSystem.Player.Move.performed += SetDirection;
        _inputSystem.Player.Move.canceled += SetDirection;
        _inputSystem.Player.Jump.started += Jump;
        _inputSystem.Player.Dash.started += Dash;
        _inputSystem.Player.BulletJump.started += BulletJump;
    }

    private void OnDisable()
    {
        _inputSystem.Player.Move.performed -= SetDirection;
        _inputSystem.Player.Move.canceled -= SetDirection;
        _inputSystem.Player.Jump.started -= Jump;
        _inputSystem.Player.Dash.started -= Dash;
        _inputSystem.Player.BulletJump.started -= BulletJump;

        _inputSystem.Player.Disable();
    }
    
    private void ToggleActionMapChange()
    {
        if (_inputSystem.Player.enabled)
        {
            _inputSystem.Player.Disable();
            _inputSystem.UI.Enable();
            currentActionMap = _inputSystem.UI.ToString();
        }
        else
        {
            _inputSystem.UI.Disable();
            _inputSystem.Player.Enable();
            currentActionMap = _inputSystem.Player.ToString();
        }
    }

    private void Dash(InputAction.CallbackContext obj)
    {
        inputEvents.dashEvent.Raise();
    }

    private void Jump(InputAction.CallbackContext obj)
    {
        inputEvents.jumpEvent.Raise();
    }

    private void BulletJump(InputAction.CallbackContext obj)
    {
        inputEvents.bulletJumpEvent.Raise();
    }

    private void SetDirection(InputAction.CallbackContext obj)
    {
        inputEvents.inputDirection.Value = obj.ReadValue<Vector2>();
    }
    
    
}
