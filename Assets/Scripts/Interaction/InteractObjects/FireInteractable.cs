using Player.Input;
using PrimeTween;
using Unity.Cinemachine;
using UnityEngine;

namespace Interaction.InteractObjects
{
    public class FireInteractable : InteractObjectBase
    {
        [SerializeField] private GameObject fireVFX;
        [SerializeField] private Transform cameraFollowPoint;
        [SerializeField] private InputReaderSO inputReader;

        [Header("Camera Settings")] 
        [SerializeField] private float cameraDistance = 5f;
        private CinemachineCamera _itemCamera;
        
        public override void Interact()
        {
            base.Interact();
            if (!_itemCamera)
            {
                GameObject cameraObject = new GameObject("Item Camera");
                _itemCamera = cameraObject.AddComponent<CinemachineCamera>();
                CinemachineHardLookAt hardLookAt = cameraObject.AddComponent<CinemachineHardLookAt>();
                CinemachineOrbitalFollow follow = cameraObject.AddComponent<CinemachineOrbitalFollow>();
                _itemCamera.Follow = cameraFollowPoint;
                follow.Radius = cameraDistance;
            }
            
            _itemCamera.Priority = 20;
            
            
            inputReader.RequestItemInteract += ExitInteract;
            inputReader.RequestItemAltInteract += GrowFire;
            inputReader.SetInputState(InputState.Interaction);
        }

        private void GrowFire()
        {
            Debug.Log("Growing Fire");
            fireVFX.SetActive(true);
            Tween.Scale(
                target: fireVFX.transform,
                startValue: Vector3.zero,
                endValue: Vector3.one,
                duration: 2,
                ease: Ease.InExpo
            );
        }

        public void ExitInteract()
        {
            Debug.Log("Exiting Interact");
            inputReader.RequestItemInteract -= ExitInteract;
            inputReader.RequestItemAltInteract -= GrowFire;
            inputReader.SetInputState(InputState.Default);
            _itemCamera.Priority = 0;
        }
    }
}
