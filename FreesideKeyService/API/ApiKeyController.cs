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
using Newtonsoft.Json;

namespace FreesideKeyService
{



    [Route("api/apikey/{action}")]
    [AuthRequired]
    public class ApiKeyController : ApiController
    {

        [HttpPost]
        public JObject CreateApiKey([NakedBody] String rawData)
        {

          
            JObject response = new JObject();
            String ErrorMsg;

            //Get ApiKey from Auth Header
            String creatorApiToken = Request.Headers.Authorization.Parameter;

            //Try to Parse JObject
            JObject request;
            try
            {
                request = JObject.Parse(rawData);
            } catch (Exception e)
            {
                response["message"] = "Invalid JSON Format";
                return response;
            }

            //Check TokenID Format
            if(request["tokenID"] == null || request["tokenID"].Value<String>() == null || request["tokenID"].Value<String>() == "" || request["tokenID"].Value<String>().Length > 64)
            {
                response["message"] = "Invalid TokenID";
                return response;
            }
 
            //Get Creator Username and Sid Based on APi Key
            KeyDbManager.TokenResponse creatorToken = KeyDbManager.LookupApiToken(creatorApiToken, out ErrorMsg);

            if(creatorToken == null)
            {
                response["message"] = ErrorMsg;
                return response;
            }

            //Create New APi Token
            KeyDbManager.TokenResponse tokenResp = KeyDbManager.CreateAPIKey(creatorToken.userName, creatorToken.userSID, request["tokenID"].Value<String>(), out ErrorMsg);

            if (tokenResp == null)
            {
                response["message"] = ErrorMsg;
                return response;
            }

            //Return New Entry
            response["message"] = "Create ApiToken Success";
            response["tokenData"] = JToken.FromObject(tokenResp);
            return response;
        }



        [HttpPost]
        public JObject DeleteApiKey([NakedBody] String rawData)
        {
            String ErrorMsg;
            JObject response = new JObject();

            JObject request = JObject.Parse(rawData);

            KeyDbManager.TokenResponse tokenResp = KeyDbManager.DeleteAPIKey(request["apiKey"].Value<String>(), out ErrorMsg);

            if (tokenResp == null)
            {
                response["message"] = ErrorMsg;
                return response;
            }

            response["message"] = "Deletet ApiToken Success";
            response["tokenData"] = JToken.FromObject(tokenResp);
            return response;
        }

        [HttpPost]
        public JObject ListApiKeys()
        {
            String ErrorMsg;
            List<KeyDbManager.TokenResponse> tokenResp = KeyDbManager.ListAPIKeys(out ErrorMsg);

            JObject response = new JObject();

            if(ErrorMsg != null)
            {
                response["message"] = ErrorMsg;
                response["apikeys"] = null;
                return response;
            }

            response["message"] = "Success";
            response["apiKeys"] = JToken.FromObject(tokenResp);
            return response;
        }



    }
}
