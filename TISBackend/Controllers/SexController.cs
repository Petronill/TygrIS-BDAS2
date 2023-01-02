using Newtonsoft.Json.Linq;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Data;
using System.Runtime.Caching;
using System.Web.Http;
using TISBackend.Auth;
using TISBackend.Db;
using TISModelLibrary;

namespace TISBackend.Controllers
{
    public class SexController : TISControllerWithInt
    {
        public const string tableName = "POHLAVI";
        public const string idName = "id_pohlavi";

        protected static readonly ObjectCache cachedSexes = MemoryCache.Default;

        private static readonly SexController instance = new SexController();

        [NonAction]
        public static Sex New(DataRow dr, AuthLevel authLevel, string idName = SexController.idName)
        {
            return new Sex()
            {
                Id = int.Parse(dr[idName].ToString()),
                Abbreviation = dr["zkratka"].ToString()
            };
        }

        // GET: api/Sex
        public IEnumerable<Sex> Get()
        {
            List<Sex> list = new List<Sex>();

            if (IsAuthorized())
            {
                DataTable query = DatabaseController.Query($"SELECT * FROM {tableName}");
                foreach (DataRow dr in query.Rows)
                {
                    list.Add(New(dr, GetAuthLevel()));
                }
            }

            return list;
        }

        // GET: api/Sex/5
        public Sex Get(int id)
        {
            if (!IsAuthorized())
            {
                return null;
            }

            if (cachedSexes[id.ToString()] is Sex)
            {
                return cachedSexes[id.ToString()] as Sex;
            }

            DataTable query = DatabaseController.Query($"SELECT * FROM {tableName} WHERE {idName} = :id", new OracleParameter("id", id));

            if (query.Rows.Count != 1)
            {
                return null;
            }

            Sex sex = New(query.Rows[0], GetAuthLevel());
            cachedSexes.Add(id.ToString(), sex, DateTimeOffset.Now.AddMinutes(15));
            return sex;
        }

        [NonAction]
        protected override bool CheckObject(JObject value)
        {
            return ValidJSON(value, "Id", "Abbreviation") && int.TryParse(value["Id"].ToString(), out _);
        }

        [NonAction]
        protected override int SetObjectInternal(JObject value, AuthLevel authLevel, OracleTransaction transaction)
        {
            Sex n = value.ToObject<Sex>();
            OracleParameter p_id = new OracleParameter("p_id", n.Id);
            DatabaseController.Execute("PKG_MODEL_DML.UPSERT_POHLAVI", transaction,
                p_id, new OracleParameter("p_zkratka", n.Abbreviation));
            int id = int.Parse(p_id.Value.ToString());

            if (id != ErrId && cachedSexes.Contains(id.ToString()))
            {
                n.Id = id;
                cachedSexes[id.ToString()] = n;
            }

            return id;
        }

        [NonAction]
        public static bool CheckObjectStatic(JObject value)
        {
            return instance.CheckObject(value);
        }

        [NonAction]
        public static int SetObjectStatic(JObject value, AuthLevel authLevel, OracleTransaction transaction = null)
        {
            return instance.SetObject(value, authLevel, transaction);
        }

        // POST: api/Sex
        public IHttpActionResult Post([FromBody] JObject value)
        {
            return PostUnknownNumber(value);
        }

        // POST: api/Sex/5
        public IHttpActionResult Post(int id, [FromBody] JObject value)
        {
            return PostSingle(id, value);
        }

        // DELETE: api/Sex/5
        public IHttpActionResult Delete(int id)
        {
            return DeleteById(tableName, idName, id, cachedSexes);
        }
    }
}
