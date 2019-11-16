using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;

using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Linq;

using System.Threading;

namespace WGToolKit
{
    public class ConnectionInfo
    {
        public struct ScanResponse
        {
            public IPEndPoint ep;
            public byte[] respBytes;
        };

        bool Configured = false;

        public UInt16 ID;
        public String MAC;

        public string IP_Address;
        public string Netmask;
        public string Gateway;

        public UInt16 udpPort;
        public UInt32 Password;

        public IPAddress HostIP;

        public ConnectionInfo(UInt16 udpPort)
        {
            this.udpPort = udpPort;
        }
    }

    public class ControllerRecord : IEquatable<ControllerRecord>
    {
        public UInt32 cardID;
        public byte statusByte; //num3?
        public byte flagsByte; //num4?
        public byte responseType;

        public DateTime readDateTime;
        public DateTime controllerDateTime;
        public DateTime localTime;

        public bool is10Digit;

        public UInt32 recordIndex;

        public ControllerRecord()
        {
        }



        //Parse Record From Data String
        public static ControllerRecord ParseRecordFromWatch(byte[] recordPayload)
        {
            ControllerRecord newRecord = new ControllerRecord();
            //Response is by byte, 2 digit year, month, weekday, date, hour, min, seconds.
            //Ignore weekday.
            //Format is in BCD
            try
            {
                newRecord.controllerDateTime = new DateTime(2000 +  Int32.Parse(recordPayload[0].ToString("X")),
                                                                    Int32.Parse(recordPayload[1].ToString("X")),
                                                                    Int32.Parse(recordPayload[2].ToString("X")),
                                                                    Int32.Parse(recordPayload[4].ToString("X")),
                                                                    Int32.Parse(recordPayload[5].ToString("X")),
                                                                    Int32.Parse(recordPayload[6].ToString("X")));
            }
            catch (Exception e)   //Catch if Data is out of Range
            {
                return null;
            }

            //I don't know why the record index is split across bytes but it is. I suspect that the high byte is a memory page number.
            //Look The original code was using 1 Indexed VB Strings and Concatenation To do this.

            newRecord.recordIndex = (UInt32)(((UInt16)recordPayload[7]) + ((recordPayload[9] & 0x0F) << 8));
            newRecord.cardID = (UInt16)(((UInt16)recordPayload[7]) + ((recordPayload[9] & 0xF0) << 4));


            //Check if Card Data Available
            if ((WGTools.NetToUInt32(recordPayload, 12) != 0xFFFFFFFF) && (WGTools.NetToUInt32(recordPayload, 16) == 0xFFFFFFFF))
            {
                return null;
            }


            newRecord.responseType = recordPayload[25];

            newRecord.is10Digit = false;
            //Read In Card Data
            if (newRecord.responseType != 0)
            {
                newRecord.cardID = WGTools.NetToUInt32(recordPayload, 12);
                newRecord.statusByte = recordPayload[20];

                newRecord.flagsByte = (byte) ((newRecord.statusByte >= 128) ? 0 : 1);  //Scan Success?
                newRecord.readDateTime = WGTools.NetToDateTime32(recordPayload, 16);

                if (newRecord.readDateTime == DateTime.MinValue)
                    return null;

                if ((newRecord.responseType & 0x08) > 0)
                    newRecord.readDateTime.AddSeconds(1.0);

                if ((newRecord.responseType & 1L) > 0)
                    newRecord.is10Digit = true;

            }
            else
            {
                newRecord.cardID = (UInt32) WGTools.NetToUInt16(recordPayload, 19) + (UInt32) recordPayload[21] * 0x10000;

                newRecord.statusByte = recordPayload[15];
                newRecord.flagsByte = (byte) ((newRecord.statusByte >= 128) ? 0 : 1);  //Scan Success?
                
                newRecord.readDateTime = WGTools.NetToDateTime32(recordPayload, 16);
               
                if (newRecord.readDateTime == DateTime.MinValue)
                    return null;
            }


            return newRecord;
        }

