using Player.Input;
using Unity.Cinemachine;
using UnityEngine;

namespace CameraControl.Cinemachine
{
    public class MouseSensitivityControlledCinemachineInputAxisController : CinemachineInputAxisController
    {
        [SerializeField] private MouseInputSettingsSO settings;

        [SerializeField] private InputReaderSO inputReader;

        private void Awake()
        {
            Controllers[0].Input.Gain = settings.MouseSensitivityX;
            Controllers[1].Input.Gain = -settings.MouseSensitivityY;
        }

        private void Start()
        {
            inputReader.OnStateChange += OnStateChange;
        }

        private void OnStateChange(InputState state)
        {
            enabled = state == InputState.Movement;
        }
    }
}
