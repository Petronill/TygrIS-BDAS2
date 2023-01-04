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
    public class AdopterController : TISControllerWithInt
    {
        public const string TABLE_NAME = PersonController.TABLE_NAME;
        public const string ID_NAME = PersonController.ID_NAME;

        protected static readonly ObjectCache cachedAdopters = MemoryCache.Default;

        private static readonly AdopterController instance = new AdopterController();

        [NonAction]
        public static Adopter New(DataRow dr, AuthLevel authLevel, string idName = AdopterController.ID_NAME)
        {
            return new Adopter()
            {
                Id = int.Parse(dr[ID_NAME].ToString()),
                FirstName = dr["jmeno"].ToString(),
                LastName = dr["prijmeni"].ToString(),
                PIN = long.Parse(dr["rodne_cislo"].ToString()),
                PhoneNumber = (dr["telefon"].ToString() == "") ? null : (long?)long.Parse(dr["telefon"].ToString()),
                Email = (dr["E-mail"].ToString() == "") ? null : dr["E-mail"].ToString(),
                AccountNumber = (dr["cislo_uctu"].ToString() == "") ? null : (long?)long.Parse(dr["cislo_uctu"].ToString()),
                Address = AddressController.New(dr, authLevel),
                Role = PersonalRoleUtils.FromDbString(dr["role_cloveka"].ToString()),
                PhotoId = (dr["id_foto"].ToString() == "") ? null : (int?)int.Parse(dr["id_foto"].ToString()),
                Donation = int.Parse(dr["prispevek"].ToString())
            };
        }

        [Route("api/id/adopter")]
        public IEnumerable<int> GetIds()
        {
            List<int> list = new List<int>();

            if (IsAuthorized())
            {
                DataTable query = DatabaseController.Query($"SELECT {ID_NAME} FROM ADOPTUJICI");
                foreach (DataRow dr in query.Rows)
                {
                    list.Add(int.Parse(dr[ID_NAME].ToString()));
                }
            }

            return list;
        }

        // GET: api/Adopter
        public IEnumerable<Adopter> Get()
        {
            List<Adopter> list = new List<Adopter>();

            if (IsAuthorized())
            {
                DataTable query = DatabaseController.Query($"SELECT * FROM {TABLE_NAME} JOIN ADRESY USING (id_adresa) JOIN ADOPTUJICI USING (id_clovek) LEFT JOIN DOKUMENTY ON id_foto = id_dokument");
                foreach (DataRow dr in query.Rows)
                {
                    list.Add(New(dr, GetAuthLevel()));
                }
            }

            return list;
        }

        // GET: api/Adopter/5
        public Adopter Get(int id)
        {
            if (!IsAuthorized())
            {
                return null;
            }

            if (cachedAdopters[id.ToString()] is Adopter)
            {
                return cachedAdopters[id.ToString()] as Adopter;
            }

            DataTable query = DatabaseController.Query($"SELECT * FROM {TABLE_NAME} JOIN ADRESY USING (id_adresa) JOIN ADOPTUJICI USING (id_clovek) LEFT JOIN DOKUMENTY ON id_foto = id_dokument WHERE {ID_NAME} = :id", new OracleParameter("id", id));

            if (query.Rows.Count != 1)
            {
                return null;
            }

            Adopter adopter = New(query.Rows[0], GetAuthLevel());
            cachedAdopters.Add(id.ToString(), adopter, DateTimeOffset.Now.AddMinutes(15));
            return adopter;
        }

        [NonAction]
        protected override bool CheckObject(JObject value)
        {
            return PersonController.CheckObjectStatic(value) && ValidJSON(value, "Donation")
                && int.TryParse(value["Donation"].ToString(), out _);
        }

        [NonAction]
        protected override int SetObjectInternal(JObject value, AuthLevel authLevel, OracleTransaction transaction)
        {
            Adopter n = value.ToObject<Adopter>();
            if (n.Role != PersonalRoles.ADOPTER)
            {
                return ErrId;
            }

            int id_person = PersonController.SetObjectStatic(value, authLevel, transaction);
            if (id_person == ErrId)
            {
                return ErrId;
            }

            OracleParameter p_id = new OracleParameter("p_id", id_person);
            DatabaseController.Execute("PKG_MODEL_DML.UPSERT_ADOPTUJICI", transaction,
                p_id,
                new OracleParameter("p_prispevek", n.Donation));
            int id = int.Parse(p_id.Value.ToString());

            if (id != ErrId && cachedAdopters.Contains(id.ToString()))
            {
                n.Id = id;
                cachedAdopters[id.ToString()] = n;
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

        // POST: api/Adopter
        public IHttpActionResult Post([FromBody] JObject value)
        {
            return PostUnknownNumber(value);
        }

        // POST : api/Adopter/5
        public IHttpActionResult Post(int id, [FromBody] JObject value)
        {
            return PostSingle(id, value);
        }

        // DELETE: api/Adopter/5
        public IHttpActionResult Delete(int id)
        {
            return DeleteById(TABLE_NAME, ID_NAME, id, cachedAdopters);
        }
    }
}
