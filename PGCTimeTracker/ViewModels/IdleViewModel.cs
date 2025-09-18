using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PGCTimeTracker.ViewModels
{
    public class IdleViewModel:ReactiveObject
    {
        private string _welcomeMessage;
        public string WelcomeMessage
        {
            get => _welcomeMessage;
            set => this.RaiseAndSetIfChanged(ref _welcomeMessage, value);
        }

        public IdleViewModel(string username)
        {
            WelcomeMessage = $"Welcome, {username}!";
        }
    }
}
