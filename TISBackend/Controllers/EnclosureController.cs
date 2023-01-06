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
    public class EnclosureController : TISControllerWithInt
    {
        public const string TABLE_NAME = "VYBEHY";
        public const string ID_NAME = "id_vybeh";
        public const string SUPER_TABLE_NAME = "PAVILONY";
        public const string SUPER_ID_NAME = "id_pavilon";
        public const string OTHER_NAZEV_NAME = "nazev2";

        protected static readonly ObjectCache cachedEnclosures = MemoryCache.Default;

        private static readonly EnclosureController instance = new EnclosureController();

        [NonAction]
        public static Enclosure New(DataRow dr, AuthLevel authLevel, string idName = EnclosureController.ID_NAME, string superIdName = EnclosureController.SUPER_ID_NAME, string otherNazevName = EnclosureController.OTHER_NAZEV_NAME)
        {
            return new Enclosure()
            {
                Id = int.Parse(dr[idName].ToString()),
                Name = dr["nazev"].ToString(),
                Capacity = int.Parse(dr["kapacita"].ToString()),
                Pavilion = (dr[superIdName].ToString() == "") ? null : new Pavilion()
                {
                    Id = int.Parse(dr[superIdName].ToString()),
                    Name = dr[otherNazevName].ToString()
                }
            };
        }

        [Route("api/id/enclosure")]
        public IEnumerable<int> GetIds()
        {
            return GetIds(TABLE_NAME, ID_NAME, true);
        }

        // GET: api/Enclosure
        public IEnumerable<Enclosure> Get()
        {
            List<Enclosure> list = new List<Enclosure>();

            DataTable query = DatabaseController.Query($"SELECT t1.*, t2.*, t2.nazev AS {OTHER_NAZEV_NAME} FROM {TABLE_NAME} t1 LEFT JOIN {SUPER_TABLE_NAME} t2 ON t1.{SUPER_ID_NAME} = t2.{SUPER_ID_NAME}");
            foreach (DataRow dr in query.Rows)
            {
                list.Add(New(dr, GetAuthLevel()));
            }

            return list;
        }

        // GET: api/Enclosure/5
        public Enclosure Get(int id)
        {
            if (cachedEnclosures[id.ToString()] is Enclosure)
            {
                return cachedEnclosures[id.ToString()] as Enclosure;
            }

            DataTable query = DatabaseController.Query($"SELECT t1.*, t2.*, t2.nazev AS {OTHER_NAZEV_NAME} FROM {TABLE_NAME} t1 LEFT JOIN {SUPER_TABLE_NAME} t2 ON t1.{SUPER_ID_NAME} = t2.{SUPER_ID_NAME} WHERE {ID_NAME} = :id", new OracleParameter("id", id));

            if (query.Rows.Count != 1)
            {
                return null;
            }

            Enclosure enclosure = New(query.Rows[0], GetAuthLevel());
            cachedEnclosures.Add(id.ToString(), enclosure, DateTimeOffset.Now.AddMinutes(15));
            return enclosure;
        }



        [NonAction]
        protected override bool CheckObject(JObject value, AuthLevel authLevel)
        {
            bool intermediate = ValidJSON(value, "Id", "Name", "Capacity", "Pavilion")
                && int.TryParse(value["Id"].ToString(), out _) && int.TryParse(value["Capacity"].ToString(), out _);

            if (!intermediate)
            {
                return false;
            }

            JObject pavilion = (value["Pavilion"]?.Type == JTokenType.Object) ? value["Pavilion"].ToObject<JObject>() : null;
            return pavilion == null || PavilionController.CheckObjectStatic(pavilion, authLevel);
        }

        [NonAction]
        protected override int SetObjectInternal(JObject value, AuthLevel authLevel, OracleTransaction transaction)
        {
            Enclosure n = value.ToObject<Enclosure>();

            int? id_pavilion = (n.Pavilion != null) ? (int?)PavilionController.SetObjectStatic(value["Pavilion"].ToObject<JObject>(), authLevel, transaction) : null;
            if (id_pavilion != null && id_pavilion.Value == ErrId)
            {
                return ErrId;
            }

            OracleParameter p_id = new OracleParameter("p_id", n.Id);
            DatabaseController.Execute("PKG_MODEL_DML.UPSERT_VYBEH", transaction,
                p_id,
                new OracleParameter("p_nazev", n.Name),
                new OracleParameter("p_kapacita", n.Capacity),
                new OracleParameter("p_id_pavilon", id_pavilion));
            int id = int.Parse(p_id.Value.ToString());

            if (id != ErrId && cachedEnclosures.Contains(id.ToString()))
            {
                n.Id = id;
                cachedEnclosures[id.ToString()] = n;
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

        // POST: api/Enclosure
        public IHttpActionResult Post([FromBody] JObject value)
        {
            return HasHigherAuth() ? PostUnknownNumber(value) : StatusCode(HttpStatusCode.Forbidden);
        }

        // POST : api/Enclosure/5
        public IHttpActionResult Post(int id, [FromBody] JObject value)
        {
            return HasHigherAuth() ? PostSingle(id, value) : StatusCode(HttpStatusCode.Forbidden);
        }

        // DELETE: api/Enclosure/5
        public IHttpActionResult Delete(int id)
        {
            return DeleteById(TABLE_NAME, ID_NAME, id, cachedEnclosures);
        }
    }
}
