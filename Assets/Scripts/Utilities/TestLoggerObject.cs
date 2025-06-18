
using UnityEngine;
using ImprovedTimers;

namespace Utilities
{
    public class TestLoggerObject : MonoBehaviour
    {
        private CountdownTimer _timer;
        public void Awake()
        {
            _timer = new CountdownTimer(1f);
            //timer.Loop();
            _timer.OnTimerEnd += () => Logger.Log("Timer started");
        }
        public void Update()
        {
            _timer.Tick();
        }
    }
}
