using Newtonsoft.Json.Linq;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Data;
using System.Runtime.Caching;
using System.Web.Http;
using TISBackend.Auth;
using TISBackend.Db;
using TISModelLibrary;

namespace TISBackend.Controllers
{
    public class AddressController : TISControllerWithInt
    {
        public const string TABLE_NAME = "ADRESY";
        public const string ID_NAME = "id_adresa";

        protected static readonly ObjectCache cachedAddresses = MemoryCache.Default;

        private static readonly AddressController instance = new AddressController();

        [NonAction]
        public static Address New(DataRow dr, AuthLevel authLevel, string idName = AddressController.ID_NAME)
        {
            return new Address()
            {
                Id = int.Parse(dr[idName].ToString()),
                Street = (dr["ulice"].ToString() == "") ? null : dr["ulice"].ToString(),
                HouseNumber = (dr["cislo_popisne"].ToString() == "") ? null : (int?)int.Parse(dr["cislo_popisne"].ToString()),
                City = (dr["obec"].ToString() == "") ? null : dr["obec"].ToString(),
                PostalCode = (dr["psc"].ToString() == "") ? null : (int?)int.Parse(dr["psc"].ToString()),
                Country = (dr["zeme"].ToString() == "") ? null : dr["zeme"].ToString()
            };
        }

        // GET: api/Address
        public IEnumerable<Address> Get()
        {
            List<Address> list = new List<Address>();

            if (IsAuthorized())
            {
                DataTable query = DatabaseController.Query($"SELECT * FROM {TABLE_NAME}");
                foreach (DataRow dr in query.Rows)
                {
                    list.Add(New(dr, GetAuthLevel()));
                }
            }

            return list;
        }

        // GET: api/Address/5
        public Address Get(int id)
        {
            if (!IsAuthorized())
            {
                return null;
            }

            if (cachedAddresses[id.ToString()] is Address)
            {
                return cachedAddresses[id.ToString()] as Address;
            }

            DataTable query = DatabaseController.Query($"SELECT * FROM {TABLE_NAME} WHERE {ID_NAME} = :id", new OracleParameter("id", id));

            if (query.Rows.Count != 1)
            {
                return null;
            }

            Address address = New(query.Rows[0], GetAuthLevel());
            cachedAddresses.Add(id.ToString(), address, DateTimeOffset.Now.AddMinutes(15));
            return address;
        }

        [NonAction]
        protected override bool CheckObject(JObject value)
        {
            return ValidJSON(value, "Id") && int.TryParse(value["Id"].ToString(), out _);
        }

        [NonAction]
        protected override int SetObjectInternal(JObject value, AuthLevel authLevel, OracleTransaction transaction)
        {
            Address n = value.ToObject<Address>();
            OracleParameter p_id = new OracleParameter("p_id", n.Id);
            DatabaseController.Execute("PKG_MODEL_DML.UPSERT_ADRESA", transaction,
                p_id,
                new OracleParameter("p_ulice", n.Street),
                new OracleParameter("p_cp", n.HouseNumber),
                new OracleParameter("p_obec", n.City),
                new OracleParameter("p_psc", n.PostalCode),
                new OracleParameter("p_zeme", n.Country)
            );
            int id = int.Parse(p_id.Value.ToString());

            if (id != ErrId && cachedAddresses.Contains(id.ToString()))
            {
                n.Id = id;
                cachedAddresses[id.ToString()] = n;
            }

            return id;
        }

        [NonAction]
        public static bool CheckObjectStatic(JObject value)
        {
            return instance.CheckObject(value);
        }

        [NonAction]
        public static int SetObjectStatic(JObject value, AuthLevel authLevel, OracleTransaction transaction = null)
        {
            return instance.SetObject(value, authLevel, transaction);
        }

        // POST: api/Address
        public IHttpActionResult Post([FromBody] JObject value)
        {
            return PostUnknownNumber(value);
        }

        // POST : api/Address/5
        public IHttpActionResult Post(int id, [FromBody] JObject value)
        {
            return PostSingle(id, value);
        }

        // DELETE: api/Address/5
        public IHttpActionResult Delete(int id)
        {
            return DeleteById(TABLE_NAME, ID_NAME, id, cachedAddresses);
        }
    }
}
