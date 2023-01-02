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
    public class PersonController : TISControllerWithInt
    {
        public const string TABLE_NAME = "LIDE";
        public const string ID_NAME = "id_clovek";

        protected static readonly ObjectCache cachedPeople = MemoryCache.Default;

        [NonAction]
        public static Person New(DataRow dr, AuthLevel authLevel, string idName = PersonController.ID_NAME)
        {
            return new Person()
            {
                Id = int.Parse(dr[idName].ToString()),
                FirstName = dr["jmeno"].ToString(),
                SecondName = dr["prijmeni"].ToString(),
                PIN = long.Parse(dr["rodne_cislo"].ToString()),
                PhoneNumber = (dr["telefon"].ToString() == "") ? null : (long?)long.Parse(dr["telefon"].ToString()),
                Email = (dr["E-mail"].ToString() == "") ? null : dr["E-mail"].ToString(),
                AccountNumber = (dr["cislo_uctu"].ToString() == "") ? null : (long?)long.Parse(dr["cislo_uctu"].ToString()),
                Address = AddressController.New(dr, authLevel),
                Role = PersonalRoleUtils.FromDbString(dr["role_cloveka"].ToString())
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

        // POST: api/Person
        public IHttpActionResult Post([FromBody] string value)
        {
            if (!IsAdmin())
            {
                return StatusCode(HttpStatusCode.Unauthorized);
            }

            // TODO

            return StatusCode(HttpStatusCode.OK);
        }

        // DELETE: api/Person/5
        public IHttpActionResult Delete(int id)
        {
            return DeleteById(TABLE_NAME, ID_NAME, id, cachedPeople);
        }
    }
}
