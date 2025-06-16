using System;
using UnityEngine;

namespace ImprovedTimers {
    public abstract class Timer : IDisposable {
        public float CurrentTime { get; protected set; }
        public bool IsRunning { get; private set; }
        public bool IsPaused { get; private set; }

        protected float initialTime;
        

        public Action OnTimerRaised = delegate { };
        public Action OnTimerEnd = delegate { };

        protected Timer(float value) {
            initialTime = value;
        }

        public void Start() {
            
            if (IsPaused && !IsFinished)
            {
                Resume();
                return;
            }
            
            CurrentTime = initialTime;
            if (!IsRunning) {
                IsPaused = true;
                IsRunning = true;
                TimerManager.RegisterTimer(this);
                OnTimerRaised.Invoke();
            }
        }

        public void Stop() {
            if (IsRunning)
            {
                IsPaused = false;
                IsRunning = false;
                TimerManager.DeregisterTimer(this);
                OnTimerEnd.Invoke();
            }
        }

        public abstract void Tick();
        public abstract bool IsFinished { get; }
        public abstract float Progress { get; }
        public void Resume()
        {
            IsPaused = false;
            IsRunning = true;
        }

        public void Pause()
        {
            IsPaused = true;
            IsRunning = false;
        }

        public virtual void Reset()
        {
            IsPaused = false;
            CurrentTime = initialTime;
        }

        public virtual void Reset(float newTime) {
            initialTime = newTime;
            Reset();
            Start();
        }

        bool disposed;

        ~Timer() {
            Dispose(false);
        }

        // Call Dispose to ensure deregistration of the timer from the TimerManager
        // when the consumer is done with the timer or being destroyed
        public void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing) {
            if (disposed) return;

            if (disposing) {
                TimerManager.DeregisterTimer(this);
            }

            disposed = true;
        }
    }
}
