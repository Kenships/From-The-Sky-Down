using ImprovedTimers;
using UnityEngine;

namespace Interaction.InteractionMenu
{
    public class ProgressVisual : MonoBehaviour
    {
        private Timer _timer;
        
        public void Initialize(Timer timer)
        {
            _timer = timer;
        }

        private void Update()
        {
            if (!_timer.IsFinished && _timer.CurrentTime > 0)
            {
                Debug.Log(_timer.Progress);
            }
        }
    }
}
