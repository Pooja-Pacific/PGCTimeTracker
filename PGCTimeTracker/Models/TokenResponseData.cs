using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PGCTimeTracker_V2.Models
{
    public class TokenApiResponse
    {
        [JsonProperty("ResponseStatus")]
        public string ResponseStatus { get; set; }

        [JsonProperty("Message")]
        public string Message { get; set; }

        [JsonProperty("ResponseData")]
        public TokenResponseData ResponseData { get; set; }

        [JsonProperty("ErrorData")]
        public object ErrorData { get; set; }
    }
    public partial class TokenResponseData
    {
        [JsonProperty("TwoFactorEnabled")]
        public bool TwoFactorEnabled { get; set; }

        [JsonProperty("Token")]
        public TokenResponseVM? Token { get; set; }
    }
    public class TokenResponseVM
    {
        [JsonProperty("Username")]
        public string Username { get; set; }

        [JsonProperty("Token")]
        public string AccessToken { get; set; }

        [JsonProperty("TokenExpiry")]
        public DateTime TokenExpiry { get; set; }
    }
}
