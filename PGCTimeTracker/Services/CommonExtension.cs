using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Media;
using Newtonsoft.Json;
using PGCTimeTracker_V2.Helpers;
using PGCTimeTracker_V2.Models;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace PGCTimeTracker_V2.Services
{
    public static class CommonExtension
    {
        public static string TokenKey { get; set; } = string.Empty;

        public static string GetUserToken() =>
            string.IsNullOrEmpty(TokenKey) ? string.Empty : TokenKey;

        // Generic HTTP call (lightweight replacement of APIRequester)
        public static async Task<TypedApiResponse<R>> ExcuteAsync<T, R>(T requestModel, string requestURL, RequestType requestType, string authorizationToken = null, Dictionary<string, string> headers = null)
        {
            using var http = new HttpClient();
            try
            {
                var request = new HttpRequestMessage(requestType == RequestType.POST ? HttpMethod.Post : HttpMethod.Get, requestURL);
                if (!string.IsNullOrEmpty(authorizationToken))
                {
                    var cleanToken = authorizationToken.Replace("bearer ", "", StringComparison.OrdinalIgnoreCase).Trim('"', '{', '}', ' ');
                    request.Headers.Authorization = new AuthenticationHeaderValue("bearer", cleanToken);                    
                }

                if (headers != null)
                { 
                    foreach (var h in headers)
                        request.Headers.Add(h.Key, h.Value);
                }

                if (requestModel != null && requestType == RequestType.POST)
                {
                    var json = JsonConvert.SerializeObject(requestModel);
                    request.Content = new StringContent(json, Encoding.UTF8, "application/json");
                }

                var response = await http.SendAsync(request);
                var body = await response.Content.ReadAsStringAsync();
                var apiResponse = JsonConvert.DeserializeObject<TypedApiResponse<R>>(body);
                // If API returns nested ResponseData in different shape adjust here
                return apiResponse ?? new TypedApiResponse<R>();// { ResponseStatus = ResponseStatuses.Failed, Message = "Empty response" };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[HTTP ERROR] {ex.Message}");
                return new TypedApiResponse<R>(); //{ ResponseStatus = ResponseStatuses.Failed, Message = ex.Message };
            }
        }

        public static async Task<string?> GetUserTokenByApi(string userName, string password)
        {
            var request = new TMSHttpRequest
            {
                Url = UrlConstants.Token,
                RequestType = RequestType.POST.ToString(),
                Headers = new Dictionary<string, string>
                {
                    { "Content-Type", "application/json" }
                },
                Body = JsonConvert.SerializeObject(new
                {
                    username = userName,
                    password
                })
            };

            var api = new APIRequester(request.Url);
            var response = await api.ExecuteRequest(request);

            if (response.StatusCode == 200 && !string.IsNullOrWhiteSpace(response.Body))
            {
                try
                {
                    var tokenResponse = JsonConvert.DeserializeObject<TokenApiResponse>(response.Body);
                    if (tokenResponse?.ResponseData?.Token != null)
                    {
                        IdleDataManager.SaveDataToFile($"token API call succeeded: {tokenResponse?.ResponseData?.Token}", IdleDataManager.FileType.SystemLog);
                        return tokenResponse.ResponseData.Token.AccessToken;
                    }
                    IdleDataManager.ErrorToFile($"Get User token is failed API call,Response:{tokenResponse?.ResponseData} ", IdleDataManager.FileType.SystemLog);
                }
                catch (Exception ex)
                {
                    IdleDataManager.ErrorToFile(ex.Message, IdleDataManager.FileType.SystemLog);
                }
            }
            return null;
        }
        public static async Task<TimeTrackerConfig> GetUserDetails()
        {
            try
            {
                TimeTrackerConfig config = new ();

                var userDetails = await ExcuteAsync<object, LoginResponseVM>(
                                                                   null,
                                                                   UrlConstants.GetUserDetail,
                                                                   RequestType.GET,
                                                                   TokenKey ?? string.Empty);

                if (userDetails?.ResponseStatus == ResponseStatuses.Success && userDetails.ResponseData != null)
                {
                    config = new TimeTrackerConfig
                    {
                        UserId = userDetails.ResponseData.UserId,
                        FirstName = userDetails.ResponseData.FirstName,
                        LastName = userDetails.ResponseData.LastName,
                        RoleName = userDetails.ResponseData.RoleId.ToString(),
                        OrgToken = userDetails.ResponseData.Organizations[0].Token
                    };
                    IdleDataManager.SaveDataToFile($"{config.FirstName + config.LastName} user details found", IdleDataManager.FileType.SystemLog);
                }
                else
                {
                    IdleDataManager.ErrorToFile($"User details not found", IdleDataManager.FileType.SystemLog);
                    return null;
                }
                return config;
            }
            catch(Exception ex)
            {
                IdleDataManager.ErrorToFile(ex.Message, IdleDataManager.FileType.SystemLog);
                throw;
            }
        }
        public static bool ValidateTokenTimeBased(string token)
        {
            if (string.IsNullOrEmpty(token)) return false;
            var handler = new JwtSecurityTokenHandler();
            try
            {
                var jwt = handler.ReadJwtToken(token);
                return jwt.ValidTo > DateTime.UtcNow;
            }
            catch
            {
                return false;
            }
        }
        public static async Task ShowMessageAsync(string message, string title)
        {
            var dialog = new Window
            {
                Title = title,
                Width = 400,
                Height = 200,
                Content = new TextBlock
                {
                    Text = message,
                    Margin = new Thickness(20),
                    TextWrapping = TextWrapping.Wrap
                }
            };

            await dialog.ShowDialog((Window)Application.Current.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop ? desktop.MainWindow : null);
        }

    }
}
