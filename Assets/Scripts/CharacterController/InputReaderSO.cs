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
        Default
    }
    
    
    [CreateAssetMenu(fileName = "InputReader", menuName = "Scriptable Object/InputReader")]
    public class InputReaderSO : ScriptableObject, InputSystem_Actions.IPlayerActions, InputSystem_Actions.IDialogueActions
    {
        //PlayerActionEvents
        public UnityAction<Vector2> RequestInputDirection;
        public UnityAction<float> ScrollDirection;
        public UnityAction RequestJump;
        public UnityAction RequestDash;
        public UnityAction RequestBulletJump;
        public UnityAction RequestInteract;
        public UnityAction CancelInteract;
        
        
        //State
        private Stack<InputState> _inputStateStack;
        public UnityAction<InputState> OnStateChange;
        
        
        //DialogueActionEvents
        public UnityAction RequestNextDialogue;
        public UnityAction<float> DialogueScrollDirection;
    
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
            _inputSystem.Enable();
            
            //Ensure Empty State
            DisableAllMaps();
        }
        
        private void OnDisable()
        {
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
        
        public void OnScroll(InputAction.CallbackContext context)
        {
            //Extract Scroll Y direction
            Vector2 scrollDirection = context.ReadValue<Vector2>();
            
            if (context.performed)
            {
                ScrollDirection?.Invoke(scrollDirection.y);
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

        public void OnDS_Scroll(InputAction.CallbackContext context)
        {
            //Extract Scroll Y direction
            Vector2 scrollDirection = context.ReadValue<Vector2>();
            
            if (context.performed)
            {
                DialogueScrollDirection?.Invoke(scrollDirection.y);
            }
        }

        #endregion
        
        #region Utility Methods

        private void DisableCursor()
        {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }

        private void EnableCursor()
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }
        
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
                    DisableCursor();
                    break;
                case InputState.Dialogue:
                    _inputSystem.Dialogue.Enable();
                    EnableCursor();
                    break;
                default:
                    if (_inputStateStack.Count >= 2)
                    {
                        _inputStateStack.Pop();
                        SetInputState(_inputStateStack.Pop());
                    }
                    else if (_inputStateStack.Count == 0)
                    {
                        Debug.LogWarning("Input state stack is empty. Default action map has been set to Movement.");
                        SetInputState(InputState.Movement);
                    }
                    return;
            }
            _inputStateStack.Push(state);
            OnStateChange?.Invoke(state);
        }
        #endregion
    }
}
