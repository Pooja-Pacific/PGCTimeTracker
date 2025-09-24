using Newtonsoft.Json;
using PGCTimeTracker_V2.Helpers;
using PGCTimeTracker_V2.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;
using System.Threading.Tasks;

namespace PGCTimeTracker_V2.Services
{
    public class IdleDataManager
    {
        public static TimeTrackerConfig configData { get; set; } = new();
        private static string DataFileName => $"PMS_Data_{configData.UserId}";
        private static string ErrorFileName => $"PMS_ErrorLog_{configData.UserId}";
        private static string CredFileName => $"PMS_CRED";
        private static string ShutdownLogFileName => $"PMS_ShutDownLog_{configData.UserId}";
        private static string SystemLogFileName => $"PMS_LOG_{configData.UserId}";
        // Add a flag to prevent recursive error logging
        private static readonly object _errorLogLock = new object();
        private static bool _isLoggingError = false;
        public enum FileType
        {
            Data,
            Error,
            Credentials,
            ShutdownLog,
            SystemLog
        }

        #region Save/Read from Files
        private static string GetDataFilePath(DateTime date, FileType type)
        {
            var baseDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),"PMS_2.0");
            if (!Directory.Exists(baseDir))
                Directory.CreateDirectory(baseDir);

            string name = type switch
            {
                FileType.Data => $"{DataFileName}.json",
                FileType.Error => $"{ErrorFileName}_{date:yyyyMMdd}.json",
                FileType.Credentials => $"{CredFileName}.json",
                FileType.ShutdownLog => $"{ShutdownLogFileName}.json",
                FileType.SystemLog => $"{SystemLogFileName}_{date:yyyyMMdd}.json",
                _ => "Unknown.json"
            };
            return Path.Combine(baseDir, name);
        }

        public static bool SaveDataToFile<T>(T data, FileType type)
        {
            try
            {
                var path = GetDataFilePath(DateTime.UtcNow, type);
                var json = System.Text.Json.JsonSerializer.Serialize(data, new JsonSerializerOptions
                {
                    Encoder = JavaScriptEncoder.Create(UnicodeRanges.All),
                    WriteIndented = true
                });
                File.WriteAllText(path, json, Encoding.UTF8);
                return true;
            }
            catch (Exception ex)
            {
                ErrorToFile(ex, FileType.SystemLog);
                return false;
            }
        }

        public static T? GetDataFromFile<T>(DateTime date, FileType type) where T : class, new()
        {
            try
            {
                var path = GetDataFilePath(date, type);
                if (File.Exists(path))
                {
                    var json = File.ReadAllText(path, Encoding.UTF8);
                    return string.IsNullOrEmpty(json) ? new T() : System.Text.Json.JsonSerializer.Deserialize<T>(json);
                }
            }
            catch (Exception ex)
            {
                ErrorToFile(ex, FileType.SystemLog);
            }
            return new T();
        }

        public static UserCredentials? GetUserCredentials()
        {
            try
            {
                var path = GetDataFilePath(DateTime.UtcNow, FileType.Credentials);
                if (File.Exists(path))
                {
                    var json = File.ReadAllText(path, Encoding.UTF8);
                    return JsonConvert.DeserializeObject<UserCredentials>(json);
                }
            }
            catch (Exception ex)
            {
                ErrorToFile(ex, FileType.SystemLog);
            }
            return null;
        }
        #endregion

        #region Error Logging
        public static bool ErrorToFile(object data, FileType type)
        {
            lock (_errorLogLock)
            {
                // Prevent recursive error logging
                if (_isLoggingError)
                {
                    // Write to debug output as a last resort
                    Debug.WriteLine($"[Recursive Error Log Prevented] {data}");
                    return false;
                }

                try
                {
                    _isLoggingError = true;

                    var path = GetDataFilePath(DateTime.UtcNow, type);

                    // Ensure directory exists
                    var directory = Path.GetDirectoryName(path);
                    if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                    {
                        Directory.CreateDirectory(directory);
                    }

                    // Format the error message
                    string errorMessage = data switch
                    {
                        Exception ex => $"{DateTime.UtcNow:HH:mm:ss} [ERROR] {ex.GetType().Name}: {ex.Message}\nStackTrace: {ex.StackTrace}",
                        _ => $"{DateTime.UtcNow:HH:mm:ss} {data}"
                    };

                    File.AppendAllText(path, $"{errorMessage}{Environment.NewLine}");
                    return true;
                }
                catch (Exception ex)
                {
                    // As a last resort, write to console/debug output
                    Debug.WriteLine($"[Error Logging Failed] {ex.Message}");
                    Console.WriteLine($"[Error Logging Failed] {ex.Message}");
                    return false;
                }
                finally
                {
                    _isLoggingError = false;
                }
            }
        }
        #endregion

        #region Token Refresh
        public static async Task GetNewTokenAfterValidation()
        {
            var cred = GetUserCredentials();
            if (cred != null)
            {
                // Call your CommonExtension to refresh token
                var token = await CommonExtension.GetUserTokenByApi(cred.UserName, cred.Password);
                if (!string.IsNullOrEmpty(token))
                {
                    CommonExtension.TokenKey = token;
                }
                else
                {
                    ErrorToFile("Token refresh failed", FileType.SystemLog);
                }
            }
        }
        public static async Task<long> SaveLogoutTime(LogoutTimeVM logoutUser, int? retry = 3, int? retryInterval = 10000)
        {
            var exception = new List<Exception>();
            for (int attempted = 0; attempted < retry; attempted++)
            {
                try
                {
                    if (!CommonExtension.ValidateTokenTimeBased(CommonExtension.GetUserToken().ToString()))
                    {
                        await GetNewTokenAfterValidation();
                    }
                    var response = await CommonExtension.ExcuteAsync<LogoutTimeVM, long>(logoutUser, UrlConstants.SaveLogoutUser, RequestType.POST, CommonExtension.GetUserToken());
                    return response.ResponseData;
                }
                catch (Exception ex)
                {
                    ErrorToFile(ex, FileType.SystemLog);
                    exception.Add(ex);
                    await Task.Delay(retryInterval??10000);
                }
            }
            ErrorToFile("Exiting SaveLogoutTime", FileType.SystemLog);
            throw new AggregateException(exception);
        }
        #endregion
    }
}
