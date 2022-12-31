using Oracle.ManagedDataAccess.Client;
using System.Collections.Generic;
using System.Data;
using System.Net;
using System.Web.Http;
using TISBackend.Db;
using TISBackend.Models;

namespace TISBackend.Controllers
{
    public class PersonController : TISController
    {
        private const string tableName = "LIDE";
        private const string idName = "id_clovek";

        // GET: api/Person
        public IEnumerable<Person> Get()
        {
            List<Person> list = new List<Person>();

            if (IsAuthorized())
            {
                DataTable query = DatabaseController.Query($"SELECT * FROM {tableName} JOIN ADRESY USING (id_adresa)");
                foreach (DataRow dr in query.Rows)
                {
                    list.Add(new Person()
                    {
                        Id = int.Parse(dr[idName].ToString()),
                        FirstName = dr["jmeno"].ToString(),
                        SecondName = dr["prijmeni"].ToString(),
                        PIN = long.Parse(dr["rodne_cislo"].ToString()),
                        PhoneNumber = (dr["telefon"].ToString() == "") ? null : (long?)long.Parse(dr["telefon"].ToString()),
                        Email = (dr["E-mail"].ToString() == "") ? null : dr["E-mail"].ToString(),
                        AccountNumber = (dr["cislo_uctu"].ToString() == "") ? null : (long?)long.Parse(dr["cislo_uctu"].ToString()),
                        Address = new Address()
                        {
                            Id = int.Parse(dr["id_adresa"].ToString()),
                            Street = (dr["ulice"].ToString() == "") ? null : dr["ulice"].ToString(),
                            HouseNumber = (dr["cislo_popisne"].ToString() == "") ? null : (int?)int.Parse(dr["cislo_popisne"].ToString()),
                            City = (dr["obec"].ToString() == "") ? null : dr["obec"].ToString(),
                            PostalCode = (dr["psc"].ToString() == "") ? null : (int?)int.Parse(dr["psc"].ToString()),
                            Country = (dr["zeme"].ToString() == "") ? null : dr["zeme"].ToString()
                        },
                        Role = PersonalRoleUtils.FromDbString(dr["role_cloveka"].ToString())
                    });
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
            DataRow query = DatabaseController.Query($"SELECT * FROM {tableName} JOIN ADRESY USING (id_adresa) WHERE {idName} = :id", new OracleParameter("id", id)).Rows[0];
            return new Person()
            {
                Id = int.Parse(query[idName].ToString()),
                FirstName = query["jmeno"].ToString(),
                SecondName = query["prijmeni"].ToString(),
                PIN = long.Parse(query["rodne_cislo"].ToString()),
                PhoneNumber = (query["telefon"].ToString() == "") ? null : (long?)long.Parse(query["telefon"].ToString()),
                Email = (query["E-mail"].ToString() == "") ? null : query["E-mail"].ToString(),
                AccountNumber = (query["cislo_uctu"].ToString() == "") ? null : (long?)long.Parse(query["cislo_uctu"].ToString()),
                Address = new Address()
                {
                    Id = int.Parse(query["id_adresa"].ToString()),
                    Street = (query["ulice"].ToString() == "") ? null : query["ulice"].ToString(),
                    HouseNumber = (query["cislo_popisne"].ToString() == "") ? null : (int?)int.Parse(query["cislo_popisne"].ToString()),
                    City = (query["obec"].ToString() == "") ? null : query["obec"].ToString(),
                    PostalCode = (query["psc"].ToString() == "") ? null : (int?)int.Parse(query["psc"].ToString()),
                    Country = (query["zeme"].ToString() == "") ? null : query["zeme"].ToString()
                },
                Role = PersonalRoleUtils.FromDbString(query["role_cloveka"].ToString())
            };
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
            return DeleteById(tableName, idName, id);
        }
    }
}
