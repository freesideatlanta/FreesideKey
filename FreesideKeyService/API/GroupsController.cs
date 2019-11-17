using System;

using System.Collections.Generic;
using System.Web.Http;

using Newtonsoft.Json.Linq;

using FreesideKeyService.FSLocalDb;
using WGToolKit;
using Newtonsoft.Json;

namespace FreesideKeyService
{


    [Route("api/groups/{action}")]
    [AuthRequired]
    public class GroupsController : ApiController
    {

        [HttpPost]
        public JObject ListGroups()
        {
            JObject response = new JObject();


            //Controller Serial Number
            //Number Of controllers
            String ErrorMsg;
            List<KeyDbManager.GroupSummary> groupsList = KeyDbManager.ListGroups(out ErrorMsg);
      
            if(ErrorMsg != null)
            {
                //Add Failed
                response["message"] = ErrorMsg;
                return response;
            }

            response["message"] = $"{groupsList.Count} Groups Found";
            response["groups"] = JToken.FromObject(groupsList);
            return response;
        }

        /// <summary>
        /// Get all Doors Identified By Controller Name, Serial Number, Door Number, Door Name Tuple
        /// </summary>
        [HttpPost]
        public JObject ListGroupPerms()
        {

            JObject result = new JObject();

            String ErrorMsg;
            List<KeyDbManager.ControllerInfo> controllers = KeyDbManager.ListControllers(out ErrorMsg);

            if (ErrorMsg != null)
            {
                result["message"] = ErrorMsg;
                return result;
            }

            List<KeyDbManager.GroupPermDesc> permEntries = new List<KeyDbManager.GroupPermDesc>();
            
            foreach (KeyDbManager.ControllerInfo ci in controllers)
            {
                permEntries.Add(new KeyDbManager.GroupPermDesc(ci.controllerSerial, 1, ci.door1Name));
                permEntries.Add(new KeyDbManager.GroupPermDesc(ci.controllerSerial, 2, ci.door2Name));
                permEntries.Add(new KeyDbManager.GroupPermDesc(ci.controllerSerial, 3, ci.door3Name));
                permEntries.Add(new KeyDbManager.GroupPermDesc(ci.controllerSerial, 4, ci.door4Name));
            }

            result["message"] = $" Doors Found: {permEntries.Count}";
            result["doorPermDesc"] = JToken.FromObject(permEntries);

            return result;
        }



        [HttpPost]
        public JObject CreateGroup([NakedBody] String rawData)
        {
            String ErrorMsg;
            JObject response = new JObject();

            JObject request = JObject.Parse(rawData);

            //Check GroupName
            try
            {
                request = JObject.Parse(rawData);
            }
            catch
            {
                response["message"] = "Invalid JSON Format";
                return response;
            }

            //Check GroupName Format
            if (request["groupName"] == null || request["groupName"].Value<String>() == null || request["groupName"].Value<String>() == "" || request["groupName"].Value<String>().Length > 64)
            {
                response["message"] = "Invalid Group Name";
                return response;
            }


            if (!KeyDbManager.CreateGroup(request["groupName"].Value<String>(), out ErrorMsg))
            {
                response["message"] = ErrorMsg;
                response["groupName"] = request["groupName"].Value<String>();
            }


            response["message"] = "Create Group Success";
            response["groupName"] = request["groupName"].Value<String>();

            return response;
        }


        [HttpPost]
        public JObject UpdateGroupPerms([NakedBody] String rawData)
        {
            String ErrorMsg;
            JObject response = new JObject();

            JObject request = JObject.Parse(rawData);

            Int32 groupKey = request["groupKey"].Value<Int32>();


            List<KeyDbManager.GroupPermEntry> newEntrys = new List<KeyDbManager.GroupPermEntry>();

            JsonConvert.PopulateObject(request["newPerms"].ToString(), newEntrys);

            if (!KeyDbManager.UpdatePerms(groupKey, newEntrys, out ErrorMsg))
            {
                response["message"] = ErrorMsg;
                response["groupKey"] = groupKey;
            }


            response["message"] = "Update Group Success";
            response["groupKey"] = groupKey;

            return response;
        }


        [HttpPost]
        public JObject DeleteGroup([NakedBody] String rawData)
        {
            String ErrorMsg;
            JObject response = new JObject();

            JObject request = JObject.Parse(rawData);

            Int32 groupKey = request["groupKey"].Value<Int32>();


            if (!KeyDbManager.DeleteGroup(groupKey, out ErrorMsg))
            {
                response["message"] = ErrorMsg;
                response["groupKey"] = groupKey;
            }

            response["message"] = "Delete Group Success";
            response["groupKey"] = groupKey;

            return response;
        }


    }
}
