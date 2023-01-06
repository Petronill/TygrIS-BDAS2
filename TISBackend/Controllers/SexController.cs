using Newtonsoft.Json.Linq;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Data;
using System.Net;
using System.Runtime.Caching;
using System.Web.Http;
using TISBackend.Auth;
using TISBackend.Db;
using TISModelLibrary;

namespace TISBackend.Controllers
{
    public class SexController : TISControllerWithInt
    {
        public const string TABLE_NAME = "POHLAVI";
        public const string ID_NAME = "id_pohlavi";

        protected static readonly ObjectCache cachedSexes = MemoryCache.Default;

        private static readonly SexController instance = new SexController();

        [NonAction]
        public static Sex New(DataRow dr, AuthLevel authLevel, string ID_NAME = SexController.ID_NAME)
        {
            return new Sex()
            {
                Id = int.Parse(dr[ID_NAME].ToString()),
                Abbreviation = dr["zkratka"].ToString()
            };
        }

        [Route("api/id/sex")]
        public IEnumerable<int> GetIds()
        {
            return GetIds(TABLE_NAME, ID_NAME, true);
        }

        // GET: api/Sex
        public IEnumerable<Sex> Get()
        {
            List<Sex> list = new List<Sex>();

            DataTable query = DatabaseController.Query($"SELECT * FROM {TABLE_NAME}");
            foreach (DataRow dr in query.Rows)
            {
                list.Add(New(dr, GetAuthLevel()));
            }

            return list;
        }

        // GET: api/Sex/5
        public Sex Get(int id)
        {
            if (cachedSexes[id.ToString()] is Sex)
            {
                return cachedSexes[id.ToString()] as Sex;
            }

            DataTable query = DatabaseController.Query($"SELECT * FROM {TABLE_NAME} WHERE {ID_NAME} = :id", new OracleParameter("id", id));

            if (query.Rows.Count != 1)
            {
                return null;
            }

            Sex sex = New(query.Rows[0], GetAuthLevel());
            cachedSexes.Add(id.ToString(), sex, DateTimeOffset.Now.AddMinutes(15));
            return sex;
        }

        [NonAction]
        protected override bool CheckObject(JObject value, AuthLevel authLevel)
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
        public static bool CheckObjectStatic(JObject value, AuthLevel authLevel)
        {
            return instance.CheckObject(value, authLevel);
        }

        [NonAction]
        public static int SetObjectStatic(JObject value, AuthLevel authLevel, OracleTransaction transaction = null)
        {
            return instance.SetObject(value, authLevel, transaction);
        }

        // POST: api/Sex
        public IHttpActionResult Post([FromBody] JObject value)
        {
            return HasHigherAuth() ? PostUnknownNumber(value) : StatusCode(HttpStatusCode.Forbidden);
        }

        // POST: api/Sex/5
        public IHttpActionResult Post(int id, [FromBody] JObject value)
        {
            return HasHigherAuth() ? PostSingle(id, value) : StatusCode(HttpStatusCode.Forbidden);
        }

        // DELETE: api/Sex/5
        public IHttpActionResult Delete(int id)
        {
            return DeleteById(TABLE_NAME, ID_NAME, id, cachedSexes);
        }
    }
}
