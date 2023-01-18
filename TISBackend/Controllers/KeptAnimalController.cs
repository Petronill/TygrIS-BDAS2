using Oracle.ManagedDataAccess.Client;
using System.Collections.Generic;
using System.Data;
using System.Runtime.Caching;
using System.Web.Http;
using TISBackend.Db;
using TISModelLibrary;

namespace TISBackend.Controllers
{
    public class KeptAnimalController : TISControllerWithInt
    {
        public const string TABLE_NAME = "ZVIRATA";
        public const string ID_NAME = "id_zvire";
        public const string ID_OSETROVATEL = "id_osetrovatel";

        protected static readonly ObjectCache cachedAnimals = MemoryCache.Default;

        [Route("api/all/id/keptanimal")]
        public IEnumerable<int> GetAllIds()
        {
            List<int> list = new List<int>();

            if (IsAuthorized())
            {
                DataTable query = DatabaseController.Query($"SELECT {ID_NAME} FROM {TABLE_NAME} WHERE {ID_OSETROVATEL} IS NOT NULL");
                foreach (DataRow dr in query.Rows)
                {
                    list.Add(int.Parse(dr[ID_NAME].ToString()));
                }
            }

            return list;
        }

        [Route("api/all/keptanimal")]
        public IEnumerable<Animal> GetAll()
        {
            List<Animal> list = new List<Animal>();

            DataTable query = DatabaseController.Query($"SELECT t1.*, t2.*, t3.*, t4.*, t5.*, t6.*, t6.nazev AS nazev2 FROM {TABLE_NAME} t1 " +
                $"JOIN DRUHY t2 ON t1.id_druh = t2.id_druh " +
                $"JOIN RODY t3 ON t2.id_rod = t3.id_rod " +
                $"JOIN POHLAVI t4 ON t1.id_pohlavi = t4.id_pohlavi " +
                $"LEFT JOIN VYBEHY t5 ON t1.id_vybeh = t5.id_vybeh " +
                $"LEFT JOIN PAVILONY t6 ON t5.id_pavilon = t6.id_pavilon " +
                $"WHERE t1.{ID_OSETROVATEL} IS NOT NULL");
            foreach (DataRow dr in query.Rows)
            {
                list.Add(AnimalController.New(dr, GetAuthLevel()));
            }

            return list;
        }

        [Route("api/id/keptanimal/{id}")]
        public IEnumerable<int> GetIds(int id)
        {
            List<int> list = new List<int>();

            if (IsAuthorized())
            {
                DataTable query = DatabaseController.Query($"SELECT {ID_NAME} FROM {TABLE_NAME} WHERE {ID_OSETROVATEL} = :id", new OracleParameter("id", id));
                foreach (DataRow dr in query.Rows)
                {
                    list.Add(int.Parse(dr[ID_NAME].ToString()));
                }
            }

            return list;
        }

        // GET: api/KeptAnimal/5
        public IEnumerable<Animal> Get(int id)
        {
            List<Animal> list = new List<Animal>();

            DataTable query = DatabaseController.Query($"SELECT t1.*, t2.*, t3.*, t4.*, t5.*, t6.*, t6.nazev AS nazev2 FROM {TABLE_NAME} t1 " +
                $"JOIN DRUHY t2 ON t1.id_druh = t2.id_druh " +
                $"JOIN RODY t3 ON t2.id_rod = t3.id_rod " +
                $"JOIN POHLAVI t4 ON t1.id_pohlavi = t4.id_pohlavi " +
                $"LEFT JOIN VYBEHY t5 ON t1.id_vybeh = t5.id_vybeh " +
                $"LEFT JOIN PAVILONY t6 ON t5.id_pavilon = t6.id_pavilon " +
                $"WHERE t1.{ID_OSETROVATEL}  = :id", new OracleParameter("id", id));
            foreach (DataRow dr in query.Rows)
            {
                list.Add(AnimalController.New(dr, GetAuthLevel()));
            }

            return list;
        }
    }
}
