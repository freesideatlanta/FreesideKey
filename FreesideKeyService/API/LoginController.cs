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



    [Route("api/login")]
    public class LoginController : ApiController
    {

        [HttpPost]
        public JObject Post()
        {
            JObject request = JObject.Parse(Request.Content.ReadAsStringAsync().Result);

            JObject response = new JObject();


            //Input Validation
            JToken jUsername;
            if (!request.TryGetValue("username", out jUsername))
            {
                response.Add(new JProperty("message", "Error: Username Not Sent"));
                return response;
            }


            JToken jPassword;
            if (!request.TryGetValue("password", out jPassword))
            {
                response.Add(new JProperty("message", "Error: Password Not Sent"));
                return response;
            }

            //Validate Domain
            String joinedDomain = "";
            try { joinedDomain = Domain.GetComputerDomain().Name; } catch (Exception e) { };

            String localDomain = System.Environment.MachineName;

            JToken jDomain;
            if (request.TryGetValue("domain", out jDomain))
            {
                if ((jDomain.ToString() != joinedDomain) && jDomain.ToString() != localDomain)
                {
                    response.Add(new JProperty("message", "Error: Domain Not Available"));
                    return response;
                }
            }

            //User Authentication
            PrincipalContext pc;
            if ((jDomain == null) || (jDomain.ToString() == localDomain))
                pc = new PrincipalContext(ContextType.Machine);
            else
                pc = new PrincipalContext(ContextType.Domain, joinedDomain);


            if (!pc.ValidateCredentials(jUsername.ToString(), jPassword.ToString()))
            {
                response.Add(new JProperty("message", $"Login Failed"));
                return response;
            }

            UserPrincipal user = UserPrincipal.FindByIdentity(pc, jUsername.ToString());

            PrincipalSearchResult<Principal> groups = user.GetAuthorizationGroups();

            //Check If has Administrate Rights
            bool hasAdminRights = false;
            foreach (Principal p in groups)
            {
                // make sure to add only group principals
                if (p is GroupPrincipal)
                    if (p.Name == "Administrators")
                        hasAdminRights = true;
            }

            if(!hasAdminRights)
            {
                response.Add(new JProperty("message", $"Login Failed. User is Not an Administrator"));
                return response;
            }

            //Get API KEy
            String ErrorMsg;
            KeyDbManager.TokenResponse apiToken;
            if ((jDomain == null) || (jDomain.ToString() == localDomain))
                apiToken = KeyDbManager.GetApiToken(localDomain + '\\' + user.SamAccountName, user.Sid.Value, out ErrorMsg);
            else
                apiToken = KeyDbManager.GetApiToken(joinedDomain + '\\' + user.SamAccountName, user.Sid.Value, out ErrorMsg);


            if (ErrorMsg != null)
            {
                response.Add(new JProperty("message", $"Login Failed: {ErrorMsg}"));
                return response;
            }


            response.Add(new JProperty("message", $"Login Success."));
            response.Add(new JProperty("apiToken", apiToken.apiToken));

            return response;

        }
    }
}
