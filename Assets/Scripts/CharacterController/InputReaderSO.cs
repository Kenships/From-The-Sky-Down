using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

namespace CharacterController
{
    [Serializable]
    public enum InputState
    {
        Movement,
        Dialogue,
        Any
    }
    
    
    [CreateAssetMenu(fileName = "InputReader", menuName = "Scriptable Object/InputReader")]
    public class InputReaderSO : ScriptableObject, InputSystem_Actions.IPlayerActions, InputSystem_Actions.IDialogueActions
    {
        //PlayerActionEvents
        public UnityAction<Vector2> RequestInputDirection;
        public UnityAction RequestJump;
        public UnityAction RequestDash;
        public UnityAction RequestBulletJump;
        public UnityAction RequestInteract;
        public UnityAction CancelInteract;
        
        private InputStateVariable _currentState;
        
        private Stack<InputState> _inputStateStack;
        
        //DialogueActionEvents
        public UnityAction RequestNextDialogue;
    
        private InputSystem_Actions _inputSystem;
        
        
        private void OnEnable()
        {
            _inputStateStack = new Stack<InputState>();
            if (_inputSystem == null)
            {
                _inputSystem = new InputSystem_Actions();
                _inputSystem.Player.SetCallbacks(this);
                _inputSystem.Dialogue.SetCallbacks(this);
            }
            if (!_currentState)
            {
                _currentState = CreateInstance<InputStateVariable>();
            }
            _inputSystem.Enable();
            _currentState.OnValueChanged += SetInputState;
            SetInputState(_currentState.Value);
        }
        private void OnDisable()
        {
            _currentState.OnValueChanged -= SetInputState;
            _inputSystem.Disable();
        }

        
        #region Player Actions
        public void OnMove(InputAction.CallbackContext context)
        {
            RequestInputDirection?.Invoke(context.ReadValue<Vector2>());
        }

        public void OnLook(InputAction.CallbackContext context)
        {
        
        }

        public void OnAttack(InputAction.CallbackContext context)
        {
        
        }

        public void OnInteract(InputAction.CallbackContext context)
        {

            if (context.started)
            {
                RequestInteract?.Invoke();
            }

            if (context.canceled)
            {
                CancelInteract?.Invoke();
            }
        }

        public void OnCrouch(InputAction.CallbackContext context)
        {
        
        }

        public void OnJump(InputAction.CallbackContext context)
        {
            if (context.started)
            {
                RequestJump?.Invoke();
            }
        }

        public void OnPrevious(InputAction.CallbackContext context)
        {
        
        }

        public void OnNext(InputAction.CallbackContext context)
        {
        
        }

        public void OnDash(InputAction.CallbackContext context)
        {
            if (context.started)
            {
                RequestDash?.Invoke();  
            }
             
        }

        public void OnBulletJump(InputAction.CallbackContext context)
        {
            if (context.started)
            {
                RequestBulletJump?.Invoke();  
            }
        }
        
        #endregion
        
        #region Dialogue Actions
        public void OnDS_Submit(InputAction.CallbackContext context)
        {
            if (context.started) RequestNextDialogue?.Invoke();
        }

        public void OnDS_Cancel(InputAction.CallbackContext context)
        {
            //This is when escape is pressed might want to do some handling of pause later on
        }

        public void OnDS_Click(InputAction.CallbackContext context)
        {
            if (context.started) RequestNextDialogue?.Invoke();
        }

        public void OnDS_RightClick(InputAction.CallbackContext context)
        {
            if (context.started) RequestNextDialogue?.Invoke();
        }

        public void OnDS_MiddleClick(InputAction.CallbackContext context)
        {
            if (context.started) RequestNextDialogue?.Invoke();
        }
        #endregion
        
        #region Utility Methods
        private void DisableAllMaps()
        {
            _inputSystem.Player.Disable();
            _inputSystem.Dialogue.Disable();
            _inputSystem.UI.Disable();
        }

        public void SetInputState(InputState state)
        {
            DisableAllMaps();
            switch (state)
            {
                case InputState.Movement:
                    _inputSystem.Player.Enable();
                    break;
                case InputState.Dialogue:
                    _inputSystem.Dialogue.Enable();
                    break;
                default:
                    _inputStateStack.Pop();
                    SetInputState(_inputStateStack.Pop());
                    return;
            }
            _inputStateStack.Push(state);
        }
        #endregion
    }
}