        //Records have 2x records per transaction
        public static ControllerRecord[] ParseRecordFromRecord(byte[] recordPayload)
        {
            ControllerRecord[] newRecords = new ControllerRecord[2];

            //First Record
            //*******************************************

            newRecords[0] = new ControllerRecord();
            newRecords[0].is10Digit = false;

            newRecords[0].cardID = (UInt32)WGTools.NetToUInt16(recordPayload, 0) + (UInt32)recordPayload[2] * 0x10000;

            if (newRecords[0].cardID == 22202125)
                return null;

            newRecords[0].statusByte = recordPayload[3];
            newRecords[0].flagsByte = (byte)((newRecords[0].statusByte >= 128) ? 0 : 1);  //Scan Success? //num5  "Character"

            newRecords[0].readDateTime = WGTools.NetToDateTime32(recordPayload, 4);
            if (newRecords[0].readDateTime == DateTime.MinValue)
                return null;

            //Check if Card Data Available
            if ((WGTools.NetToUInt64(recordPayload, 8) == 0xFFFFFFFFFFFFFFFF))
                return null;
   
            //Get Record ID
            if ((WGTools.NetToUInt32(recordPayload, 8) != 0x00000000) && (WGTools.NetToUInt32(recordPayload, 8) != 0xFFFFFFFF))
                newRecords[0].recordIndex = WGTools.NetToUInt32(recordPayload, 8);
           
            //Record Local Time 
            newRecords[0].localTime = DateTime.Now;

            if (newRecords[0].cardID == 25565535L)  //Special card ID. Not sure what this actually means
                newRecords[0] =  null;


            //Second Record
            //*******************************************

            newRecords[1] = new ControllerRecord();
            newRecords[1].is10Digit = false;

            newRecords[1].cardID = (UInt32)WGTools.NetToUInt16(recordPayload, 12) + (UInt32)recordPayload[14] * 0x10000;

            if (newRecords[1].cardID == 22202125)
            {
                newRecords[1] = null;
                return newRecords;
            }

            newRecords[1].statusByte = recordPayload[15];
            newRecords[1].flagsByte = (byte)((newRecords[1].statusByte >= 128) ? 0 : 1);  //Scan Success? //num5  "Character"

            newRecords[1].readDateTime = WGTools.NetToDateTime32(recordPayload, 16);
            if (newRecords[1].readDateTime == DateTime.MinValue)
            {
                newRecords[1] = null;
                return newRecords;
            }

            //Check if Card Data Available
            if (WGTools.NetToUInt64(recordPayload, 20) == 0xFFFFFFFFFFFFFFFF)
            {
                newRecords[1] = null;
                return newRecords;
            }

            //Get Record ID
            if ((WGTools.NetToUInt32(recordPayload, 20) != 0x00000000) && (WGTools.NetToUInt32(recordPayload, 20) != 0xFFFFFFFF))
                newRecords[1].recordIndex = WGTools.NetToUInt32(recordPayload, 20);

            //Record Local Time 
            newRecords[1].localTime = DateTime.Now;

            if (newRecords[1].cardID == 25565535L)  //Special card ID. Not sure what this actually means
            {
                newRecords[1] = null;
                return newRecords;
            }


            return newRecords;

        }


        //************************************************************************
        //IComparable Interface Implementation. Basically Iggnore the SystemTime
        //************************************************************************

        public bool Equals(ControllerRecord other)
        {
            if (other == null)
                return false;

            if (this.cardID == other.cardID)
                if (this.statusByte == other.statusByte)
                    if (this.flagsByte == other.flagsByte)
                        if (this.responseType == other.responseType)
                            if (this.readDateTime == other.readDateTime)
                                return true;

            return false;

        }

        public override bool Equals(Object obj)
        {
            if (obj == null)
                return false;

            ControllerRecord watchObject = obj as ControllerRecord;

            if (watchObject == null)
                return false;
            else
                return Equals(watchObject);
        }

        public override int GetHashCode()
        {
            int a = this.cardID.GetHashCode();
            int b = this.statusByte.GetHashCode();
            int c = this.flagsByte.GetHashCode();
            int d = this.responseType.GetHashCode();
            int e = this.readDateTime.GetHashCode();

            return a ^ b ^ c ^ d ^ e;
        }

        public static bool operator ==(ControllerRecord watchRecord1, ControllerRecord watchRecord2)
        {
            if (((object)watchRecord1) == null || ((object)watchRecord2) == null)
                return Object.Equals(watchRecord1, watchRecord2);

            return watchRecord1.Equals(watchRecord2);
        }

        public static bool operator !=(ControllerRecord watchRecord1, ControllerRecord watchRecord2)
        {
            if (((object)watchRecord1) == null || ((object)watchRecord2) == null)
                return !Object.Equals(watchRecord1, watchRecord2);

            return !(watchRecord1.Equals(watchRecord2));

        }
    }


    public class WGController
    {
        //Connection Parameters
        public ConnectionInfo Connection;

        public List<ControllerRecord> WatchRecords;

        WGController()
        {
            Connection = new ConnectionInfo(60000);
            WatchRecords = new List<ControllerRecord>();
        }

        WGController(UInt16 udpPort)
        {
            Connection = new ConnectionInfo(udpPort);
            WatchRecords = new List<ControllerRecord>();
        }

