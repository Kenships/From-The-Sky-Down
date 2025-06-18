using ImprovedTimers;
using Interaction.InteractionMenu;
using JetBrains.Annotations;
using UnityEngine;

namespace Interaction.InteractObjects
{
    public abstract class HoldToInteractObjectBase : InteractObjectBase
    {
        [SerializeField] [CanBeNull] private ProgressVisual visual;
        [SerializeField][Range(0, float.MaxValue)] private float startInteractDuration;
        protected DecayTimer _timer;
        
        private void Awake()
        {
            if (startInteractDuration <= 0)
            {
                Debug.LogError("Invalid Start Interact Duration. Must be a positive value greater than zero!");
                return;
            }
            _timer = new DecayTimer(startInteractDuration);
            visual?.Initialize(_timer);
        }

        public override void Interact()
        {
            base.Interact();
            _timer.Start();
        }

        public override void CancelInteract()
        {
            base.CancelInteract();
            _timer.Pause();
        }
    }
}
