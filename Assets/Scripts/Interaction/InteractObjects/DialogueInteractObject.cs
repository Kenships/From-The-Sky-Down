using PrimeTween;
using UI.Interfaces;
using UnityEngine;

namespace Interaction.InteractObjects
{
    public class DialogueInteractObject : InteractObjectBase, IHoverable
    {
        [SerializeField] private Transform canvasTransform;
        [SerializeField] private GameObject dialoguePrefab;
        [SerializeField] private Transform hoverVisual;
        
        private GameObject _dialogue;
        
        public override void Interact()
        {
            base.Interact();
            Instantiate(dialoguePrefab, canvasTransform);
        }

        public void HoverSelect()
        {
            Tween.Scale(hoverVisual,
                endValue: new Vector3(1.05f, 1.05f, 1.05f),
                startValue: Vector3.one,
                duration: 0.5f,
                ease: Ease.InOutSine,
                cycles: -1,
                cycleMode: CycleMode.Yoyo
                );
        }

        public void HoverDeselect()
        {
            Tween.StopAll(hoverVisual);
            Tween.Scale(hoverVisual,
                endValue: Vector3.one,
                startValue: hoverVisual.localScale,
                duration: 0.1f,
                ease: Ease.Linear,
                cycles: 1
            );
        }
    }
}