        public static List<WGController> ScanNet(UInt16 udpPort)
        {
            //Get a List of all NICS, and set up a UDPClient ON Each
            NetworkInterface[] nics = NetworkInterface.GetAllNetworkInterfaces();

            List<UdpClient> clients = new List<UdpClient>();

            foreach (NetworkInterface n in nics)
            {

                if (n.NetworkInterfaceType == NetworkInterfaceType.Loopback)
                    continue; //Ignore Loopback Interface
                if (n.OperationalStatus != OperationalStatus.Up)
                    continue; //This adapter is off or not connected

                IPInterfaceProperties ipProps = n.GetIPProperties();
                if (ipProps == null)
                    continue; //IP Not Supported

                IPv4InterfaceProperties ip4Props = ipProps.GetIPv4Properties();
                if (ip4Props == null)
                    continue; //IPv4 Not Supported



                //Build UDPClient List

                foreach (UnicastIPAddressInformation uc in ipProps.UnicastAddresses)
                {
                    if (uc.Address.AddressFamily != AddressFamily.InterNetwork)
                        continue;


                    IPEndPoint listenEP = new IPEndPoint(uc.Address, 0);
                    UdpClient udpClient = new UdpClient(listenEP);

                    clients.Add(udpClient);
                }


            }


            //Broadcast Status Request to All Interfaces

            foreach (UdpClient c in clients)
            {
                byte[] dgram = WGTools.strTobyte("7EFFFF0111000000000000000000000000000000000000000000000000000010020D");

                IPEndPoint endPoint = new IPEndPoint(IPAddress.Broadcast, 60000);
                int snt = c.Send(dgram, dgram.Length, endPoint);

            }

            System.Diagnostics.Stopwatch watch = System.Diagnostics.Stopwatch.StartNew();



            List<ConnectionInfo.ScanResponse> recvResp = new List<ConnectionInfo.ScanResponse>();

            //Listen for 1 second on each interface

            while (watch.ElapsedMilliseconds < 1500)
            {
                foreach (UdpClient c in clients)
                {

                    try
                    {
                        while (true)
                        {

                            //IPEndPoint object will allow us to read datagrams sent from any source.
                            IPEndPoint remEP = new IPEndPoint(IPAddress.Any, 0);
                            c.Client.ReceiveTimeout = 100;
                            byte[] recvBytes = c.Receive(ref remEP);
                           
                            ConnectionInfo.ScanResponse s = new ConnectionInfo.ScanResponse();
                            s.ep = (IPEndPoint)c.Client.LocalEndPoint;
                            s.respBytes = recvBytes;
                            lock (recvResp)
                            {
                                recvResp.Add(s);
                            }
                        }
                    }
                    catch //Task Done if Socket is closed, or other exception
                    { }

                }
            }

            //Close All the Clientss
            foreach (UdpClient c in clients)
                c.Close();

            List<WGController> controllers = new List<WGController>();

            //Parse Received Bytes
            foreach (ConnectionInfo.ScanResponse s in recvResp)
            {
                byte[] buf = s.respBytes;

                if (buf.Length != 34)
                    continue;  //Verify Packet Length

                if (buf[33] != (byte)13 | buf[0] != (byte)126)
                    continue; //Verify Start and End Bytes

                long checksum = 0;

                for (int i = 1; i <= 30; i++)
                    checksum += buf[i];

                //CheckSum
                if (checksum != (buf[32] * 256L + buf[31]))
                    continue;  //Verify Checksum


                string byteString = WGTools.byteTostr(buf);

                string tst = byteString.Substring(4, 4);
                if (byteString.Substring(6, 4) != "0111")
                    continue; //Validate Response Type


                WGController controller = new WGController();
                controller.Connection.ID = WGTools.NetToUInt16(buf, 1);
                controller.Connection.MAC = WGTools.NetToMacString(buf, 5);

                controller.Connection.IP_Address = WGTools.NetToIPString(buf, 11);// $"{buf[12]}.{buf[13]}.{buf[14]}.{buf[15]}";
                controller.Connection.Netmask = WGTools.NetToIPString(buf, 15); //$"{buf[16]}.{buf[17]}.{buf[18]}.{buf[19]}";
                controller.Connection.Gateway = WGTools.NetToIPString(buf, 19); //$"{buf[20]}.{buf[21]}.{buf[22]}.{buf[23]}";


                controller.Connection.udpPort = WGTools.NetToUInt16(buf, 23);
                controller.Connection.Password = WGTools.NetToUInt32(buf, 25);

                controller.Connection.HostIP = s.ep.Address;

                controllers.Add(controller);

            }
            return controllers;
        }

        public void OpenDoor(int doorIndex)
        {
            if (doorIndex < 0 || doorIndex > 3)
                return;

            doorIndex++; //Input Zero Based. Controller Takes 1 Based For This Command.

            byte[] payload = WGTools.strTobyte("0" + doorIndex.ToString() + "01000000000000000000000000000000000000000000000000");


            byte[] response = SendCommand(WGParams.CommCmd.COMM_OPEN_DOOR, payload);
        }


