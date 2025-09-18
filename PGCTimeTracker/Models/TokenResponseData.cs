using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PGCTimeTracker.Models
{
    public partial class TokenResponseData
    {
        [JsonProperty("TwoFactorEnabled")]
        public bool TwoFactorEnabled { get; set; }

        [JsonProperty("Token")]
        public TokenResponseVM? Token { get; set; }
    }
    public class TokenResponseVM
    {
        [JsonProperty("accessToken")]
        public string AccessToken { get; set; } = string.Empty;

        [JsonProperty("refreshToken")]
        public string RefreshToken { get; set; } = string.Empty;

        [JsonProperty("expiresIn")]
        public int ExpiresIn { get; set; }
    }
}
