using System;

using System.Collections.Generic;
using System.Web.Http;
using Newtonsoft.Json.Linq;

using FreesideKeyService.FSLocalDb;
using WGToolKit;

namespace FreesideKeyService
{

    [Route("api/server/{action}")]
    [AuthRequired]
    public class ServerController : ApiController
    {

        [HttpPost]
        public JObject ValidateDB()
        {
            JObject request = JObject.Parse(Request.Content.ReadAsStringAsync().Result);

            JObject response = new JObject();

            String ErrorMsg;

            //Check DB
            if(!KeyDbManager.ValidateDB(out ErrorMsg))
            {
                //Response if DB Not Initialized. Should only be if something has gone horribly wrong.
                response.Add(new JProperty("message", $"DB Validate Failed: {ErrorMsg}"));
                response.Add(new JProperty("isValid", false));
                return response;
            }

            //Success Response
            response.Add(new JProperty("message", $"DB Validate Success"));
            response.Add(new JProperty("isValid", true));
            return response;
        }

 

        [HttpPost]
        //TODO: Add Support For NonStandard Port Selection
        public JObject SearchControllers()
        {
            JObject response = new JObject();

            //Controller Serial Number
            //Number Of Controllers
            List<WGController> controllers = WGController.ScanNet(Properties.Settings.Default.controllerPort);

            if(controllers == null)
            {
                response.Add(new JProperty("message", "No controllers Found"));
                response.Add(new JProperty("controllers", null));
                return response;
            }

            response.Add(new JProperty("message", $"{controllers.Count} controllers found"));
            List<UInt16> serialNumbers = new List<UInt16>();

            foreach (WGController c in controllers)
                serialNumbers.Add(c.Connection.ID);

            response.Add(new JProperty("controllers", serialNumbers));
            return response;
            
        }
    }
}
