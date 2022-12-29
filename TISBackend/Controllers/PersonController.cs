using Newtonsoft.Json.Linq;
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

        // POST: api/Person
        public IEnumerable<Person> Post(JObject value)
        {
            List<Person> list = new List<Person>();
            
            if (value == null)
            {
                return list;
            }

            AuthLevel level = AuthController.Check(AuthToken.FromJSON(value["auth"] as JObject));
            string sql;
            DataTable query;

            switch (level) {
                case AuthLevel.OUTER:
                    sql = "SELECT * FROM LIDE";
                    break;
                case AuthLevel.INNER:
                case AuthLevel.ADMIN:
                    sql = "SELECT * FROM LIDE";
                    break;
                default:
                    return list;
            }

            query = DatabaseController.Query(sql);
            foreach (DataRow dr in query.Rows)
            {
                list.Add(new Person() { Id = int.Parse(dr["id_clovek"].ToString()), Name = $"{dr["jmeno"]} {dr["prijmeni"]}" });
            }

            return list;
        }
    }
}
