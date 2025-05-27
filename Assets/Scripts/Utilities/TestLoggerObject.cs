using System;
using UnityEngine;
using ImprovedTimers;

namespace Utilities
{
    public class TestLoggerObject : MonoBehaviour
    {
        private CountdownTimer timer;
        public void Awake()
        {
            timer = new CountdownTimer(1f);
            //timer.Loop();
            timer.OnTimerEnd += () => Logger.Log("Timer started");
        }
        public void Update()
        {
            timer.Tick();
        }
    }
}