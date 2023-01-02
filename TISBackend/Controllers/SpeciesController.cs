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
    public class SpeciesController : TISControllerWithInt
    {
        public const string TABLE_NAME = "DRUHY";
        public const string ID_NAME = "id_druh";
        public const string SUPER_TABLE_NAME = "RODY";
        public const string SUPER_ID_NAME = "id_rod";

        protected static readonly ObjectCache cachedSpecies = MemoryCache.Default;

        private static readonly SpeciesController instance = new SpeciesController();

        [NonAction]
        public static Species New(DataRow dr, AuthLevel authLevel, string idName = SpeciesController.ID_NAME)
        {
            return new Species()
            {
                Id = int.Parse(dr[idName].ToString()),
                CzechName = dr["jmeno_druhu_cesky"].ToString(),
                LatinName = dr["jmeno_druhu_latinsky"].ToString(),
                Genus = GenusController.New(dr, authLevel)
            };
        }

        // GET: api/Species
        public IEnumerable<Species> Get()
        {
            List<Species> list = new List<Species>();

            if (IsAuthorized())
            {
                DataTable query = DatabaseController.Query($"SELECT * FROM {TABLE_NAME} JOIN {SUPER_TABLE_NAME} USING ({SUPER_ID_NAME})");
                foreach (DataRow dr in query.Rows)
                {
                    list.Add(New(dr, GetAuthLevel()));
                }
            }

            return list;
        }

        // GET: api/Species/5
        public Species Get(int id)
        {
            if (!IsAuthorized())
            {
                return null;
            }

            if (cachedSpecies[id.ToString()] is Species)
            {
                return cachedSpecies[id.ToString()] as Species;
            }

            DataTable query = DatabaseController.Query($"SELECT * FROM {TABLE_NAME} JOIN {SUPER_TABLE_NAME} USING ({SUPER_ID_NAME}) WHERE {ID_NAME} = :id", new OracleParameter("id", id));

            if (query.Rows.Count != 1)
            {
                return null;
            }

            Species species = New(query.Rows[0], GetAuthLevel());
            cachedSpecies.Add(id.ToString(), species, DateTimeOffset.Now.AddMinutes(15));
            return species;
        }

        [NonAction]
        protected override bool CheckObject(JObject value)
        {
            return ValidJSON(value, "Id", "CzechName", "LatinName", "Genus")
                && int.TryParse(value["Id"].ToString(), out _)
                && value["Genus"].Type == JTokenType.Object
                && GenusController.CheckObjectStatic(value["Genus"].ToObject<JObject>());
        }

        [NonAction]
        protected override int SetObjectInternal(JObject value, AuthLevel authLevel, OracleTransaction transaction)
        {
            Species n = value.ToObject<Species>();

            int id_genus = GenusController.SetObjectStatic(value["Genus"].ToObject<JObject>(), authLevel, transaction);
            if (id_genus == ErrId)
            {
                return ErrId;
            }

            OracleParameter p_id = new OracleParameter("p_id", n.Id);
            DatabaseController.Execute("PKG_MODEL_DML.UPSERT_DRUH", transaction,
                p_id,
                new OracleParameter("p_cesky", n.CzechName),
                new OracleParameter("p_latinsky", n.LatinName),
                new OracleParameter("p_id_rod", id_genus));
            int id = int.Parse(p_id.Value.ToString());

            if (id != ErrId && cachedSpecies.Contains(id.ToString()))
            {
                n.Id = id;
                cachedSpecies[id.ToString()] = n;
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

        // POST: api/Species
        public IHttpActionResult Post([FromBody] JObject value)
        {
            return PostUnknownNumber(value);
        }

        // POST : api/Species/5
        public IHttpActionResult Post(int id, [FromBody] JObject value)
        {
            return PostSingle(id, value);
        }

        // DELETE: api/Species/5
        public IHttpActionResult Delete(int id)
        {
            return DeleteById(TABLE_NAME, ID_NAME, id, cachedSpecies);
        }
    }
}
