using Newtonsoft.Json.Linq;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Data;
using System.Net;
using System.Runtime.Caching;
using System.Web.Http;
using System.Web.Http.Results;
using TISBackend.Auth;
using TISBackend.Db;

namespace TISBackend.Controllers
{
    public abstract class TISController<TId> : ApiController
    {
        protected abstract TId ErrId { get; }

        [NonAction]
        protected AuthLevel GetAuthLevel()
        {
            return AuthController.Check(AuthToken.From(Request.Headers));
        }

        [NonAction]
        protected bool IsAdmin()
        {
             return GetAuthLevel() == AuthLevel.ADMIN;
        }

        [NonAction]
        protected bool IsAuthorized()
        {
            return GetAuthLevel() != AuthLevel.NONE;
        }

        [NonAction]
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

        [NonAction]
        protected abstract List<TId> GetIds(string tableName, string idName);

        [NonAction]
        protected StatusCodeResult DeleteById(string tableName, string idName, TId id, ObjectCache cache = null)
        {
            if (!IsAuthorized())
            {
                return StatusCode(HttpStatusCode.Forbidden);
            }

            cache?.Remove(id.ToString());

            DatabaseController.Query($"DELETE FROM {tableName} WHERE {idName} = :id", new OracleParameter("id", id));
            return StatusCode(HttpStatusCode.OK);
        }

        [NonAction]
        protected virtual bool CheckObject(JObject value) { return true; }

        [NonAction]
        protected virtual TId SetObjectInternal(JObject value, AuthLevel authLevel, OracleTransaction transaction) { return default; }

        [NonAction]
        protected TId SetObject(JObject value, AuthLevel authLevel, OracleTransaction transaction = null)
        {
            bool newTransaction = transaction == null;
            transaction = transaction ?? DatabaseController.StartTransaction();

            TId res = ErrId;
            try
            {
                res = SetObjectInternal(value, authLevel, transaction);
            }
            catch (Exception)
            {
                // TODO - logging?
#if DEBUG
                if (newTransaction) DatabaseController.Rollback(transaction);
                throw;
#endif
            }

            if (newTransaction)
            {
                if (res.Equals(ErrId))
                {
                    DatabaseController.Rollback(transaction);
                }
                else
                {
                    DatabaseController.Commit(transaction);
                }
            }

            return res;
        }

        [NonAction]
        public IHttpActionResult PostUnknownNumber(JObject value)
        {
            try
            {
                if (!IsAuthorized())
                {
                    return StatusCode(HttpStatusCode.Unauthorized);
                }

                if (value.Type == JTokenType.Array)
                {
                    return PostMultiple(value.ToObject<JArray>());
                }

                return PostSingle(value);
            }
            catch (Exception ex)
            {
                return Content(HttpStatusCode.BadRequest, ex.ToString());
            }
        }

        [NonAction]
        public IHttpActionResult PostMultiple(JArray array)
        {
            try
            {
                List<TId> ids = new List<TId>();
                HttpStatusCode statusCode = HttpStatusCode.OK;
                foreach (JToken token in array)
                {
                    if (token.Type != JTokenType.Object)
                    {
                        statusCode = HttpStatusCode.BadRequest;
                        break;
                    }

                    JObject obj = token.ToObject<JObject>();
                    if (!CheckObject(obj))
                    {
                        statusCode = HttpStatusCode.BadRequest;
                        break;
                    }

                    TId id = SetObject(obj, GetAuthLevel());
                    if (id.Equals(ErrId))
                    {
                        statusCode = HttpStatusCode.BadRequest;
                        break;
                    }
                    ids.Add(id);
                }
                return Content(statusCode, ids.ToArray());
            }
            catch (Exception ex)
            {
                return Content(HttpStatusCode.InternalServerError, ex.ToString());
            }
        }

        [NonAction]
        public IHttpActionResult PostSingle(JObject value)
        {
            try { 
                if (!CheckObject(value))
                {
                    return StatusCode(HttpStatusCode.BadRequest);
                }

                TId id = SetObject(value, GetAuthLevel());
                if (id.Equals(ErrId))
                {
                    return StatusCode(HttpStatusCode.BadRequest);
                }
                return Content(HttpStatusCode.OK, id);
            }
            catch (Exception ex)
            {
                return Content(HttpStatusCode.InternalServerError, ex.ToString());
            }
        }

        [NonAction]
        public IHttpActionResult PostSingle(TId id, JObject value)
        {
            try {
                if (!IsAuthorized())
                {
                    return StatusCode(HttpStatusCode.Unauthorized);
                }

                value["Id"] = JToken.FromObject(id);
                return PostSingle(value);
                }
            catch (Exception ex)
            {
                return Content(HttpStatusCode.InternalServerError, ex.ToString());
            }
        }
    }

    public class TISControllerWithInt : TISController<int>
    {
        protected override int ErrId => -1;

        [NonAction]
        protected override List<int> GetIds(string tableName, string idName)
        {
            List<int> list = new List<int>();

            if (IsAuthorized())
            {
                DataTable query = DatabaseController.Query($"SELECT {idName} FROM {tableName}");
                foreach (DataRow dr in query.Rows)
                {
                    list.Add(int.Parse(dr[idName].ToString()));
                }
            }

            return list;
        }
    }

    public class TISControllerWithString : TISController<string>
    {
        protected override string ErrId => "";

        [NonAction]
        protected override List<string> GetIds(string tableName, string idName)
        {
            List<string> list = new List<string>();

            if (IsAuthorized())
            {
                DataTable query = DatabaseController.Query($"SELECT {idName} FROM {tableName}");
                foreach (DataRow dr in query.Rows)
                {
                    list.Add(dr[idName].ToString());
                }
            }

            return list;
        }
    }
}