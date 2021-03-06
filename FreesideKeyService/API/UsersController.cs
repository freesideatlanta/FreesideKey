﻿using System;

using System.Collections.Generic;
using System.Web.Http;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Threading;

using Newtonsoft.Json.Linq;

using FreesideKeyService.FSLocalDb;
using Newtonsoft.Json;

namespace FreesideKeyService
{

    [Route("api/users/{action}")]
    [AuthRequired]
    public class UsersController : ApiController
    {

        


        private class CardScanResult : IEquatable<CardScanResult>
        {
            public Int32 cardNumber;
            public Int32 controllerSerial;
            public Int32 doorIndex;
            public String doorName;

            public CardScanResult(Int32 cardNumber, Int32 controllerSerial, Int32 doorIndex, String doorName)
            {
                this.cardNumber = cardNumber;
                this.controllerSerial = controllerSerial;
                this.doorIndex = doorIndex;
                this.doorName = doorName;
            }

            public bool Equals(CardScanResult other)
            {
                if (other.cardNumber == cardNumber)
                    if (other.controllerSerial == controllerSerial)
                        if (other.doorIndex == doorIndex)
                            if (other.doorName == doorName)
                                return true;

                return false;
            }
        }

        //Shared Scanning
        private static bool watchActive;
        private static List<CardScanResult> scanResults = new List<CardScanResult>();
        private static Stopwatch scanIdle = new Stopwatch();
        private static Stopwatch scanStart = new Stopwatch();
        private static bool scanValid;

   
        [HttpPost]
        public JObject ScanCards()
        {
            //Reset The Stopwatch
            lock(scanIdle)
            {
                if (scanIdle == null)
                    scanIdle = new Stopwatch();

                scanIdle.Restart();
            }

            //Startup the watch thread
            if(!watchActive)
            {
                watchActive = true;
                scanValid = false;
                scanStart.Restart();

                Task t = new Task( () => {
                    //Get Controllers

                    String ErrorMsg;
                    List<KeyDbManager.ControllerInfo> dbControllers = KeyDbManager.ListControllers(out ErrorMsg);

                    //Scan Net For Active Controllers
                    List<WGToolKit.WGController> netControllers = WGToolKit.WGController.ScanNet(FreesideKeyService.Properties.Settings.Default.controllerPort);

                    //Start Watch
                    foreach (WGToolKit.WGController nc in netControllers )
                    {
                        
                        nc.startWatch((Object sender, WGToolKit.ControllerRecord recvRecord) =>
                        {
                            Int32 cardID = (Int32) recvRecord.cardID;
                            Int32 controllerSerial = ((WGToolKit.WGController)sender).Connection.ID;
                            Int32 doorIndex = WGToolKit.WGTools.getDoorFromRecordStatus(recvRecord.cardID, recvRecord.statusByte);

                            String doorName = KeyDbManager.LookupDoorName(controllerSerial, doorIndex, out ErrorMsg);
                            CardScanResult c = new CardScanResult(cardID, controllerSerial, doorIndex, doorName);

                            lock (scanResults)
                            {    
                                if (!scanResults.Contains(c) && scanValid)
                                    scanResults.Add(c);
                            }                           
                        });
                    }



                    //Watch Started. Now Just Wait FOr Idle Timer TO expire and cleanup
                    while (true)
                    {
                        //Timer to burn first two seconds of entries (Stale).
                        if(!scanValid)
                            lock(scanStart)
                            {
                                if (scanStart.ElapsedMilliseconds > 2000)
                                    scanValid = true;
                            }

                        lock (scanIdle)
                        {
                            if (scanIdle.ElapsedMilliseconds > 10000)
                            {
                                //Stop Watch
                                foreach (WGToolKit.WGController nc in netControllers)
                                {
                                    nc.stopWatch();
                                }

                                //Clear Lists;
                                watchActive = false;
                                scanResults.Clear();
                            }
                        }

                        Thread.Sleep(10);
                    }

                });

                t.Start();
            }

            //Reset the scan Timer
            lock(scanIdle)
            {
                scanIdle.Restart();
            }

            //Return List Of collected REsults
            JObject result = new JObject();

            lock (scanResults)
            {
                result["scanResults"] = JToken.FromObject(scanResults);
                result["message"] = $"Scan Results Found: {scanResults.Count}";
            }

            
            return result;

        }

        [HttpPost]
        public JObject AddUser([NakedBody] String rawData)
        {
            String ErrorMsg;

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


            UInt32 cardID;
            if (!UInt32.TryParse(request["cardID"].Value<String>(), out cardID))
            {
                response["message"] = "Invalid Card Number";
                return response;
            }

            
            //Add To Database With Default Perms. (CardActive, No Groups)
            if (!KeyDbManager.AddUser(request["userName"].Value<String>(), cardID, out ErrorMsg))
            {
                //Add Failed
                response["message"] = ErrorMsg;
                return response;
            }


            //Success Response
            response["message"] = "User Add Success";
            response["cardID"] = cardID;
            return response;
        }


        [HttpPost]
        public JObject DeleteUser([NakedBody] String rawData)
        {
            String ErrorMsg;

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


            UInt32 userKey;
            if (!UInt32.TryParse(request["userKey"].Value<String>(), out userKey))
            {
                response["message"] = "Invalid User Key";
                return response;
            }


            //Delete User
            if (!KeyDbManager.DeleteUser(userKey, out ErrorMsg))
            {
                //Delete Failed
                response["message"] = ErrorMsg;
                return response;
            }


            //Success Response
            response["message"] = "Delete User Success";
            response["userKey"] = userKey;
            return response;
        }

        [HttpPost]
        public JObject ListUsers()
        {
            String ErrorMsg;

            JObject response = new JObject();


            //Get List Of Users and Associated Groups
            List<KeyDbManager.UserSummary> userSummary = KeyDbManager.ListUsers(out ErrorMsg);
            if (ErrorMsg != null)
            {
                //List Users Failed
                response["message"] = ErrorMsg;
                return response;
            }

            //Success Response
            response["message"] = $"{userSummary.Count} Users Found";
            response["userSummary"] = JToken.FromObject(userSummary);
            return response;
        }

        [HttpPost]
        public JObject EditUser([NakedBody] String rawData)
        {
            String ErrorMsg;

            //Try to Parse JObject
            JObject request;

            request = JObject.Parse(rawData);
            JObject response = new JObject();


            Int32 userKey = request["userKey"].Value<Int32>();
            String userName = request["userName"].Value<String>();
            Int32 cardID = request["cardID"].Value<Int32>();
            Boolean cardActive = request["cardActive"].Value<Boolean>();

            List<Int32> groupPerms = JsonConvert.DeserializeObject<List<Int32>>(request["groupPerms"].ToString());

            //Add To Database With Default Perms. (CardActive, No Groups)
            if (!KeyDbManager.EditUser(userKey, userName, cardID, groupPerms, cardActive, out ErrorMsg))
            {
                //Add Failed
                response["message"] = ErrorMsg;
                return response;
            }


            //Success Response
            response["message"] = "User Edit Success";
            response["userKey"] = userKey;
            return response;
        }

    }


}
