using Newtonsoft.Json.Linq;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Net;
using System.Web.Http;
using TISBackend.Auth;
using TISBackend.Db;

namespace TISBackend.Controllers
{
    public class LoginController : TISController
    {
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
        private bool CheckAccount(JObject account)
        {
            return ValidJSON(account, "user", "hash", "level") && int.TryParse(account["level"].ToString(), out _);
        }

        [NonAction]
        private void SetAccount(JObject account)
        {
            DatabaseController.Execute("PKG_HESLA.NASTAV_UCET",
                new OracleParameter("p_jmeno", account["user"].ToString()),
                new OracleParameter("p_hash", account["hash"].ToString()),
                new OracleParameter("p_uroven", account["level"].ToString()));
            if (account.ContainsKey("pid") && int.TryParse(account["pid"].ToString(), out int id))
            {
                DatabaseController.Execute("PKG_HESLA.NASTAV_CLOVEKA",
                    new OracleParameter("p_jmeno", account["user"].ToString()),
                    new OracleParameter("p_id_clovek", id));
            }
        }

        // POST: api/Login
        public IHttpActionResult Post([FromBody] JObject value)
        {
            if (!IsAdmin()) {
                return StatusCode(HttpStatusCode.Unauthorized);
            }
            
            if (value.Type == JTokenType.Array)
            {
                JArray array = value.ToObject<JArray>();
                foreach (JToken token in array)
                {
                    if (token.Type != JTokenType.Object)
                    {
                        return StatusCode(HttpStatusCode.BadRequest);
                    }
                    JObject obj = token.ToObject<JObject>();
                    if (!CheckAccount(obj))
                    {
                        return StatusCode(HttpStatusCode.BadRequest);
                    }
                    SetAccount(obj);
                }
            } else
            {
                if (!CheckAccount(value))
                {
                    return StatusCode(HttpStatusCode.BadRequest);
                }
                SetAccount(value);
            }
            return StatusCode(HttpStatusCode.OK);
        }

        // DELETE: api/Login/ucet
        public IHttpActionResult Delete(string id)
        {
            return DeleteById("UCTY", "jmeno", id);
        }
    }
}
