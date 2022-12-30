using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using TISBackend.Auth;

namespace TISBackend.Controllers
{
    public class LoginController : ApiController
    {
        public bool Post(JObject value)
        {
            return value != null && AuthController.Check(AuthToken.FromJSON(value["auth"] as JObject)) != AuthLevel.NONE;
        }
    }
}
