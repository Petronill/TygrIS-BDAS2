using Newtonsoft.Json.Linq;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Data;
using System.Linq;
using System.Net.Http.Headers;
using System.Runtime.Caching;
using TISBackend.Db;

namespace TISBackend.Auth
{
    public struct AuthToken
    {
        public string Username { get; set; }
        public string Hash { get; set; }

        public AuthLevel? As { get; set; }

        public static AuthToken? From(HttpRequestHeaders headers)
        {
            if (!headers.TryGetValues("Tis-User", out var userValues) || !headers.TryGetValues("Tis-Hash", out var hashValues)) {
                return null;
            }

            return new AuthToken()
            {
                Username = userValues.First(),
                Hash = hashValues.First(),
                As = (headers.TryGetValues("Tis-As", out var asValues) && Enum.TryParse(asValues.First(), out AuthLevel result)) ? result : (AuthLevel?)null
            };
        }

        [Obsolete("Method AuthToken.From(JObject json) is deprecated, please use method AuthToken.From(HttpRequestHeaders headers) instead.")]
        public static AuthToken? From(JObject json)
        {
            if (json == null || !json.ContainsKey("user") || !json.ContainsKey("hash"))
            {
                return null;
            }

            return new AuthToken()
            {
                Username = json["user"].ToString(),
                Hash = json["hash"].ToString(),
                As = (json.ContainsKey("as") && Enum.TryParse(json["as"].ToString(), out AuthLevel result)) ? result : (AuthLevel?)null
            };
        }
    }

    public enum AuthLevel
    {
        NONE = -1, OUTER = 2, INNER = 1, ADMIN = 0
    }

    public static class AuthController
    {
        private static readonly ObjectCache cachedTokens = MemoryCache.Default;

        private static AuthLevel CheckInDatabase(AuthToken authToken)
        {
            DataRow query = DatabaseController.Query(
                $"SELECT PKG_HESLA.ZJISTI_UROVEN(:jmeno, :hash) \"level\" FROM DUAL",
                new OracleParameter("jmeno", authToken.Username),
                new OracleParameter("hash", authToken.Hash)
            ).Rows[0];
            return Enum.TryParse(query["level"].ToString(), out AuthLevel result) ? result : AuthLevel.NONE;
        }

        public static AuthLevel Check(AuthToken? authToken)
        {
            if (authToken == null)
            {
                return AuthLevel.NONE;
            }
            AuthToken token = authToken.Value;

            AuthLevel? cachedLevel = cachedTokens[token.Username] as AuthLevel?;
            if (cachedLevel != null)
            {
                return (token.As is null || cachedLevel != AuthLevel.ADMIN) ? cachedLevel.Value : token.As.Value;
            }

            AuthLevel level = CheckInDatabase(token);
            cachedTokens.Add(token.Username+token.Hash, level, DateTimeOffset.Now.AddMinutes(15));
            return (token.As is null || level != AuthLevel.ADMIN) ? level : token.As.Value;
        }

        public static void InvalidateCache(AuthToken? authToken)
        {
            if (authToken != null)
            {
                cachedTokens.Remove(authToken.Value.Username + authToken.Value.Hash);
            }
        }
    }
}