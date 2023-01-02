using Newtonsoft.Json.Linq;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Net;
using System.Web.Http;
using TISBackend.Auth;
using TISBackend.Db;

namespace TISBackend.Controllers
{
    public class LoginController : TISControllerWithString
    {
        private static readonly LoginController instance = new LoginController();

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
        protected override bool CheckObject(JObject value)
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
        public static bool CheckObjectStatic(JObject value)
        {
            return instance.CheckObject(value);
        }

        [NonAction]
        public static string SetObjectStatic(JObject value, AuthLevel authLevel, OracleTransaction transaction = null)
        {
            return instance.SetObject(value, authLevel, transaction);
        }

        // POST: api/Login
        public IHttpActionResult Post([FromBody] JObject value)
        {
            if (!IsAdmin()) {
                return StatusCode(HttpStatusCode.Unauthorized);
            }

            return PostUnknownNumber(value);
        }

        // POST: api/Login/5
        public IHttpActionResult Post(string id, [FromBody] JObject value)
        {
            if (!IsAdmin())
            {
                return StatusCode(HttpStatusCode.Unauthorized);
            }

            return PostSingle(id, value);
        }

        // DELETE: api/Login/ucet
        public IHttpActionResult Delete(string id)
        {
            return DeleteById("UCTY", "jmeno", id);
        }
    }
}
