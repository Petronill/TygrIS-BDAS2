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
    public class DiedController : TISControllerWithInt
    {
        public const string TABLE_NAME = "ZVIRATA";
        public const string ID_NAME = "id_zvire";

        protected static readonly ObjectCache cachedDead = MemoryCache.Default;

        [Route("api/id/died")]
        public IEnumerable<int> GetIds()
        {
            List<int> list = new List<int>();

            if (IsAuthorized())
            {
                DataTable query = DatabaseController.Query($"SELECT {ID_NAME} FROM {TABLE_NAME} WHERE datum_umrti IS NOT NULL");
                foreach (DataRow dr in query.Rows)
                {
                    list.Add(int.Parse(dr[ID_NAME].ToString()));
                }
            }

            return list;
        }

        [NonAction]
        public static Animal New(DataRow dr, AuthLevel authLevel, string idName = AnimalController.ID_NAME)
        {
            return new Animal()
            {
                Id = int.Parse(dr[idName].ToString()),
                Name = dr["jmeno"].ToString(),
                Species = SpeciesController.New(dr, authLevel),
                Sex = SexController.New(dr, authLevel),
                Birth = DateTime.Parse(dr["datum_narozeni"].ToString()),
                Death = DateTime.Parse(dr["datum_umrti"].ToString()),
                Enclosure = null,
                MaintCosts = int.Parse(dr["naklady"].ToString()),
                KeeperId = null,
                AdopterId = (dr["id_adoptujici"].ToString() == "") ? null : (int?)int.Parse(dr["id_adoptujici"].ToString()),
                PhotoId = (dr["id_foto"].ToString() == "") ? null : (int?)int.Parse(dr["id_foto"].ToString())
            };
        }

        // GET: api/Died
        public IEnumerable<Animal> Get()
        {
            List<Animal> list = new List<Animal>();

            if (IsAuthorized())
            {
                DataTable query = DatabaseController.Query($"SELECT * FROM {TABLE_NAME} t1 JOIN DRUHY USING (id_druh) JOIN RODY USING (id_rod) JOIN POHLAVI USING (id_pohlavi) WHERE datum_umrti IS NOT NULL");
                foreach (DataRow dr in query.Rows)
                {
                    list.Add(New(dr, GetAuthLevel()));
                }
            }

            return list;
        }

        // POST: api/Died/5
        public IHttpActionResult Post(int id,[FromBody] DateTime date)
        {
            try
            {
                if (!IsAuthorized())
                {
                    return StatusCode(HttpStatusCode.Unauthorized);
                }

                OracleParameter p_id = new OracleParameter("p_id", id);
                DatabaseController.Execute("PKG_MODEL_DML.DEAD_ZVIRE",
                    p_id,
                    new OracleParameter("p_datum", date)
                );

                if (int.Parse(p_id.Value.ToString()) == ErrId)
                {
                    return StatusCode(HttpStatusCode.BadRequest);
                }
                return Content(HttpStatusCode.OK, id);
            }
            catch (Exception ex)
            {
                return Content(HttpStatusCode.InternalServerError, ex.ToString());
            }
        }
    }
}
