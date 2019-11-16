using System.Collections.Generic;
using System.Web.Http;
using Microsoft.Owin;

using FreesideKeyService.FSLocalDb;
using System;
using Newtonsoft.Json.Linq;
using System.Net.Http.Headers;
using System.Net.Http;

using System.DirectoryServices.AccountManagement;
using System.DirectoryServices.ActiveDirectory;

using WGToolKit;

namespace FreesideKeyService
{

    //Response Data Types
    public class ServerResponse
    {
        public String message;
        public ServerResponse(String _message)
        {
            message = _message;
        }
    }

    public class ApiKeyReponse : ServerResponse
    {
        public List<KeyDbManager.TokenResponse> ApiTokens;
        public ApiKeyReponse(String _message, KeyDbManager.TokenResponse _token) : base (_message)
        {
            ApiTokens = new List<KeyDbManager.TokenResponse>();
            ApiTokens.Add(_token);
        }

        public ApiKeyReponse(String _message, List<KeyDbManager.TokenResponse> _tokenList) : base(_message)
        {
            ApiTokens = _tokenList;
        }
    }


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
        public JObject SearchControllers()
        {
            JObject response = new JObject();

            //Controller Serial Number
            //Number Of Controllers
            List<WGController> controllers = WGController.ScanNet();

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
