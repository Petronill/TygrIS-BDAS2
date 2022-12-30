using Newtonsoft.Json.Linq;
using Oracle.ManagedDataAccess.Client;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Web.Http;
using TISBackend.Auth;
using TISBackend.Db;
using TISBackend.Models;

namespace TISBackend.Controllers
{
    public class PersonController : ApiController
    {
        // GET: api/Person
        public IEnumerable<Person> GetAll()
        {
            AuthLevel level = AuthController.Check(AuthToken.From(Request.Headers));
            DataTable query;
            List<Person> list = new List<Person>();

            switch (level) {
                case AuthLevel.OUTER:
                    query = DatabaseController.Query("SELECT * FROM LIDE");
                    break;
                case AuthLevel.INNER:
                case AuthLevel.ADMIN:
                    query = DatabaseController.Query("SELECT * FROM LIDE");
                    break;
                default:
                    return list;
            }
            
            foreach (DataRow dr in query.Rows)
            {
                list.Add(new Person() { Id = int.Parse(dr["id_clovek"].ToString()), Name = $"{dr["jmeno"]} {dr["prijmeni"]}" });
            }

            return list;
        }
    }
}
