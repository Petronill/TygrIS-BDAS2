﻿using Oracle.ManagedDataAccess.Client;
using System.Collections.Generic;
using System.Data;
using System.Net;
using System.Web.Http;
using TISBackend.Db;
using TISModelLibrary;

namespace TISBackend.Controllers
{
    public class EnclosureController : TISController
    {
        private const string tableName = "VYBEHY";
        private const string idName = "id_vybeh";
        private const string superTableName = "PAVILONY";
        private const string superIdName = "id_pavilon";
        private const string otherNazevName = "nazev2";

        [NonAction]
        public static Enclosure New(DataRow dr, string idName = EnclosureController.idName, string superIdName = EnclosureController.superIdName, string otherNazevName = EnclosureController.otherNazevName)
        {
            return new Enclosure()
            {
                Id = int.Parse(dr[idName].ToString()),
                Name = dr["nazev"].ToString(),
                Capacity = int.Parse(dr["kapacita"].ToString()),
                Pavilion = (dr[superIdName].ToString() == "") ? null : new Pavilion()
                {
                    Id = int.Parse(dr[superIdName].ToString()),
                    Name = dr[otherNazevName].ToString()
                }
            };
        }

        // GET: api/Enclosure
        public IEnumerable<Enclosure> Get()
        {
            List<Enclosure> list = new List<Enclosure>();

            if (IsAuthorized())
            {
                DataTable query = DatabaseController.Query($"SELECT t1.*, t2.*, t2.nazev AS {otherNazevName} FROM {tableName} t1 LEFT JOIN {superTableName} t2 ON t1.{superIdName} = t2.{superIdName}");
                foreach (DataRow dr in query.Rows)
                {
                    list.Add(New(dr));
                }
            }

            return list;
        }

        // GET: api/Enclosure/5
        public Enclosure Get(int id)
        {
            if (!IsAuthorized())
            {
                return null;
            }
            DataRow query = DatabaseController.Query($"SELECT t1.*, t2.*, t2.nazev AS {otherNazevName} FROM {tableName} t1 LEFT JOIN {superTableName} t2 ON t1.{superIdName} = t2.{superIdName} WHERE {idName} = :id", new OracleParameter("id", id)).Rows[0];
            return New(query);
        }

        // POST: api/Enclosure
        public IHttpActionResult Post([FromBody] string value)
        {
            if (!IsAdmin())
            {
                return StatusCode(HttpStatusCode.Unauthorized);
            }

            // TODO

            return StatusCode(HttpStatusCode.OK);
        }

        // DELETE: api/Enclosure/5
        public IHttpActionResult Delete(int id)
        {
            return DeleteById(tableName, idName, id);
        }
    }
}
