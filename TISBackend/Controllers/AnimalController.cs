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
    public class AnimalController : TISControllerWithInt
    {
        public const string TABLE_NAME = "ZVIRATA";
        public const string ID_NAME = "id_zvire";

        protected static readonly ObjectCache cachedAnimals = MemoryCache.Default;

        private static readonly AnimalController instance = new AnimalController();

        [NonAction]
        public static Animal New(DataRow dr, AuthLevel authLevel, string idName = AnimalController.ID_NAME)
        {
            return new Animal()
            {
                Id = int.Parse(dr[idName].ToString()),
                Name = dr["jmeno"].ToString(),
                Species = SpeciesController.New(dr, authLevel),
                Sex = SexController.New(dr, authLevel),
                Birth = DateTime.Parse(dr["datum_narozeni"].ToString()),
                Death = (dr["datum_umrti"].ToString() == "") ? null : (DateTime?)DateTime.Parse(dr["datum_umrti"].ToString()),
                Enclosure = (dr["id_vybeh"].ToString() == "") ? null : EnclosureController.New(dr, authLevel, otherNazevName: "nazev2"),
                MaintCosts = int.Parse(dr["naklady"].ToString()),
                KeeperId = (dr["id_osetrovatel"].ToString() == "") ? null : (int?)int.Parse(dr["id_osetrovatel"].ToString()),
                AdopterId = (dr["id_adoptujici"].ToString() == "") ? null : (int?)int.Parse(dr["id_adoptujici"].ToString()),
                PhotoId = (dr["id_foto"].ToString() == "") ? null : (int?)int.Parse(dr["id_foto"].ToString())
            };
        }

        [Route("api/id/animal")]
        public IEnumerable<int> GetIds()
        {
            return GetIds(TABLE_NAME, ID_NAME);
        }

        // GET: api/Animal
        public IEnumerable<Animal> Get()
        {
            List<Animal> list = new List<Animal>();

            if (IsAuthorized())
            {
                DataTable query = DatabaseController.Query($"SELECT t1.*, t2.*, t3.*, t4.*, t5.*, t6.*, t6.nazev AS nazev2 FROM {TABLE_NAME} t1 " +
                    $"JOIN DRUHY t2 ON t1.id_druh = t2.id_druh " +
                    $"JOIN RODY t3 ON t2.id_rod = t3.id_rod " +
                    $"JOIN POHLAVI t4 ON t1.id_pohlavi = t4.id_pohlavi " +
                    $"LEFT JOIN VYBEHY t5 ON t1.id_vybeh = t5.id_vybeh " +
                    $"LEFT JOIN PAVILONY t6 ON t5.id_pavilon = t6.id_pavilon");
                foreach (DataRow dr in query.Rows)
                {
                    list.Add(New(dr, GetAuthLevel()));
                }
            }

            return list;
        }

        // GET: api/Animal/5
        public Animal Get(int id)
        {
            if (!IsAuthorized())
            {
                return null;
            }

            if (cachedAnimals[id.ToString()] is Animal)
            {
                return cachedAnimals[id.ToString()] as Animal;
            }

            DataTable query = DatabaseController.Query($"SELECT t1.*, t2.*, t3.*, t4.*, t5.*, t6.*, t6.nazev AS nazev2 FROM {TABLE_NAME} t1 " +
                    $"JOIN DRUHY t2 ON t1.id_druh = t2.id_druh " +
                    $"JOIN RODY t3 ON t2.id_rod = t3.id_rod " +
                    $"JOIN POHLAVI t4 ON t1.id_pohlavi = t4.id_pohlavi " +
                    $"LEFT JOIN VYBEHY t5 ON t1.id_vybeh = t5.id_vybeh " +
                    $"LEFT JOIN PAVILONY t6 ON t5.id_pavilon = t6.id_pavilon " +
                    $"WHERE {ID_NAME} = :id", new OracleParameter("id", id));

            if (query.Rows.Count != 1)
            {
                return null;
            }

            Animal animal = New(query.Rows[0], GetAuthLevel());
            cachedAnimals.Add(id.ToString(), animal, DateTimeOffset.Now.AddMinutes(15));
            return animal;
        }

        [NonAction]
        protected override bool CheckObject(JObject value, AuthLevel authLevel)
        {
            bool intermediate = ValidJSON(value, "Id", "Name", "Species", "Sex", "Enclosure", "MaintCosts", "Birth", "Death", "KeeperId", "AdopterId", "PhotoId")
                && int.TryParse(value["Id"].ToString(), out _)
                && int.TryParse(value["MaintCosts"].ToString(), out _)
                && DateTime.TryParse(value["Birth"].ToString(), out DateTime birth)
                && (value["Death"].Type == JTokenType.Null || (DateTime.TryParse(value["Death"].ToString(), out DateTime death) && birth.CompareTo(death) <= 0))
                && (value["KeeperId"].Type == JTokenType.Null || int.TryParse(value["KeeperId"].ToString(), out _))
                && (value["AdopterId"].Type == JTokenType.Null || int.TryParse(value["AdopterId"].ToString(), out _))
                && (value["PhotoId"].Type == JTokenType.Null || int.TryParse(value["PhotoId"].ToString(), out _))
                && ((value["Death"].Type == JTokenType.Null && value["KeeperId"].Type != JTokenType.Null && value["Enclosure"].Type != JTokenType.Null)
                    || (value["Death"].Type != JTokenType.Null && value["KeeperId"].Type == JTokenType.Null && value["Enclosure"].Type == JTokenType.Null))
                && SpeciesController.CheckObjectStatic(value["Species"].ToObject<JObject>(), authLevel)
                && SexController.CheckObjectStatic(value["Sex"].ToObject<JObject>(), authLevel);

            if (!intermediate)
            {
                return false;
            }

            JObject enclosure = (value["Enclosure"]?.Type == JTokenType.Object) ? value["Enclosure"].ToObject<JObject>() : null;
            return enclosure == null || EnclosureController.CheckObjectStatic(enclosure, authLevel);
        }

        [NonAction]
        protected override int SetObjectInternal(JObject value, AuthLevel authLevel, OracleTransaction transaction)
        {
            Animal n = value.ToObject<Animal>();

            int id_species = SpeciesController.SetObjectStatic(value["Species"].ToObject<JObject>(), authLevel, transaction);
            if (id_species == ErrId)
            {
                return ErrId;
            }
            int id_sex = SexController.SetObjectStatic(value["Sex"].ToObject<JObject>(), authLevel, transaction);
            if (id_sex == ErrId)
            {
                return ErrId;
            }

            int? id_enclosure = (n.Enclosure != null) ? (int?)EnclosureController.SetObjectStatic(value["Enclosure"].ToObject<JObject>(), authLevel, transaction) : null;
            if (id_enclosure != null && id_enclosure.Value == ErrId)
            {
                return ErrId;
            }

            OracleParameter p_id = new OracleParameter("p_id", n.Id);
            DatabaseController.Execute("PKG_MODEL_DML.UPSERT_ZVIRE", transaction,
                p_id,
                new OracleParameter("p_jmeno", n.Name),
                new OracleParameter("p_id_druh", id_species),
                new OracleParameter("p_id_pohlavi", id_sex),
                new OracleParameter("p_narozeni", n.Birth),
                new OracleParameter("p_umrti", n.Death),
                new OracleParameter("p_id_vybeh", id_enclosure),
                new OracleParameter("p_id_adoptujici", n.AdopterId),
                new OracleParameter("p_id_osetrovatel", n.KeeperId),
                new OracleParameter("p_id_foto", n.PhotoId),
                new OracleParameter("p_naklady", n.MaintCosts)
            );
            int id = int.Parse(p_id.Value.ToString());

            if (id != ErrId && cachedAnimals.Contains(id.ToString()))
            {
                n.Id = id;
                cachedAnimals[id.ToString()] = n;
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

        // POST: api/Animal
        public IHttpActionResult Post([FromBody] JObject value)
        {
            return PostUnknownNumber(value);
        }

        // POST : api/Animal/5
        public IHttpActionResult Post(int id, [FromBody] JObject value)
        {
            return PostSingle(id, value);
        }

        // DELETE: api/Animal/5
        public IHttpActionResult Delete(int id)
        {
            return DeleteById(TABLE_NAME, ID_NAME, id, cachedAnimals);
        }
    }
}
