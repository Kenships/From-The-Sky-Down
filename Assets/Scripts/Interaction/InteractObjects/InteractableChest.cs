using PrimeTween;
using UnityEngine;

namespace Interaction.InteractObjects
{
    public class InteractableChest : HoldToInteractObjectBase
    {
        [SerializeField] ParticleSystem particle;

        [SerializeField] private Transform chestLid;

        private Tween _shakeTween;
        
        private void Start()
        {
            _timer.OnTimerEnd += () =>
            {
                particle.Play();
                _shakeTween.Stop();
                OpenChest();
            };
        }

        public override void Interact()
        {
            base.Interact();
            _shakeTween = ShakeChest(-1);
        }

        public override void CancelInteract()
        {
            base.CancelInteract();
            _shakeTween.Stop();
        }

        private Tween ShakeChest(int cycles)
        {
            return Tween.ShakeLocalPosition(
                target: transform,
                strength: new Vector3(0.1f, 0.1f, 0.1f),
                duration: 1f,
                frequency: 10f,
                easeBetweenShakes: Ease.InOutElastic,
                cycles: cycles
            );
        }
        
        private Sequence OpenChest()
        {
            
            return Tween.LocalRotation(
                target: chestLid.transform,
                endValue: new Vector3(60, 0, 0),
                startValue: new Vector3(0, 0, 0),
                duration: 0.3f,
                ease: Ease.InExpo
            ).Chain(
                Tween.Delay(1f)
            ).Chain(
                Tween.LocalRotation(
                    target: chestLid.transform,
                    endValue: new Vector3(0, 0, 0),
                    startValue: new Vector3(60, 0, 0),
                    duration: 0.3f,
                    ease: Ease.OutBounce
                )
            );
        }
    }
}
