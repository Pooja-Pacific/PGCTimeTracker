using Newtonsoft.Json;
using PGCTimeTracker.Models;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;  

namespace PGCTimeTracker.Helpers
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
                    request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authorizationToken);

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
                    password = password
                })
            };

            var api = new APIRequester(request.Url);
            var response = await api.ExecuteRequest(request);

            if (response.StatusCode == 200 && !string.IsNullOrWhiteSpace(response.Body))
            {
                try
                {
                    var tokenResponse = JsonConvert.DeserializeObject<TokenResponseData>(response.Body);
                    if (tokenResponse?.Token != null)
                    {
                        return tokenResponse.Token.AccessToken; // adjust property name based on your API
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[Token Parse Error] {ex.Message}");
                }
            }
            return null;
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

        public static async Task<TResponse?> ExecuteAsync<TRequest, TResponse>(TRequest request,string url,RequestType requestType,string token = "")
            where TResponse : class, new()
        {
            // TODO: Replace with actual HTTP call
            await Task.Delay(300);
            return new TResponse();
        }
    }
}
