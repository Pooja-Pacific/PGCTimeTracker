using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PGCTimeTracker_V2.Helpers
{
    public static class ResponseStatuses
    {
        public const string Success = "Success";
        public const string Failure = "Failure";
    }

    public static class UrlConstants
    {       
        public static string BaseUserManagementUrl = "https://pms-stag-usermanager.azurewebsites.net/api/";
        public static string BaseWorklogsUrl = "https://pms-stag-worklog.azurewebsites.net/api/";
        public static string BaseHelpUrl = "https://pms-stag-common.azurewebsites.net/api";

        public static string Token => BaseUserManagementUrl + "auth/token";
        public static string GetUserDetail => BaseUserManagementUrl + "auth/getuserdetails";
        public static string SaveIdleTime => BaseWorklogsUrl + "workitem/idle/SaveIdleEntry";
        public static string SaveLogoutUser => BaseUserManagementUrl + "auth/logoutwithexe";
        public static string GetExeSetupLocation => BaseHelpUrl + "tutorial/getsetup";
    }

    public static class MessageConstants
    {
        public const string LogSynced = "Time logs saved successfully!";
        public const string TaskRunning = "Please pause or stop any running task and try again!";
    }

    public enum RequestType
    {
        POST,
        DELETE,
        GET,
        PUT
    }

    public static class StringExtensions       
    {
        public static bool EqualsIgnoreCase(this string source, string toCheck)
        {
            return source?.Equals(toCheck ?? "", StringComparison.OrdinalIgnoreCase) ?? false;
        }
    }

    public class TMSHttpRequest
    {
        public string RequestType { get; set; } = "GET";
        public string Url { get; set; } = string.Empty;
        public int Timeout { get; set; } = -1;
        public Dictionary<string, string>? Headers { get; set; }
        public string Body { get; set; } = string.Empty;
    }

    public class TMSHttpResponse
    {
        public int StatusCode { get; set; }
        public Dictionary<string, string> Headers { get; set; } = new();
        public string Body { get; set; } = string.Empty;
        public Exception? Exception { get; set; }
    }

    public class APIRequester
    {
        private static async Task<string> Decompress(Stream stream)
        {
            using var mStream = new GZipStream(stream, CompressionMode.Decompress);
            using var reader = new StreamReader(mStream);
            return await reader.ReadToEndAsync();
        }

        private readonly HttpClient _httpClient;
        private readonly int _defaultTimeOut = 6000000;

        public APIRequester(string? baseUrl=null)
        {
            var clientHandler = new HttpClientHandler { AutomaticDecompression = DecompressionMethods.None };
            _httpClient = new HttpClient(clientHandler)
            {
                BaseAddress = new Uri(baseUrl),              // ✅ Fix: set BaseAddress
                Timeout = TimeSpan.FromMilliseconds(_defaultTimeOut)
            };
        }

        public async Task<TMSHttpResponse> ExecuteRequest(TMSHttpRequest request)
        {
            var requestUri = Uri.TryCreate(request.Url, UriKind.RelativeOrAbsolute, out var uri) && !uri.IsAbsoluteUri
                ? new Uri(request.Url, UriKind.Relative)
                : uri;

            var requestMessage = new HttpRequestMessage(new HttpMethod(request.RequestType), requestUri);

            // Add body only if it's not empty
            if (!string.IsNullOrEmpty(request.Body))
            {
                requestMessage.Content = new StringContent(request.Body, Encoding.UTF8, "application/json");
            }

            // Add body (compressed or normal)
            var isRequestCompressed =
                requestMessage.Headers.TryAddWithoutValidation("Content-Encoding", new List<string>() { "gzip" });


            // Add headers
            if (request.Headers != null)
            {
                foreach (var key in request.Headers.Keys)
                {
                    var val = request.Headers[key];
                    switch (key.ToLower())
                    {
                        case "authorization":
                            var spl = val.Split(' ');
                            if (spl.Length == 2)
                                requestMessage.Headers.Authorization = new AuthenticationHeaderValue(spl[0], spl[1]);
                            else
                                requestMessage.Headers.TryAddWithoutValidation("Authorization", val);
                            break;
                        case "connection":
                            requestMessage.Headers.Connection.Add(val);
                            break;
                        case "accept":
                            requestMessage.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue(val));
                            break;
                        case "content-type":
                            requestMessage.Content.Headers.ContentType = MediaTypeHeaderValue.Parse(val);
                            break;
                        default:
                            requestMessage.Headers.TryAddWithoutValidation(key, val);
                            break;
                    }
                }
            }

            // Accept gzip responses
            var isResponseCompressed =
                requestMessage.Headers.TryAddWithoutValidation("Content-Encoding", new List<string>() { "gzip" });

            var cancellationToken = new CancellationTokenSource(
                (request.Timeout != -1) ? request.Timeout : _defaultTimeOut);

            try
            {
                Console.WriteLine($"URL: {request.Url}");
                var responseMessage = await _httpClient.SendAsync(requestMessage, cancellationToken.Token);
                var body = await responseMessage.Content.ReadAsStringAsync();

                return new TMSHttpResponse
                {
                    StatusCode = (int)responseMessage.StatusCode,
                    Headers = responseMessage.Headers.ToDictionary(h => h.Key, h => string.Join(" ", h.Value)),
                    Body = body
                };
            }
            catch (TaskCanceledException e)
            {
                // Replace IdleDataManager with logging
                Console.WriteLine($"Request timeout: {e.Message}");
                return new TMSHttpResponse()
                {
                    StatusCode = 408,
                    Exception = e
                };
            }
        }
    }
}
