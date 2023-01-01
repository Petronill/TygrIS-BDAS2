using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Data;
using System.Net;
using System.Web.Http;
using TISBackend.Db;
using TISModelLibrary;

namespace TISBackend.Controllers
{
    public class AnimalController : TISController
    {
        private const string tableName = "ZVIRATA";
        private const string idName = "id_zvire";

        [NonAction]
        private static Animal New(DataRow dr, string idName = AnimalController.idName)
        {
            return new Animal()
            {
                Id = int.Parse(dr[idName].ToString()),
                Name = dr["jmeno"].ToString(),
                Species = SpeciesController.New(dr),
                Sex = SexController.New(dr),
                Birth = DateTime.Parse(dr["datum_narozeni"].ToString()),
                Death = (dr["datum_umrti"].ToString() == "") ? null : (DateTime?)DateTime.Parse(dr["datum_umrti"].ToString()),
                Enclosure = (dr["id_vybeh"].ToString() == "") ? null : EnclosureController.New(dr, otherNazevName: "nazev2"),
                MaintCosts = int.Parse(dr["naklady"].ToString()),
                KeeperId = (dr["id_osetrovatel"].ToString() == "") ? null : (int?)int.Parse(dr["id_osetrovatel"].ToString()),
                AdopterId = (dr["id_adoptujici"].ToString() == "") ? null : (int?)int.Parse(dr["id_adoptujici"].ToString())
            };
        }

        // GET: api/Animal
        public IEnumerable<Animal> Get()
        {
            List<Animal> list = new List<Animal>();

            if (IsAuthorized())
            {
                DataTable query = DatabaseController.Query($"SELECT t1.*, t2.*, t3.*, t4.*, t5.*, t6.*, t6.nazev AS nazev2 FROM {tableName} t1 " +
                    $"JOIN DRUHY t2 ON t1.id_druh = t2.id_druh " +
                    $"JOIN RODY t3 ON t2.id_rod = t3.id_rod " +
                    $"JOIN POHLAVI t4 ON t1.id_pohlavi = t4.id_pohlavi " +
                    $"JOIN VYBEHY t5 ON t1.id_vybeh = t5.id_vybeh " +
                    $"LEFT JOIN PAVILONY t6 ON t5.id_pavilon = t6.id_pavilon");
                foreach (DataRow dr in query.Rows)
                {
                    list.Add(New(dr));
                }
            }

            return list;
        }

        // GET: api/Animal/5
        public Animal Get(int id)
        {
            if (!IsAuthorized())
            {
                return null;
            }
            DataRow query = DatabaseController.Query($"SELECT t1.*, t2.*, t3.*, t4.*, t5.*, t6.*, t6.nazev AS nazev2 FROM {tableName} t1 " +
                    $"JOIN DRUHY t2 ON t1.id_druh = t2.id_druh " +
                    $"JOIN RODY t3 ON t2.id_rod = t3.id_rod " +
                    $"JOIN POHLAVI t4 ON t1.id_pohlavi = t4.id_pohlavi " +
                    $"JOIN VYBEHY t5 ON t1.id_vybeh = t5.id_vybeh " +
                    $"LEFT JOIN PAVILONY t6 ON t5.id_pavilon = t6.id_pavilon " +
                    $"WHERE {idName} = :id", new OracleParameter("id", id)).Rows[0];
            return New(query);
        }

        // POST: api/Animal
        public IHttpActionResult Post([FromBody] string value)
        {
            if (!IsAdmin())
            {
                return StatusCode(HttpStatusCode.Unauthorized);
            }

            // TODO

            return StatusCode(HttpStatusCode.OK);
        }

        // DELETE: api/Animal/5
        public IHttpActionResult Delete(int id)
        {
            return DeleteById(tableName, idName, id);
        }
    }
}
