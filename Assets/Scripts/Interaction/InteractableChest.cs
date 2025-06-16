using UnityEngine;

namespace Interaction
{
    public class InteractableChest : DelayedInteractObjectBase
    {
        private void Start()
        {
            _timer.OnTimerEnd += () =>
            {
                Debug.Log("InteractableChest");
            };
        }

        public override void Interact()
        {
            _timer.Start();
        }

        public override void CancelInteract()
        {
            _timer.Pause();
        }
    }
}
