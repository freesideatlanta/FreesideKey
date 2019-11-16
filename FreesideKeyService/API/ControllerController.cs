using System;

using System.Collections.Generic;
using System.Web.Http;

using Newtonsoft.Json.Linq;

using FreesideKeyService.FSLocalDb;
using WGToolKit;

namespace FreesideKeyService
{


    [Route("api/controller/{action}")]
    [AuthRequired]
    public class ControllerController : ApiController
    {

        private class ControllerScanResponse
        {
            public UInt16 serialNumber;
            public UInt16 doorCount;

            public ControllerScanResponse(UInt16 serialNumber, UInt16 doorCount)
            {
                this.serialNumber = serialNumber;
                this.doorCount = doorCount;
            }
        }



        [HttpPost]
        public JObject ScanControllers()
        {
            JObject response = new JObject();

            //Controller Serial Number
            //Number Of controllers
            List<WGController> controllers = WGController.ScanNet(Properties.Settings.Default.controllerPort);

            if(controllers == null)
            {
                response["message"] = "No controllers Found";
                return response;
            }

            response["message"] = $"{controllers.Count} controllers found";


            List<ControllerScanResponse> scanResults = new List<ControllerScanResponse>();


            foreach (WGController c in controllers)
                scanResults.Add(new ControllerScanResponse(c.Connection.ID, 4));

            response["controllers"] = JToken.FromObject(scanResults);

            return response;
            
        }

        [HttpPost]
        public JObject SaveController([NakedBody] String rawData)
        {
            //Try to Parse JObject
            JObject request;
            JObject response = new JObject();

            try
            {
                request = JObject.Parse(rawData);
            }
            catch
            {
                response["message"] = "Invalid JSON Format";
                return response;
            }


            //Check Serial Number Format
            if (request["serial"] == null )
            {
                response["message"] = "Invalid Serial";
                return response;
            }

            UInt16 serialNum;
            if (!UInt16.TryParse(request["serial"].Value<String>(), out serialNum))
            {
                response["message"] = "Serial Number";
                return response;
            }


            String ErrorMsg;
            //Add To Database With Default Names
            if(!KeyDbManager.AddController("Controller#"+ serialNum, serialNum, serialNum + "-1", serialNum + "-2", serialNum + "-3", serialNum + "-4", out ErrorMsg))
            {
                //Add Failed
                response["message"] = ErrorMsg;
                return response;
            }

            //Success Response
            response["message"] = "Controller Add Success";
            response["controllerSN"] = serialNum;
            return response;

        }


        [HttpPost]
        public JObject DeleteController([NakedBody] String rawData)
        {
            //Try to Parse JObject
            JObject request;
            JObject response = new JObject();

            try
            {
                request = JObject.Parse(rawData);
            }
            catch
            {
                response["message"] = "Invalid JSON Format";
                return response;
            }


            //Check Serial Number Format
            if (request["serial"] == null)
            {
                response["message"] = "Invalid Serial";
                return response;
            }

            UInt16 serialNum;
            if (!UInt16.TryParse(request["serial"].Value<String>(), out serialNum))
            {
                response["message"] = "Invalid Serial";
                return response;
            }


            String ErrorMsg;
            //Add To Database With Default Names
            if (!KeyDbManager.DeleteController(serialNum, out ErrorMsg))
            {
                //Delete Failes
                response["message"] = ErrorMsg;
                return response;
            }

            //Success Response
            response["message"] = "Controller Delete Success";
            response["controllerSN"] = serialNum;
            return response;

        }



        public JObject ListControllers()
        {
            //Try to Parse JObject
            JObject response = new JObject();


            String ErrorMsg;
            //Add To Database With Default Names
            List<KeyDbManager.ControllerInfo> controllers = KeyDbManager.ListControllers(out ErrorMsg);

            if(ErrorMsg != null)
            {
                response["message"] = ErrorMsg;
                return response;
            }

            if(controllers == null || controllers.Count == 0)
            {
                response["message"] = "No Controllers installed";
                return response;
            }

            //Success Response
            response["message"] = controllers.Count + " Controllers Installed";
            response["controllers"] = JToken.FromObject(controllers);
            
            return response;

        }

        [HttpPost]
        public JObject EditController([NakedBody] String rawData)
        {
            //Try to Parse JObject
            JObject request;
            JObject response = new JObject();

            try
            {
                request = JObject.Parse(rawData);
            }
            catch
            {
                response["message"] = "Invalid JSON Format";
                return response;
            }


            //Check Serial Number Format
            if (request["serial"] == null)
            {
                response["message"] = "Invalid Serial";
                return response;
            }

            UInt16 serialNum;
            if (!UInt16.TryParse(request["serial"].Value<String>(), out serialNum))
            {
                response["message"] = "Invalid Serial";
                return response;
            }

            //Check Names
            if(request["controllerName"] == null || request["controllerName"].Value<String>().Length <=0)
            {
                response["message"] = "Invalid Controller Name";
                return response;
            }
            if (request["door1Name"] == null || request["door1Name"].Value<String>().Length <= 0)
            {
                response["message"] = "Invalid Door 1 Name";
                return response;
            }
            if (request["door2Name"] == null || request["door2Name"].Value<String>().Length <= 0)
            {
                response["message"] = "Invalid Door 2 Name";
                return response;
            }
            if (request["door3Name"] == null || request["door3Name"].Value<String>().Length <= 0)
            {
                response["message"] = "Invalid Door 3 Name";
                return response;
            }
            if (request["door4Name"] == null || request["door4Name"].Value<String>().Length <= 0)
            {
                response["message"] = "Invalid Door 4 Name";
                return response;
            }


            String ErrorMsg;
            //Edit
            if (!KeyDbManager.EditController(request["controllerName"].Value<String>(), serialNum, request["door1Name"].Value<String>(), request["door2Name"].Value<String>(), request["door3Name"].Value<String>(), request["door4Name"].Value<String>(), out ErrorMsg))
            {
                //Add Failed
                response["message"] = ErrorMsg;
                return response;
            }

            //Success Response
            response["message"] = "Controller Add Success";
            response["controllerSN"] = serialNum;
            return response;

        }

    }
}
