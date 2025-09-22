using Newtonsoft.Json;
using PGCTimeTracker.Helpers;
using PGCTimeTracker.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;
using System.Threading.Tasks;

namespace PGCTimeTracker.Services
{
    public class IdleDataManager
    {
        public static TimeTrackerConfig configData { get; set; } = new();
        private static string DataFileName => $"PMS_Data_{configData.UserId}";
        private static string ErrorFileName => $"PMS_ErrorLog_{configData.UserId}";
        private static string CredFileName => $"PMS_CRED";
        private static string ShutdownLogFileName => $"PMS_ShutDownLog_{configData.UserId}";
        private static string SystemLogFileName => $"PMS_LOG_{configData.UserId}";
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
            try
            {
                var path = GetDataFilePath(DateTime.UtcNow, type);
                File.AppendAllText(path, $"{DateTime.UtcNow:HH:mm:ss} {data}{Environment.NewLine}");
                return true;
            }
            catch(Exception ex)
            {
                ErrorToFile($"[Error Logging Failed] {ex.Message}",FileType.SystemLog);
                return false;
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