        public void ReadCard(int cardIndex)
        {

            byte[] result = SendCommand(WGParams.CommCmd.COMM_READ_REGISTERCARD, null);
            return;

        }
        //SendCommand Helper
        private byte[] SendCommand(byte[] commCmd, byte[] payload)
        {

            //Standard Command is 34 bytes long. TODO: Add helper for specialized longer commands.
            byte[] dGram = new byte[34];

            //Set Start and End Flags
            dGram[0] = WGParams.CommFlag.COMM_STARTFLAG;
            dGram[33] = WGParams.CommFlag.COMM_ENDFLAG;

            //Set Controller ID
            WGTools.UInt16ToNet(Connection.ID, ref dGram, 1);

            //Copy Command
            Array.Copy(commCmd, 0, dGram, 3, 2);

            //Copy Payload
            if (payload != null)
                Array.Copy(payload, 0, dGram, 5, 26);

            //Calculate Checksum
            UInt16 checksum = 0;

            for (int i = 1; i <= 30; i++)
                checksum += dGram[i];

            //Copy Checksum
            WGTools.UInt16ToNet(checksum, ref dGram, 31);

            //Send Data And Wait For Response
            UdpClient client = new UdpClient(new IPEndPoint(Connection.HostIP, 0));



            IPEndPoint endPoint = new IPEndPoint(IPAddress.Broadcast, 60000);
            int snt = client.Send(dGram, dGram.Length, endPoint);

            byte[] recvBytes;
            try
            {
                //Wait Up to 1 Second for Response
                client.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReceiveTimeout, 1000);
                IPEndPoint remoteEP = new IPEndPoint(IPAddress.Any, 0);

                recvBytes = client.Receive(ref remoteEP);
            }
            catch (Exception x)
            {
                //UDP Failed.
                return null;
            }


            //Verify bytes receive is long enough for our checks
            if (recvBytes.Length <= 8)  //Start/End Flag, ControllerID, Command, CheckSum
                return null;

            //Verify Magic Bytes
            if (recvBytes[0] != WGParams.CommFlag.COMM_STARTFLAG)
                return null;
            if (recvBytes[recvBytes.Length - 1] != WGParams.CommFlag.COMM_ENDFLAG)
                return null;

            //Verify Controller Index
            UInt16 remoteID = WGTools.NetToUInt16(recvBytes, 1);
            if (remoteID != this.Connection.ID)
                return null;

            //Verify Controller Command Response
            UInt16 remoteCmd = WGTools.NetToUInt16(recvBytes, 3);
            UInt16 localCmd = WGTools.NetToUInt16(commCmd, 0);

            if (remoteCmd != localCmd)
                return null;


            //Verify Checksum

            //Calculate Checksum
            UInt16 recvCheckSum = 0;

            for (int i = 1; i <= recvBytes.Length - 4; i++)
                recvCheckSum += recvBytes[i];

            UInt16 recvActCheckSum = WGTools.NetToUInt16(recvBytes, recvBytes.Length - 3);

            if (recvActCheckSum != recvCheckSum)
                return null;


            //TODO: Verify Payload Length based on command

            byte[] recvPayload = new byte[recvBytes.Length - 8];
            Array.Copy(recvBytes, 5, recvPayload, 0, recvPayload.Length);

            return recvPayload;



        }


        Thread watchThread;
        EventWaitHandle watchCancelEvent;

        public void startWatch(EventHandler<ControllerRecord> watchDelegate)
        {
            watchThread = new Thread(() => WatchTask(watchDelegate));
            watchCancelEvent = new EventWaitHandle(false, EventResetMode.ManualReset);
            watchThread.Start();
        }

        public void stopWatch()
        {
            if (watchCancelEvent == null)
                return;
            watchCancelEvent.Set();

            if(!watchThread.Join(250))
                watchThread.Abort();

            return;
            
        }
        
        public void WatchTask(EventHandler<ControllerRecord> watchDelegate)
        {
            //Loop Here and Update 
            const int waitMS = 500;
            //Exit Signal
           

            UInt32 getRecordIndex = 0;
            while (true)
            {

                byte[] sendPayload = new byte[26];

                WGTools.UInt32ToNet(getRecordIndex, ref sendPayload, 0);

                byte[] watchResponse = SendCommand(WGParams.CommCmd.COMM_WATCH, null);

                if (watchResponse == null)
                {
                    if (watchCancelEvent.WaitOne(waitMS))
                        break;
                    else
                        continue;
                }

                ControllerRecord rec = ControllerRecord.ParseRecordFromWatch(watchResponse);

                //Dont record if parse failed
                if (rec == null)
                    continue;

                
                getRecordIndex = rec.recordIndex++;

                lock (WatchRecords)
                {
                    if (!WatchRecords.Contains(rec))
                    {
                        WatchRecords.Add(rec);
                        if (watchDelegate != null)
                            watchDelegate(this, rec);
                    }
                }
            
                

                //Delay Until Next Update Period
                if (watchCancelEvent.WaitOne(waitMS))
                    break;
                else
                    continue;
            }

        }

