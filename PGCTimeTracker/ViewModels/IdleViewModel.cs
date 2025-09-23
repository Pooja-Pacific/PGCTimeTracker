using PGCTimeTracker.Services;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PGCTimeTracker.ViewModels
{
    public class IdleViewModel: ReactiveObject
    {
        private string _welcomeMessage;
        private bool _isIdle;
        public string Username { get; }
        public IdleTrackerService IdleService { get; }
        public bool IsIdle
        {
            get => _isIdle;
            set => this.RaiseAndSetIfChanged(ref _isIdle, value);
        }
        public string WelcomeMessage
        {
            get => _welcomeMessage;
            set => this.RaiseAndSetIfChanged(ref _welcomeMessage, value);
        }

        public IdleViewModel(string username)
        {
            WelcomeMessage = $"Welcome, {username}!";
            IdleService = new IdleTrackerService(TimeSpan.FromMinutes(5));

            IdleService.OnIdle += () => IsIdle = true;
            IdleService.OnResume += () => IsIdle = false;
        }
    }
}
