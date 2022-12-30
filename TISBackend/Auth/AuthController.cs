using Newtonsoft.Json.Linq;
using System;
using System.Data;
using System.Runtime.Caching;
using TISBackend.Db;

namespace TISBackend.Auth
{
    public struct AuthToken
    {
        public string Username { get; set; }
        public string Hash { get; set; }

        public static AuthToken? FromJSON(JObject json)
        {
            if (json == null || !json.ContainsKey("user") || !json.ContainsKey("hash"))
            {
                return null;
            }

            return new AuthToken() { Username = json["user"].ToString(), Hash = json["hash"].ToString() };
        }
    }

    public enum AuthLevel
    {
        NONE, OUTER, INNER, ADMIN
    }

    public static class AuthController
    {
        private static ObjectCache cachedTokens = MemoryCache.Default;

        public static AuthLevel CheckInDatabase(AuthToken authToken)
        {
            DataRow query = DatabaseController.Query($"SELECT \"ST64113\".\"VERIFY_USER\"(\"{authToken.Username}\", \"{authToken.Hash}\") \"level\" FROM DUAL").Rows[0];
            switch (query["level"].ToString())
            {
                case "0": return AuthLevel.ADMIN;
                case "1": return AuthLevel.INNER;
                case "2": return AuthLevel.OUTER;
                default: return AuthLevel.NONE;
            }
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
                return cachedLevel.Value;
            }

            AuthLevel level = CheckInDatabase(token);
            cachedTokens.Add(token.Username, level, DateTimeOffset.Now.AddMinutes(15));
            return level;
        }

    }
}