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
    public class PavilionController : TISControllerWithInt
    {
        public const string TABLE_NAME = "PAVILONY";
        public const string ID_NAME = "id_pavilon";

        protected static readonly ObjectCache cachedPavilions = MemoryCache.Default;

        private static readonly PavilionController instance = new PavilionController();

        [NonAction]
        private static Pavilion New(DataRow dr, AuthLevel authLevel, string idName = PavilionController.ID_NAME)
        {
            return new Pavilion()
            {
                Id = int.Parse(dr[idName].ToString()),
                Name = dr["nazev"].ToString()
            };
        }

        [Route("api/id/pavilion")]
        public IEnumerable<int> GetIds()
        {
            return GetIds(TABLE_NAME, ID_NAME, true);
        }

        // GET: api/Pavilion
        public IEnumerable<Pavilion> Get()
        {
            List<Pavilion> list = new List<Pavilion>();

            DataTable query = DatabaseController.Query($"SELECT * FROM {TABLE_NAME}");
            foreach (DataRow dr in query.Rows)
            {
                list.Add(New(dr, GetAuthLevel()));
            }

            return list;
        }

        // GET: api/Pavilion/5
        public Pavilion Get(int id)
        {
            if (cachedPavilions[id.ToString()] is Pavilion)
            {
                return cachedPavilions[id.ToString()] as Pavilion;
            }

            DataTable query = DatabaseController.Query($"SELECT * FROM {TABLE_NAME} WHERE {ID_NAME} = :id", new OracleParameter("id", id));
            
            if (query.Rows.Count != 1)
            {
                return null;
            }

            Pavilion pavilion = New(query.Rows[0], GetAuthLevel());
            cachedPavilions.Add(id.ToString(), pavilion, DateTimeOffset.Now.AddMinutes(15));
            return pavilion;
        }

        [NonAction]
        protected override bool CheckObject(JObject value, AuthLevel authLevel)
        {
            return ValidJSON(value, "Id", "Name") && int.TryParse(value["Id"].ToString(), out _);
        }

        [NonAction]
        protected override int SetObjectInternal(JObject value, AuthLevel authLevel, OracleTransaction transaction)
        {
            Pavilion n = value.ToObject<Pavilion>();
            OracleParameter p_id = new OracleParameter("p_id", n.Id);
            DatabaseController.Execute("PKG_MODEL_DML.UPSERT_PAVILON", transaction,
                p_id, new OracleParameter("p_nazev", n.Name));
            int id = int.Parse(p_id.Value.ToString());

            if (id != ErrId && cachedPavilions.Contains(id.ToString()))
            {
                n.Id = id;
                cachedPavilions[id.ToString()] = n;
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

        // POST: api/Pavilion
        public IHttpActionResult Post([FromBody] JObject value)
        {
            return HasHigherAuth() ? PostUnknownNumber(value) : StatusCode(HttpStatusCode.Forbidden);
        }

        // POST: api/Pavilion/5
        public IHttpActionResult Post(int id, [FromBody] JObject value)
        {
            return HasHigherAuth() ? PostSingle(id, value) : StatusCode(HttpStatusCode.Forbidden);
        }

        // DELETE: api/Pavilion/5
        public IHttpActionResult Delete(int id)
        {
            return DeleteById(TABLE_NAME, ID_NAME, id, cachedPavilions);
        }
    }
}
