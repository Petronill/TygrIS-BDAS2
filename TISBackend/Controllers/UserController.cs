using Oracle.ManagedDataAccess.Client;
using System;
using System.Data;
using System.Runtime.Caching;
using TISBackend.Auth;
using TISBackend.Db;
using TISModelLibrary;

namespace TISBackend.Controllers
{
    public class UserController : TISControllerWithString
    {
        protected static readonly ObjectCache cachedUsers = MemoryCache.Default;

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
                    return PersonController.New(dr, AuthController.Check(authToken));
                }
            }

            return null;
        }

        // GET: api/User/id
        public Person Get(string id)
        {
            if (AuthController.Check(AuthToken.From(Request.Headers)) != AuthLevel.ADMIN)
            {
                return null;
            }
            
            if (cachedUsers[id] is Person)
            {
                return cachedUsers[id] as Person;
            }
                
            DataTable query = DatabaseController.Query($"SELECT t2.*, t3.* FROM UCTY t1 LEFT JOIN LIDE t2 ON t1.id_clovek = t2.id_clovek JOIN ADRESY t3 ON t2.id_adresa = t3.id_adresa WHERE t1.jmeno = :id", new OracleParameter("id", id));
            
            if (query.Rows.Count != 1)
            {
                return null;
            }

            Person user = PersonController.New(query.Rows[0], GetAuthLevel());
            cachedUsers.Add(id, user, DateTimeOffset.Now.AddMinutes(15));
            return user;
        }
    }
}
