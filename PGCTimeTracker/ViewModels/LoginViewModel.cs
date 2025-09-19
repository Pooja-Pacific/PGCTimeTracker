using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Threading;
using PGCTimeTracker.Helpers;
using PGCTimeTracker.Models;
using ReactiveUI;
using System;
using System.Reactive;
using System.Threading.Tasks;
using System.Windows.Input;

namespace PGCTimeTracker.ViewModels;

public class LoginViewModel : ReactiveObject
{
    private string _username = string.Empty;
    private string _password = string.Empty;
    private bool _showError;
    private string _errorMessage = string.Empty;
    private bool _canLogin;
    private char _passwordChar = '●';
    private bool _isPasswordVisible = false;

    public string Username  
    {
        get => _username;
        set
        {
            this.RaiseAndSetIfChanged(ref _username, value);
            UpdateCanLogin();
        }
    }

    public string Password
    {
        get => _password;
        set
        {
            this.RaiseAndSetIfChanged(ref _password, value);
            UpdateCanLogin();
        }
    }

    public bool ShowError
    {
        get => _showError;
        set => this.RaiseAndSetIfChanged(ref _showError, value);
    }

    public string ErrorMessage
    {
        get => _errorMessage;
        set => this.RaiseAndSetIfChanged(ref _errorMessage, value);
    }

    public bool CanLogin
    {
        get => _canLogin;
        set => this.RaiseAndSetIfChanged(ref _canLogin, value);
    }
    public char PasswordChar
    {
        get => _passwordChar;
        set => this.RaiseAndSetIfChanged(ref _passwordChar, value);
    }

    public ICommand LoginCommand { get; }
    public ICommand TogglePasswordVisibilityCommand { get; }
    public LoginViewModel()
    {
        LoginCommand = new RelayCommand(async () => await PerformLoginAsync());

        TogglePasswordVisibilityCommand = new RelayCommand(() =>
        {
            _isPasswordVisible = !_isPasswordVisible;
            PasswordChar = _isPasswordVisible ? '\0' : '●';
            return Task.CompletedTask;
        });

    }

    private void UpdateCanLogin()
    {
        CanLogin = !string.IsNullOrWhiteSpace(Username) && !string.IsNullOrWhiteSpace(Password);
    }

    private async Task PerformLoginAsync()
    {
        CanLogin = false;
        ShowError = false;
        ErrorMessage = string.Empty;

        try
        {
            string token;
            try
            {
                token = await CommonExtension.GetUserTokenByApi(Username, Password);
            }
            catch
            {
                token = null;
            }

            if (string.IsNullOrEmpty(token))
            {
                ShowError = true;
                ErrorMessage = "Login failed. Please check your credentials or network.";
                return;
            }

            CommonExtension.TokenKey = token;

            var userDetails = await CommonExtension.ExcuteAsync<object, LoginResponseVM>(
                null,
                UrlConstants.GetUserDetail,
                RequestType.GET,
                CommonExtension.TokenKey??string.Empty);

            if (userDetails?.ResponseStatus != ResponseStatuses.Success)
            {
                ShowError = true;
                ErrorMessage = userDetails?.Message ?? "Failed to fetch user details.";
                return;
            }

            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                if (App.Current.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
                {
                    var idleForm = new IdleWindow(_username);
                    idleForm.Show();
                    desktop.MainWindow?.Close();
                    desktop.MainWindow = idleForm;
                }
            });
        }
        finally
        {
            CanLogin = true;
            UpdateCanLogin();
        }
    }

    private class RelayCommand : ICommand
    {
        private readonly Func<Task> _execute;

        public RelayCommand(Func<Task> execute)
        {
            _execute = execute;
        }

        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter) => true;

        public async void Execute(object parameter) => await _execute();
    }
}