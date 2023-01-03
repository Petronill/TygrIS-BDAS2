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
    public class PersonController : TISControllerWithInt
    {
        public const string TABLE_NAME = "LIDE";
        public const string ID_NAME = "id_clovek";

        protected static readonly ObjectCache cachedPeople = MemoryCache.Default;

        private static readonly PersonController instance = new PersonController();

        [NonAction]
        public static Person New(DataRow dr, AuthLevel authLevel, string idName = PersonController.ID_NAME)
        {
            return new Person()
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
                PhotoId = (dr["id_foto"].ToString() == "") ? null : (int?)int.Parse(dr["id_foto"].ToString())
            };
        }

        [Route("api/id/person")]
        public IEnumerable<int> GetIds()
        {
            return GetIds(TABLE_NAME, ID_NAME);
        }

        // GET: api/Person
        public IEnumerable<Person> Get()
        {
            List<Person> list = new List<Person>();

            if (IsAuthorized())
            {
                DataTable query = DatabaseController.Query($"SELECT * FROM {TABLE_NAME} JOIN ADRESY USING (id_adresa)");
                foreach (DataRow dr in query.Rows)
                {
                    list.Add(New(dr, GetAuthLevel()));
                }
            }

            return list;
        }

        // GET: api/Person/5
        public Person Get(int id)
        {
            if (!IsAuthorized())
            {
                return null;
            }

            if (cachedPeople[id.ToString()] is Person)
            {
                return cachedPeople[id.ToString()] as Person;
            }

            DataTable query = DatabaseController.Query($"SELECT * FROM {TABLE_NAME} JOIN ADRESY USING (id_adresa) WHERE {ID_NAME} = :id", new OracleParameter("id", id));
            
            if (query.Rows.Count != 1)
            {
                return null;
            }

            Person person = New(query.Rows[0], GetAuthLevel());
            cachedPeople.Add(id.ToString(), person, DateTimeOffset.Now.AddMinutes(15));
            return person;
        }

        [NonAction]
        protected override bool CheckObject(JObject value)
        {
            return ValidJSON(value, "Id", "FirstName", "LastName", "PIN", "PhoneNumber", "Email", "AccountNumber", "Address", "Role", "PhotoId")
                && int.TryParse(value["Id"].ToString(), out _)
                && long.TryParse(value["PIN"].ToString(), out _)
                && (value["PhoneNumber"].Type == JTokenType.Null || long.TryParse(value["PhoneNumber"].ToString(), out _))
                && (value["AccountNumber"].Type == JTokenType.Null || long.TryParse(value["AccountNumber"].ToString(), out _))
                && (value["PhotoId"].Type == JTokenType.Null || int.TryParse(value["PhotoId"].ToString(), out _))
                && AddressController.CheckObjectStatic(value["Address"].ToObject<JObject>())
                && Enum.TryParse<PersonalRoles>(value["Role"].ToString(), out _);
        }

        [NonAction]
        protected override int SetObjectInternal(JObject value, AuthLevel authLevel, OracleTransaction transaction)
        {
            Person n = value.ToObject<Person>();

            int id_address = AddressController.SetObjectStatic(value["Address"].ToObject<JObject>(), authLevel, transaction);
            if (id_address == ErrId)
            {
                return ErrId;
            }

            OracleParameter p_id = new OracleParameter("p_id", n.Id);
            DatabaseController.Execute("PKG_MODEL_DML.UPSERT_CLOVEK", transaction,
                p_id,
                new OracleParameter("p_jmeno", n.FirstName),
                new OracleParameter("p_prijmeni", n.LastName),
                new OracleParameter("p_rc", n.PIN),
                new OracleParameter("p_tel", n.PhoneNumber),
                new OracleParameter("p_mail", n.Email),
                new OracleParameter("p_cislo_uctu", n.AccountNumber),
                new OracleParameter("p_id_address", id_address),
                new OracleParameter("p_id_foto", n.PhotoId),
                new OracleParameter("p_role", PersonalRoleUtils.ToDbString(n.Role))
            );
            int id = int.Parse(p_id.Value.ToString());

            if (id != ErrId && cachedPeople.Contains(id.ToString()))
            {
                n.Id = id;
                cachedPeople[id.ToString()] = n;
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

        // DELETE: api/Person/5
        public IHttpActionResult Delete(int id)
        {
            return DeleteById(TABLE_NAME, ID_NAME, id, cachedPeople);
        }
    }
}
