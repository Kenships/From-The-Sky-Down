using System;
using UnityEngine;

namespace Utilities
{
    public class TestLoggerObject : MonoBehaviour
    {
        private Timer timer;
        public void Awake()
        {
            timer = new Timer(1f);
            timer.Loop();
            timer.OnTimerEnd += () => Logger.Log("Timer started");
        }
        public void Update()
        {
            timer.Tick(Time.deltaTime);
        }
    }
}