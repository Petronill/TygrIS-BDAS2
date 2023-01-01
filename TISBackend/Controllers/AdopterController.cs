using Oracle.ManagedDataAccess.Client;
using System.Collections.Generic;
using System.Data;
using System.Net;
using System.Web.Http;
using TISBackend.Db;
using TISModelLibrary;

namespace TISBackend.Controllers
{
    public class AdopterController : TISController
    {
        private const string tableName = "LIDE";
        private const string idName = "id_clovek";

        [NonAction]
        public static Adopter New(DataRow dr)
        {
            return new Adopter()
            {
                Id = int.Parse(dr[idName].ToString()),
                FirstName = dr["jmeno"].ToString(),
                SecondName = dr["prijmeni"].ToString(),
                PIN = long.Parse(dr["rodne_cislo"].ToString()),
                PhoneNumber = (dr["telefon"].ToString() == "") ? null : (long?)long.Parse(dr["telefon"].ToString()),
                Email = (dr["E-mail"].ToString() == "") ? null : dr["E-mail"].ToString(),
                AccountNumber = (dr["cislo_uctu"].ToString() == "") ? null : (long?)long.Parse(dr["cislo_uctu"].ToString()),
                Address = AddressController.New(dr),
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
                DataTable query = DatabaseController.Query($"SELECT * FROM {tableName} JOIN ADRESY USING (id_adresa) JOIN ADOPTUJICI USING (id_clovek)");
                foreach (DataRow dr in query.Rows)
                {
                    list.Add(New(dr));
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
            DataRow query = DatabaseController.Query($"SELECT * FROM {tableName} JOIN ADRESY USING (id_adresa) JOIN ADOPTUJICI USING (id_clovek) WHERE {idName} = :id", new OracleParameter("id", id)).Rows[0];
            return New(query);
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
            return DeleteById(tableName, idName, id);
        }
    }
}
