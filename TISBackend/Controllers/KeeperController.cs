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
    public class KeeperController : TISControllerWithInt
    {
        public const string TABLE_NAME = PersonController.TABLE_NAME;
        public const string ID_NAME = PersonController.ID_NAME;

        protected static readonly ObjectCache cachedKeepers = MemoryCache.Default;

        private static readonly KeeperController instance = new KeeperController();

        [NonAction]
        public static Keeper New(DataRow dr, AuthLevel authLevel, string idName = KeeperController.ID_NAME)
        {
            return new Keeper()
            {
                Id = int.Parse(dr[idName].ToString()),
                FirstName = dr["jmeno"].ToString(),
                LastName = dr["prijmeni"].ToString(),
                PIN = long.Parse(dr["rodne_cislo"].ToString()),
                PhoneNumber = (dr["telefon"].ToString() == "") ? null : (long?)long.Parse(dr["telefon"].ToString()),
                Email = (dr["E-mail"].ToString() == "") ? null : dr["E-mail"].ToString(),
                AccountNumber = (dr["cislo_uctu"].ToString() == "") ? null : (long?)long.Parse(dr["cislo_uctu"].ToString()),
                Address = AddressController.New(dr, authLevel),
                Role = PersonalRoleUtils.FromDbString(dr["role_cloveka"].ToString()),
                PhotoId = (dr["id_foto"].ToString() == "") ? null : (int?)int.Parse(dr["id_foto"].ToString()),
                GrossWage = int.Parse(dr["hruba_mzda"].ToString()),
                SupervisorId = (dr["id_nadrizeny"].ToString() == "") ? null : (int?)int.Parse(dr["id_nadrizeny"].ToString())
            };
        }

        [Route("api/id/keeper")]
        public IEnumerable<int> GetIds()
        {
            List<int> list = new List<int>();

            if (IsAuthorized())
            {
                DataTable query = DatabaseController.Query($"SELECT {ID_NAME} FROM OSETROVATELE");
                foreach (DataRow dr in query.Rows)
                {
                    list.Add(int.Parse(dr[ID_NAME].ToString()));
                }
            }

            return list;
        }

        [Route("api/id/subordinates/{id}")]
        public IEnumerable<int> GetSubordinateIds(int id)
        {
            List<int> list = new List<int>();

            if (HasHigherAuth())
            {
                DataTable query = DatabaseController.Query($"SELECT * FROM (SELECT {ID_NAME} FROM OSETROVATELE START WITH {ID_NAME} = :id CONNECT BY PRIOR {ID_NAME} = id_nadrizeny) WHERE {ID_NAME} != :id", new OracleParameter(":id", id));
                foreach (DataRow dr in query.Rows)
                {
                    list.Add(int.Parse(dr[ID_NAME].ToString()));
                }
            }

            return list;
        }

        [Route("api/subordinates/{id}")]
        public IEnumerable<Keeper> GetSubordinates(int id)
        {
            List<Keeper> list = new List<Keeper>();

            if (HasHigherAuth())
            {
                DataTable query = DatabaseController.Query($"SELECT * FROM (SELECT * FROM OSETROVATELE START WITH {ID_NAME} = :id CONNECT BY PRIOR {ID_NAME} = id_nadrizeny) JOIN {TABLE_NAME} USING (id_clovek) JOIN ADRESY USING (id_adresa) LEFT JOIN DOKUMENTY ON id_foto = id_dokument WHERE {ID_NAME} != :id", new OracleParameter(":id", id));
                foreach (DataRow dr in query.Rows)
                {
                    list.Add(New(dr, GetAuthLevel()));
                }
            }

            return list;
        }

        // GET: api/Keeper
        public IEnumerable<Keeper> Get()
        {
            List<Keeper> list = new List<Keeper>();

            if (IsAuthorized())
            {
                DataTable query = DatabaseController.Query($"SELECT * FROM {TABLE_NAME} JOIN ADRESY USING (id_adresa) JOIN OSETROVATELE USING (id_clovek) LEFT JOIN DOKUMENTY ON id_foto = id_dokument");
                foreach (DataRow dr in query.Rows)
                {
                    list.Add(New(dr, GetAuthLevel()));
                }
            }

            return list;
        }

        // GET: api/Keeper/5
        public Keeper Get(int id)
        {
            if (!IsAuthorized())
            {
                return null;
            }

            if (cachedKeepers[id.ToString()] is Keeper)
            {
                return cachedKeepers[id.ToString()] as Keeper;
            }

            DataTable query = DatabaseController.Query($"SELECT * FROM {TABLE_NAME} JOIN ADRESY USING (id_adresa) JOIN OSETROVATELE USING (id_clovek) LEFT JOIN DOKUMENTY ON id_foto = id_dokument WHERE {ID_NAME} = :id", new OracleParameter("id", id));

            if (query.Rows.Count != 1)
            {
                return null;
            }

            Keeper keeper = New(query.Rows[0], GetAuthLevel());
            cachedKeepers.Add(id.ToString(), keeper, DateTimeOffset.Now.AddMinutes(15));
            return keeper;
        }

        [NonAction]
        protected override bool CheckObject(JObject value, AuthLevel authLevel)
        {
            return PersonController.CheckObjectStatic(value, authLevel) && ValidJSON(value, "GrossWage", "SupervisorId")
                && int.TryParse(value["GrossWage"].ToString(), out _)
                && (value["SupervisorId"].Type == JTokenType.Null || int.TryParse(value["SupervisorId"].ToString(), out _));
        }

        [NonAction]
        protected override int SetObjectInternal(JObject value, AuthLevel authLevel, OracleTransaction transaction)
        {
            Keeper n = value.ToObject<Keeper>();
            if (n.Role != PersonalRoles.KEEPER)
            {
                return ErrId;
            }

            int id_person = PersonController.SetObjectStatic(value, authLevel, transaction);
            if (id_person == ErrId)
            {
                return ErrId;
            }

            OracleParameter p_id = new OracleParameter("p_id", id_person);
            DatabaseController.Execute("PKG_MODEL_DML.UPSERT_OSETROVATEL", transaction,
                p_id,
                new OracleParameter("p_mzda", n.GrossWage),
                new OracleParameter("p_id_nadrizeny", n.SupervisorId));
            int id = int.Parse(p_id.Value.ToString());

            if (id != ErrId && cachedKeepers.Contains(id.ToString()))
            {
                n.Id = id;
                cachedKeepers[id.ToString()] = n;
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

        // POST: api/Keeper
        public IHttpActionResult Post([FromBody] JObject value)
        {
            return IsAdmin() ? PostUnknownNumber(value) : StatusCode(HttpStatusCode.Forbidden);
        }

        // POST : api/Keeper/5
        public IHttpActionResult Post(int id, [FromBody] JObject value)
        {
            return IsAdmin() ? PostSingle(id, value) : StatusCode(HttpStatusCode.Forbidden);
        }

        // DELETE: api/Keeper/5
        public IHttpActionResult Delete(int id)
        {
            return DeleteById(TABLE_NAME, ID_NAME, id, cachedKeepers);
        }
    }
}
