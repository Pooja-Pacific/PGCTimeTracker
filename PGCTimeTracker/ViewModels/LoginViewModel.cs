using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Threading;
using PGCTimeTracker.Helpers;
using PGCTimeTracker.Models;
using PGCTimeTracker.Services;
using ReactiveUI;
using System;
using System.Threading.Tasks;
using System.Windows.Input;
using static PGCTimeTracker.Services.IdleDataManager;

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

        TryAutoLoginAsync();
        var LogOutDetails = IdleDataManager.GetDataFromFile<UserLogoutVM>(DateTime.UtcNow.Date,FileType.ShutdownLog);
        if (LogOutDetails.UserId != 0)
        {
            _ = IdleDataManager.SaveLogoutTime(new LogoutTimeVM { LogOutTime = LogOutDetails.LogOutTime });
            //    Username = savedCreds.UserName ?? string.Empty;
            //    Password = savedCreds.Password ?? string.Empty;
                        
            UpdateCanLogin();

            // 🔹 Optional, auto-login if creds are found
            if (CanLogin)
            {
                _ = PerformLoginAsync();
            }
        }
    }
    private void UpdateCanLogin()
    {
        CanLogin = !string.IsNullOrWhiteSpace(Username) && !string.IsNullOrWhiteSpace(Password);
    }
    private async Task PerformLoginAsync()
    {
        CanLogin = false;
        ErrorMessage = string.Empty;

        try
        {
            if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(Password))
            {
                await CommonExtension.ShowMessageAsync("Email address and password are required", "Incorrect Login Credentials");
                return;
            }

            await SetUserTokenWithCred(Username, Password);

            await SetUserDetails();

            //var logoutDetails = IdleDataManager.GetUserCredentials();
            var logoutDetails = IdleDataManager.GetDataFromFile<UserLogoutVM>(DateTime.UtcNow.Date, FileType.ShutdownLog);
            if (logoutDetails.UserId > 0)
                _ = IdleDataManager.SaveLogoutTime(new LogoutTimeVM {LogOutTime=logoutDetails.LogOutTime});

            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                if (App.Current.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
                {
                    var idleForm = new IdleWindow(Username);
                    idleForm.Show();
                    desktop.MainWindow?.Close();
                    desktop.MainWindow = idleForm;
                }
            });
        }
        catch (Exception ex)
        {
            IdleDataManager.ErrorToFile(ex, FileType.SystemLog);
        }
        finally
        {
            CanLogin = true;
            UpdateCanLogin();
        }
    }
    private async Task SetUserTokenWithCred(string username, string password)
    {
        try
        {
            var token = await CommonExtension.GetUserTokenByApi(username, password);
            if (!string.IsNullOrEmpty(token))
            {
                var userCred = new UserCredentials
                {
                    UserName = username,
                    Password = password
                };
                CommonExtension.TokenKey = token;
                IdleDataManager.SaveDataToFile(userCred, IdleDataManager.FileType.Credentials);
            }
            IdleDataManager.ErrorToFile($"Login failed. Please check your credentials or network.", IdleDataManager.FileType.SystemLog);
        }
        catch (Exception ex)
        {
            IdleDataManager.ErrorToFile(ex, IdleDataManager.FileType.SystemLog);
        }
    }
    private async Task SetUserDetails()=>IdleDataManager.configData=await CommonExtension.GetUserDetails();
    private async void TryAutoLoginAsync()
    {
        var userCredentials = IdleDataManager.GetUserCredentials();
        if (userCredentials != null &&
            !string.IsNullOrWhiteSpace(userCredentials.UserName) &&
            !string.IsNullOrWhiteSpace(userCredentials.Password))
        {
            Username = userCredentials.UserName;
            Password = userCredentials.Password;

            await PerformLoginAsync();
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