        //Get Number of Records. -1 If Error
        public int GetNumRecords()
        {
            byte[] recordResponse = SendCommand(WGParams.CommCmd.COMM_GETRECORDNUM, null);
            if (recordResponse == null)
                return -1;


            return (int) WGTools.NetToUInt32(recordResponse, 0);
        }

        public ControllerRecord[] GetOneRecord(UInt32 recordNum)
        {
            byte[] payload = new byte[34];
            WGTools.UInt32ToNet(recordNum, ref payload, 0);

            byte[] recordResponse = SendCommand(WGParams.CommCmd.COMM_GETONERECORD, payload);

            if(recordResponse != null)
                return ControllerRecord.ParseRecordFromRecord(recordResponse);

            return null;
            
        }

        public void ClearRecords()
        {
        }

        public void GetRecordsAndClear()
        {

        }
    }
}

/*
int num1 = checked((int)Math.Round(Conversion.Int(unchecked((double)character / 16.0))));
string str3;
        switch (num1 >= 8 || num1 <= 2 ? checked ((int) Math.Round(Conversion.Int(unchecked ((double) character / 4.0)))) : 0)
        {
          case 0:
            str3 = this.resStr.GetString("strAllowableAccess");
            break;
          case 4:
            str3 = this.resStr.GetString("strSwipeOpen");
            break;
          case 8:
            str3 = this.resStr.GetString("strSwipeClose");
            break;
          case 32:
            str3 = this.resStr.GetString("strDeniedAccess");
            break;
          case 36:
            str3 = this.resStr.GetString("strDeniedAccessNOPRIVILEGE");
            break;
          case 40:
            str3 = this.resStr.GetString("strDeniedAccessERRPASSWORD");
            break;
          case 44:
            str3 = this.resStr.GetString("strDeniedAccessSYSERR");
            break;
          case 48:
            str3 = this.resStr.GetString("strDeniedAccessSPECIAL");
            break;
          case 49:
            str3 = this.resStr.GetString("strDeniedAccessSPECIAL_ANTIBACK");
            break;
          case 50:
            str3 = this.resStr.GetString("strDeniedAccessSPECIAL_MORECARD");
            break;
          case 51:
            str3 = this.resStr.GetString("strDeniedAccessSPECIAL_FIRSTCARD");
            break;
          case 52:
            str3 = this.resStr.GetString("strDeniedAccessDOORNC");
            break;
          case 53:
            str3 = this.resStr.GetString("strDeniedAccessSPECIAL_INTERLOCK");
            break;
          case 56:
            str3 = this.resStr.GetString("strDeniedAccessINVALIDTIMEZONE");
            break;
          default:
            str3 = this.resStr.GetString("strDeniedAccess");
            break;
}




 if (cardid <= iCCardGlobal.SPECIAL_CARD_MAXIMIUM)
        {
          str2 = "";
          this.singleInfo = "";
          long num2 = cardid;
          int num3 = status;
          if (true)
          {
    if (num2 == 5L && num3 % 16 <= 4)
    {
        switch (num3 / 16)
        {
            case 1:
                str2 = this.resStr.GetString("strSuperPasswordOpen");
                this.singleInfo += this.resStr.GetString("strSuperPasswordOpen");
                break;
            case 2:
                str2 = this.resStr.GetString("strSuperPasswordClose");
                this.singleInfo += this.resStr.GetString("strSuperPasswordClose");
                break;
            default:
                str2 = this.resStr.GetString("strSuperPasswordDoorOpen");
                this.singleInfo += this.resStr.GetString("strSuperPasswordDoorOpen");
                break;
        }
        mainControl = (object)str2;
        iCCardGlobal.replaceStr4Control(ref mainControl, false);
        str2 = StringType.FromObject(mainControl);
        str2 = str2 + "\r\n" + this.resStr.GetString("strTime") + ": " + StringType.FromDate(readdate);
        this.singleInfo = this.singleInfo + "-" + Strings.Format((object)readdate, "HH:mm:ss");
        this.dvReaders.RowFilter = "  [f_ControllerID]= " + StringType.FromInteger(controllerID);
        this.dvReaders.Sort = " [f_ReaderNO] ASC";
        if (this.dvReaders.Count >= readerNo)
        {
            string str4 = "0";
            string str5 = StringType.FromObject(this.dvReaders[checked(readerNo - 1)]["f_ReaderNO"]);
            StringType.FromObject(this.dvReaders[checked(readerNo - 1)]["f_ReaderName"]);
            if (str5.IndexOf("1") >= 0)
                str4 = "1";
            else if (str5.IndexOf("2") >= 0)
                str4 = "2";
            else if (str5.IndexOf("3") >= 0)
                str4 = "3";
            else if (str5.IndexOf("4") >= 0)
                str4 = "4";
            this.dvDoors.RowFilter = "  [f_ControllerID]= " + StringType.FromInteger(controllerID) + " AND [f_DoorNO]=" + wgTools.PrepareStr((object)str4, false, "");
            str2 = str2 + "\r\n" + this.resStr.GetString("strAddr") + ": ";
            if (this.dvDoors.Count > 0)
            {
                str2 = StringType.FromObject(ObjectType.StrCatObj((object)str2, this.dvDoors[0]["f_DoorName"]));
                this.singleInfo = StringType.FromObject(ObjectType.StrCatObj(ObjectType.StrCatObj(ObjectType.StrCatObj(ObjectType.StrCatObj(this.dvDoors[0]["f_DoorName"], (object)"["), this.dvReaders[checked(readerNo - 1)]["f_ReaderName"]), (object)"];"), (object)this.singleInfo));
            }
            else
                this.singleInfo = StringType.FromObject(ObjectType.StrCatObj(ObjectType.StrCatObj(ObjectType.StrCatObj((object)"[", this.dvReaders[checked(readerNo - 1)]["f_ReaderName"]), (object)"];"), (object)this.singleInfo));
            str2 = StringType.FromObject(ObjectType.StrCatObj((object)str2, ObjectType.StrCatObj(ObjectType.StrCatObj((object)"(", this.dvReaders[checked(readerNo - 1)]["f_ReaderName"]), (object)")")));
        }
        else
            str2 = str2 + "\r\n" + this.resStr.GetString("strAddr") + ": ";
    }
    else
    {
        if (num2 < 4L)
        {
            bool flag = true;
            string str4 = "";
            switch (num3)
            {
                case 0:
                    str4 = this.resStr.GetString("strPushButton");
                    break;
                case 1:
                    str4 = this.resStr.GetString("strPushButtonOpen");
                    break;
                case 2:
                    str4 = this.resStr.GetString("strPushButtonClose");
                    break;
                case 3:
                    str4 = this.resStr.GetString("strRemoteOpenDoor");
                    break;
                case 4:
                    str4 = this.resStr.GetString("strRemoteOpen");
                    break;
                case 5:
                    str4 = this.resStr.GetString("strRemoteClose");
                    break;
                default:
                    flag = false;
                    break;
            }
            if (flag)
            {
                str2 = this.resStr.GetString("strTime") + ": " + Strings.Format((object)readdate, "yyyy-MM-dd HH:mm:ss");
                str2 = str2 + "\r\n" + this.resStr.GetString("strAddr") + ": ";
                this.dvDoorTable2.RowFilter = "  [f_ControllerID]= " + StringType.FromInteger(controllerID) + " AND [f_DoorNO]=" + wgTools.PrepareStr((object)checked(num2 + 1L), false, "");
                this.singleInfo = StringType.FromObject(ObjectType.StrCatObj(ObjectType.StrCatObj(ObjectType.StrCatObj(ObjectType.StrCatObj(ObjectType.StrCatObj(ObjectType.StrCatObj(ObjectType.StrCatObj(this.dvDoorTable2[0]["f_DoorName"], (object)";["), (object)str4), (object)"] --"), (object)this.resStr.GetString("strTime")), (object)": "), (object)Strings.Format((object)readdate, "yyyy-MM-dd HH:mm:ss")), (object)this.singleInfo));
                str2 = StringType.FromObject(ObjectType.StrCatObj((object)str2, ObjectType.StrCatObj(ObjectType.StrCatObj(ObjectType.StrCatObj(ObjectType.StrCatObj(ObjectType.StrCatObj(this.dvDoorTable2[0]["f_DoorName"], (object)"\r\n"), (object)this.resStr.GetString("strState")), (object)": ["), (object)str4), (object)"]")));
                goto label_118;
            }
        }
        str2 = this.resStr.GetString("strTime") + ": " + Strings.Format((object)readdate, "yyyy-MM-dd HH:mm:ss");
        str2 = str2 + "\r\n" + this.resStr.GetString("strAddr") + ":  ";
        this.dvDoorTable2.RowFilter = "  [f_ControllerID]= " + StringType.FromInteger(controllerID) + " AND [f_DoorNO]=" + wgTools.PrepareStr((object)checked(num2 + 1L), false, "");
        string str5 = "";
        if (num2 == 4L && num3 == 160)
        {
            string str4 = "";
            this.dvDoorTable2.RowFilter = "  [f_ControllerID]= " + StringType.FromInteger(controllerID);
            if (this.dvDoorTable2.Count > 0)
            {
                str4 = StringType.FromObject(this.dvDoorTable2[0]["f_DoorName"]);
                int num4 = checked(this.dvDoorTable2.Count - 1);
                int index = 1;
                while (index <= num4)
                {
                    str4 = StringType.FromObject(ObjectType.StrCatObj((object)(str4 + ","), this.dvDoorTable2[index]["f_DoorName"]));
                    checked { ++index; }
                }
            }
            string str7 = str5 + "--" + this.resStr.GetString("strFire");
            if (StringType.StrCmp(this.strDispWarnInfo, "", false) == 0)
                this.strDispWarnInfo = this.resStr.GetString("strFire") + str4;
            this.singleInfo = str4 + ";[" + this.resStr.GetString("strAlarm") + "]--" + str7 + "--" + this.resStr.GetString("strTime") + ": " + Strings.Format((object)readdate, "yyyy-MM-dd HH:mm:ss");
            str2 = this.resStr.GetString("strTime") + ": " + Strings.Format((object)readdate, "yyyy-MM-dd HH:mm:ss");
            str2 = str2 + "\r\n" + this.resStr.GetString("strAddr") + ":  ";
            str2 += str4;
            str2 = str2 + "\r\n" + this.resStr.GetString("strState") + ": [" + this.resStr.GetString("strAlarm") + "]";
            str2 += str7;
            this.bbtnWarnExistVisible = true;
        }
        else if (num2 == 6L && num3 == 160)
        {
            string str4 = "";
            this.dvDoorTable2.RowFilter = "  [f_ControllerID]= " + StringType.FromInteger(controllerID);
            if (this.dvDoorTable2.Count > 0)
            {
                str4 = StringType.FromObject(this.dvDoorTable2[0]["f_DoorName"]);
                int num4 = checked(this.dvDoorTable2.Count - 1);
                int index = 1;
                while (index <= num4)
                {
                    str4 = StringType.FromObject(ObjectType.StrCatObj((object)(str4 + ","), this.dvDoorTable2[index]["f_DoorName"]));
                    checked { ++index; }
                }
            }
            string str7 = str5 + "--" + this.resStr.GetString("strCloseByForce");
            if (StringType.StrCmp(this.strDispWarnInfo, "", false) == 0)
                this.strDispWarnInfo = this.resStr.GetString("strCloseByForce") + str4;
            this.singleInfo = str4 + ";[" + this.resStr.GetString("strAlarm") + "]--" + str7 + "--" + this.resStr.GetString("strTime") + ": " + Strings.Format((object)readdate, "yyyy-MM-dd HH:mm:ss");
            str2 = this.resStr.GetString("strTime") + ": " + Strings.Format((object)readdate, "yyyy-MM-dd HH:mm:ss");
            str2 = str2 + "\r\n" + this.resStr.GetString("strAddr") + ":  ";
            str2 += str4;
            str2 = str2 + "\r\n" + this.resStr.GetString("strState") + ": [" + this.resStr.GetString("strAlarm") + "]";
            str2 += str7;
            this.bbtnWarnExistVisible = true;
        }
        else if (num2 == 7L)
        {
            string str4 = "";
            this.dvDoorTable2.RowFilter = "  [f_ControllerID]= " + StringType.FromInteger(controllerID);
            if (this.dvDoorTable2.Count > 0)
                str4 = StringType.FromObject(this.dvDoorTable2[0]["f_DoorName"]);
            string str7;
            switch (num3)
            {
                case 161:
                    str7 = "A2 " + this.resStr.GetString("str24Hour");
                    break;
                case 162:
                    str7 = "A3 " + this.resStr.GetString("strEmergencyCall");
                    break;
                default:
                    str7 = "A1 " + this.resStr.GetString("strGuardAgainstTheft");
                    break;
            }
            string str8 = str5 + "--" + str7;
            if (StringType.StrCmp(this.strDispWarnInfo, "", false) == 0)
                this.strDispWarnInfo = str7 + str4;
            this.singleInfo = str4 + ";[" + this.resStr.GetString("strAlarm") + "]--" + str8 + "--" + this.resStr.GetString("strTime") + ": " + Strings.Format((object)readdate, "yyyy-MM-dd HH:mm:ss");
            str2 = this.resStr.GetString("strTime") + ": " + Strings.Format((object)readdate, "yyyy-MM-dd HH:mm:ss");
            str2 = str2 + "\r\n" + this.resStr.GetString("strAddr") + ":  ";
            str2 += str4;
            str2 = str2 + "\r\n" + this.resStr.GetString("strState") + ": [" + this.resStr.GetString("strAlarm") + "]";
            str2 += str8;
            this.bbtnWarnExistVisible = true;
        }
        else if (num2 < 4L && (num3 == 129 || num3 == 130 || num3 == 132))
        {
            str2 = this.resStr.GetString("strTime") + ": " + Strings.Format((object)readdate, "yyyy-MM-dd HH:mm:ss");
            str2 = str2 + "\r\n" + this.resStr.GetString("strAddr") + ":  ";
            this.dvDoorTable2.RowFilter = "  [f_ControllerID]= " + StringType.FromInteger(controllerID) + " AND [f_DoorNO]=" + wgTools.PrepareStr((object)checked(cardid + 1L), false, "");
            string str4 = "";
            if (this.dvDoorTable2.Count > 0)
            {
                if (num3 == 129)
                {
                    str4 = str4 + "--" + this.resStr.GetString("strThreat");
                    if (StringType.StrCmp(this.strDispWarnInfo, "", false) == 0)
                        this.strDispWarnInfo = StringType.FromObject(ObjectType.StrCatObj(this.dvDoorTable2[0]["f_DoorName"], (object)this.resStr.GetString("strThreat")));
                }
                if (num3 == 130)
                {
                    str4 = str4 + "--" + this.resStr.GetString("strLeftOpen");
                    if (StringType.StrCmp(this.strDispWarnInfo, "", false) == 0)
                        this.strDispWarnInfo = StringType.FromObject(ObjectType.StrCatObj(this.dvDoorTable2[0]["f_DoorName"], (object)this.resStr.GetString("strLeftOpen")));
                }
                if (num3 == 132)
                {
                    str4 = str4 + "--" + this.resStr.GetString("strOpenByForce");
                    if (StringType.StrCmp(this.strDispWarnInfo, "", false) == 0)
                        this.strDispWarnInfo = StringType.FromObject(ObjectType.StrCatObj(this.dvDoorTable2[0]["f_DoorName"], (object)this.resStr.GetString("strOpenByForce")));
                }
                this.singleInfo = StringType.FromObject(ObjectType.StrCatObj(ObjectType.StrCatObj(ObjectType.StrCatObj(ObjectType.StrCatObj(ObjectType.StrCatObj(ObjectType.StrCatObj(ObjectType.StrCatObj(ObjectType.StrCatObj(this.dvDoorTable2[0]["f_DoorName"], (object)";["), (object)this.resStr.GetString("strAlarm")), (object)"]--"), (object)str4), (object)"--"), (object)this.resStr.GetString("strTime")), (object)": "), (object)Strings.Format((object)readdate, "yyyy-MM-dd HH:mm:ss")));
                str2 = StringType.FromObject(ObjectType.StrCatObj((object)str2, this.dvDoorTable2[0]["f_DoorName"]));
                str2 = str2 + "\r\n" + this.resStr.GetString("strState") + ": [" + this.resStr.GetString("strAlarm") + "]";
                str2 += str4;
                this.bbtnWarnExistVisible = true;
            }
        }
        else if (num2 >= 8L && num2 <= 15L && num3 == 0)
        {
            bool flag = true;
            int num4 = 0;
            string str4 = "";
            switch (num2)
            {
                case 8:
                    str4 = this.resStr.GetString("strDoorOpen");
                    num4 = 1;
                    break;
                case 9:
                    str4 = this.resStr.GetString("strDoorOpen");
                    num4 = 2;
                    break;
                case 10:
                    str4 = this.resStr.GetString("strDoorOpen");
                    num4 = 3;
                    break;
                case 11:
                    str4 = this.resStr.GetString("strDoorOpen");
                    num4 = 4;
                    break;
                case 12:
                    str4 = this.resStr.GetString("strDoorClosed");
                    num4 = 1;
                    break;
                case 13:
                    str4 = this.resStr.GetString("strDoorClosed");
                    num4 = 2;
                    break;
                case 14:
                    str4 = this.resStr.GetString("strDoorClosed");
                    num4 = 3;
                    break;
                case 15:
                    str4 = this.resStr.GetString("strDoorClosed");
                    num4 = 4;
                    break;
                default:
                    flag = false;
                    break;
            }
            if (flag)
            {
                str2 = this.resStr.GetString("strTime") + ": " + Strings.Format((object)readdate, "yyyy-MM-dd HH:mm:ss");
                str2 = str2 + "\r\n" + this.resStr.GetString("strAddr") + ": ";
                this.dvDoorTable2.RowFilter = "  [f_ControllerID]= " + StringType.FromInteger(controllerID) + " AND [f_DoorNO]=" + wgTools.PrepareStr((object)num4, false, "");
                if (this.dvDoorTable2.Count > 0)
                {
                    this.singleInfo = StringType.FromObject(ObjectType.StrCatObj(ObjectType.StrCatObj(ObjectType.StrCatObj(ObjectType.StrCatObj(ObjectType.StrCatObj(ObjectType.StrCatObj(ObjectType.StrCatObj(this.dvDoorTable2[0]["f_DoorName"], (object)";["), (object)str4), (object)"] --"), (object)this.resStr.GetString("strTime")), (object)": "), (object)Strings.Format((object)readdate, "yyyy-MM-dd HH:mm:ss")), (object)this.singleInfo));
                    str2 = StringType.FromObject(ObjectType.StrCatObj((object)str2, ObjectType.StrCatObj(ObjectType.StrCatObj(ObjectType.StrCatObj(ObjectType.StrCatObj(ObjectType.StrCatObj(this.dvDoorTable2[0]["f_DoorName"], (object)"\r\n"), (object)this.resStr.GetString("strState")), (object)": ["), (object)str4), (object)"]")));
                }
            }
        }
    }
}
}*/
