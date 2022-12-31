using Newtonsoft.Json.Linq;
using Oracle.ManagedDataAccess.Client;
using System.Net;
using System.Web.Http;
using TISBackend.Auth;
using TISBackend.Db;

namespace TISBackend.Controllers
{
    public class TISController : ApiController
    {
        protected bool IsAdmin()
        {
             return AuthController.Check(AuthToken.From(Request.Headers)) == AuthLevel.ADMIN;
        }

        protected bool IsAuthorized()
        {
            return AuthController.Check(AuthToken.From(Request.Headers)) != AuthLevel.NONE;
        }

        protected bool ValidJSON(JObject value, params string[] keys)
        {
            if (value == null)
            {
                return false;
            }
            foreach (string key in keys)
            {
                if (!value.ContainsKey(key))
                {
                    return false;
                }
            }
            return true;
        }

        protected IHttpActionResult DeleteById<T>(string tableName, string idName, T id)
        {
            if (!IsAuthorized())
            {
                return StatusCode(HttpStatusCode.Forbidden);
            }
            DatabaseController.Query($"DELETE FROM {tableName} WHERE {idName} = :id", new OracleParameter("id", id));
            return StatusCode(HttpStatusCode.OK);
        }
    }
}