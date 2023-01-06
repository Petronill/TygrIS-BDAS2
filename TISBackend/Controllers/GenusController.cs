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
    public class GenusController : TISControllerWithInt
    {
        public const string TABLE_NAME = "RODY";
        public const string ID_NAME = "id_rod";

        protected static readonly ObjectCache cachedGeni = MemoryCache.Default;

        private static readonly GenusController instance = new GenusController();

        [NonAction]
        public static Genus New(DataRow dr, AuthLevel authLevel, string idName = GenusController.ID_NAME)
        {
            return new Genus()
            {
                Id = int.Parse(dr[idName].ToString()),
                CzechName = dr["jmeno_rodu_cesky"].ToString(),
                LatinName = dr["jmeno_rodu_latinsky"].ToString()
            };
        }

        [Route("api/id/genus")]
        public IEnumerable<int> GetIds()
        {
            return GetIds(TABLE_NAME, ID_NAME, true);
        }

        // GET: api/Genus
        public IEnumerable<Genus> Get()
        {
            List<Genus> list = new List<Genus>();

            DataTable query = DatabaseController.Query($"SELECT * FROM {TABLE_NAME}");
            foreach (DataRow dr in query.Rows)
            {
                list.Add(New(dr, GetAuthLevel()));
            }

            return list;
        }

        // GET: api/Genus/5
        public Genus Get(int id)
        {
            if (cachedGeni[id.ToString()] is Genus)
            {
                return cachedGeni[id.ToString()] as Genus;
            }

            DataTable query = DatabaseController.Query($"SELECT * FROM {TABLE_NAME} WHERE {ID_NAME} = :id", new OracleParameter("id", id));

            if (query.Rows.Count != 1)
            {
                return null;
            }

            Genus genus = New(query.Rows[0], GetAuthLevel());
            cachedGeni.Add(id.ToString(), genus, DateTimeOffset.Now.AddMinutes(15));
            return genus;
        }

        [NonAction]
        protected override bool CheckObject(JObject value, AuthLevel authLevel)
        {
            return ValidJSON(value, "Id", "CzechName", "LatinName") && int.TryParse(value["Id"].ToString(), out _);
        }

        [NonAction]
        protected override int SetObjectInternal(JObject value, AuthLevel authLevel, OracleTransaction transaction)
        {
            Genus n = value.ToObject<Genus>();
            OracleParameter p_id = new OracleParameter("p_id", n.Id);
            DatabaseController.Execute("PKG_MODEL_DML.UPSERT_ROD", transaction,
                p_id,
                new OracleParameter("p_cesky", n.CzechName),
                new OracleParameter("p_latinsky", n.LatinName));
            int id = int.Parse(p_id.Value.ToString());

            if (id != ErrId && cachedGeni.Contains(id.ToString()))
            {
                n.Id = id;
                cachedGeni[id.ToString()] = n;
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

        // POST: api/Genus
        public IHttpActionResult Post([FromBody] JObject value)
        {
            return HasHigherAuth() ? PostUnknownNumber(value) : StatusCode(HttpStatusCode.Forbidden);
        }

        // POST : api/Genus/5
        public IHttpActionResult Post(int id, [FromBody] JObject value)
        {
            return HasHigherAuth() ? PostSingle(id, value) : StatusCode(HttpStatusCode.Forbidden);
        }

        // DELETE: api/Genus/5
        public IHttpActionResult Delete(int id)
        {
            return DeleteById(TABLE_NAME, ID_NAME, id, cachedGeni);
        }
    }
}
