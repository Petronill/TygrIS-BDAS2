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
                    return PersonController.New(dr);
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
                    return PersonController.New(dr);
                }
            }

            return null;
        }
    }
}
