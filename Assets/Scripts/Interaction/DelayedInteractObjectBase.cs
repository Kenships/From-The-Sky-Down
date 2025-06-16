using System;
using DefaultNamespace;
using ImprovedTimers;
using Obvious.Soap;
using UnityEngine;
using UnityEngine.Serialization;

namespace Interaction
{
    public abstract class DelayedInteractObjectBase : InteractObjectBase
    {
        [SerializeField] private ProgressVisual visual;
        [SerializeField] private float startInteractDuration;
        protected DecayTimer _timer;
        
        private void Awake()
        {
            if (startInteractDuration < 0) return;
            _timer = new DecayTimer(startInteractDuration);
            visual.Initialize(_timer);
        }
        
    }
}
