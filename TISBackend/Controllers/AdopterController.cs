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
    public class AdopterController : TISControllerWithInt
    {
        public const string TABLE_NAME = PersonController.TABLE_NAME;
        public const string ID_NAME = PersonController.ID_NAME;

        protected static readonly ObjectCache cachedAdopters = MemoryCache.Default;

        [NonAction]
        public static Adopter New(DataRow dr, AuthLevel authLevel, string idName = AdopterController.ID_NAME)
        {
            return new Adopter()
            {
                Id = int.Parse(dr[ID_NAME].ToString()),
                FirstName = dr["jmeno"].ToString(),
                SecondName = dr["prijmeni"].ToString(),
                PIN = long.Parse(dr["rodne_cislo"].ToString()),
                PhoneNumber = (dr["telefon"].ToString() == "") ? null : (long?)long.Parse(dr["telefon"].ToString()),
                Email = (dr["E-mail"].ToString() == "") ? null : dr["E-mail"].ToString(),
                AccountNumber = (dr["cislo_uctu"].ToString() == "") ? null : (long?)long.Parse(dr["cislo_uctu"].ToString()),
                Address = AddressController.New(dr, authLevel),
                Role = PersonalRoleUtils.FromDbString(dr["role_cloveka"].ToString()),
                Donation = int.Parse(dr["prispevek"].ToString())
            };
        }

        // GET: api/Adopter
        public IEnumerable<Adopter> Get()
        {
            List<Adopter> list = new List<Adopter>();

            if (IsAuthorized())
            {
                DataTable query = DatabaseController.Query($"SELECT * FROM {TABLE_NAME} JOIN ADRESY USING (id_adresa) JOIN ADOPTUJICI USING (id_clovek)");
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

            DataTable query = DatabaseController.Query($"SELECT * FROM {TABLE_NAME} JOIN ADRESY USING (id_adresa) JOIN ADOPTUJICI USING (id_clovek) WHERE {ID_NAME} = :id", new OracleParameter("id", id));

            if (query.Rows.Count != 1)
            {
                return null;
            }

            Adopter adopter = New(query.Rows[0], GetAuthLevel());
            cachedAdopters.Add(id.ToString(), adopter, DateTimeOffset.Now.AddMinutes(15));
            return adopter;
        }

        // POST: api/Adopter
        public IHttpActionResult Post([FromBody] string value)
        {
            if (!IsAdmin())
            {
                return StatusCode(HttpStatusCode.Unauthorized);
            }

            // TODO

            return StatusCode(HttpStatusCode.OK);
        }

        // DELETE: api/Adopter/5
        public IHttpActionResult Delete(int id)
        {
            return DeleteById(TABLE_NAME, ID_NAME, id, cachedAdopters);
        }
    }
}
