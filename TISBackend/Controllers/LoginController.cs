using Newtonsoft.Json.Linq;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web.Http;
using TISBackend.Auth;
using TISBackend.Db;

namespace TISBackend.Controllers
{
    public class LoginController : TISControllerWithString
    {
        private static readonly LoginController instance = new LoginController();

        [Route("api/logout")]
        public bool GetLogout()
        {
            if (!IsAuthorized())
            {
                return false;
            }

            AuthController.InvalidateCache(AuthToken.From(Request.Headers));
            return true;
        }

        [Route("api/level/login")]
        public IEnumerable<AuthLevel> GetAllLevels()
        {
            return Enum.GetValues(typeof(AuthLevel)).Cast<AuthLevel>();
        }

        [Route("api/level/login/{id}")]
        public bool GetIsLevel(string id)
        {
            return Enum.TryParse(id, out AuthLevel _);
        }

        // GET: api/Login
        public AuthLevel Get()
        {
            return AuthController.Check(AuthToken.From(Request.Headers));
        }

        // GET: api/Login/1
        public bool Get(string id)
        {
            return Enum.TryParse(id, out AuthLevel result) && result == AuthController.Check(AuthToken.From(Request.Headers));
        }

        [NonAction]
        protected override bool CheckObject(JObject value, AuthLevel authLevel)
        {
            return ValidJSON(value, "user", "hash", "level") && int.TryParse(value["level"].ToString(), out _);
        }

        [NonAction]
        protected override string SetObjectInternal(JObject value, AuthLevel authLevel, OracleTransaction transaction)
        {
            DatabaseController.Execute("PKG_HESLA.NASTAV_UCET", transaction,
                new OracleParameter("p_jmeno", value["user"].ToString()),
                new OracleParameter("p_hash", value["hash"].ToString()),
                new OracleParameter("p_uroven", value["level"].ToString()));
            if (value.ContainsKey("pid") && int.TryParse(value["pid"].ToString(), out int id))
            {
                DatabaseController.Execute("PKG_HESLA.NASTAV_CLOVEKA", transaction,
                    new OracleParameter("p_jmeno", value["user"].ToString()),
                    new OracleParameter("p_id_clovek", id));
            }

            return value["user"].ToString();
        }


        [NonAction]
        public static bool CheckObjectStatic(JObject value, AuthLevel authLevel)
        {
            return instance.CheckObject(value, authLevel);
        }

        [NonAction]
        public static string SetObjectStatic(JObject value, AuthLevel authLevel, OracleTransaction transaction = null)
        {
            return instance.SetObject(value, authLevel, transaction);
        }

        // POST: api/Login
        public IHttpActionResult Post([FromBody] JObject value)
        {
            return IsAdmin() ? PostUnknownNumber(value) : StatusCode(HttpStatusCode.Forbidden);
        }

        // POST: api/Login/5
        public IHttpActionResult Post(string id, [FromBody] JObject value)
        {
            return IsAdmin() ? PostSingle(id, value) : StatusCode(HttpStatusCode.Forbidden);
        }

        // DELETE: api/Login/ucet
        public IHttpActionResult Delete(string id)
        {
            return DeleteById("UCTY", "jmeno", id);
        }

        [Route("api/login/byid/{id}")]
        public IHttpActionResult Delete(int id)
        {
            if (!IsAdmin())
            {
                return StatusCode(HttpStatusCode.Forbidden);
            }

            DatabaseController.Query($"DELETE FROM UCTY WHERE id_clovek = :id", new OracleParameter("id", id));
            return StatusCode(HttpStatusCode.OK);
        }
    }
}
