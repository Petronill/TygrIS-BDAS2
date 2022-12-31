using Oracle.ManagedDataAccess.Client;
using System.Data;
using System.Web.Http;
using TISBackend.Auth;
using TISBackend.Db;
using TISModelLibrary;

namespace TISBackend.Controllers
{
    public class UserController : ApiController
    {
        // GET: api/User
        public Person Get()
        {
            AuthToken? authToken = AuthToken.From(Request.Headers);
            if (AuthController.Check(authToken) != AuthLevel.NONE)
            {
                DataTable query = DatabaseController.Query($"SELECT t2.*, t3.* FROM UCTY t1 LEFT JOIN LIDE t2 ON t1.id_clovek = t2.id_clovek JOIN ADRESY t3 ON t2.id_adresa = t3.id_adresa WHERE t1.jmeno = :id", new OracleParameter("id", authToken.Value.Username));
                if (query.Rows.Count > 0)
                {
                    DataRow dr = query.Rows[0];
                    return new Person()
                    {
                        Id = int.Parse(dr["id_clovek"].ToString()),
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
                    };
                }
            }

            return null;
        }

        // GET: api/User/id
        public Person Get(string id)
        {
            if (AuthController.Check(AuthToken.From(Request.Headers)) == AuthLevel.ADMIN)
            {
                DataTable query = DatabaseController.Query($"SELECT t2.*, t3.* FROM UCTY t1 LEFT JOIN LIDE t2 ON t1.id_clovek = t2.id_clovek JOIN ADRESY t3 ON t2.id_adresa = t3.id_adresa WHERE t1.jmeno = :id", new OracleParameter("id", id));
                if (query.Rows.Count > 0)
                {
                    DataRow dr = query.Rows[0];
                    return new Person()
                    {
                        Id = int.Parse(dr["id_clovek"].ToString()),
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
                    };
                }
            }

            return null;
        }
    }
}
