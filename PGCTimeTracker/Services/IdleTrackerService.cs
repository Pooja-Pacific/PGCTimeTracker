using Avalonia.Threading;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PGCTimeTracker.Services
{
    public class IdleTrackerService
    {
        private readonly DispatcherTimer _dispatcherTimer;
        private DateTime _lastActivity;
        private readonly TimeSpan _idle;

        public event Action? OnIdle;
        public event Action? OnResume;
        private bool _isIdle;

        public IdleTrackerService(TimeSpan idleThreshold)
        {
            _idle = idleThreshold;
            _lastActivity = DateTime.UtcNow;

            _dispatcherTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(10) //check every 10sec
            };
            _dispatcherTimer.Tick += CheckIdle;
            _dispatcherTimer.Start();
        }
        public void UpdateActivity()
        {
            _lastActivity = DateTime.UtcNow;

            if (_isIdle)
            {
                _isIdle = false;
                OnResume?.Invoke();
            }
        }
        private void CheckIdle(object? sender, EventArgs e)
        {
            var idleTime = DateTime.UtcNow - _lastActivity;

            if (!_isIdle && idleTime >= _idle)
            {
                _isIdle = true;
                OnIdle?.Invoke();
            }
        }
    }
}
