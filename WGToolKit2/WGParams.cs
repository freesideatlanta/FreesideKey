using System;
using System.Collections.Generic;
using System.Text;

namespace WGToolKit
{
    public static class WGParams
    {
        public static class CommFlag
        {
            public static readonly byte COMM_STARTFLAG = 0x7E;
            public static readonly byte COMM_ENDFLAG = 0x0D;
        }
        public class CommCmd
        {
            public static readonly byte[] COMM_WATCH = { 0x81, 0x10 };                          //Implemented 
            public static readonly byte[] COMM_GETPRODUCTINFO = { 0x82, 0x10 };
            public static readonly byte[] COMM_GETTIME = { 0x8A, 0x10 };
            public static readonly byte[] COMM_SETTIME = { 0x8B, 0x10 };
            public static readonly byte[] COMM_GETRECORDNUM = { 0x8C, 0x10 };                   //Implemented
            public static readonly byte[] COMM_GETONERECORD = { 0x8D, 0x10 };                   //Implemented            
            public static readonly byte[] COMM_CLEAROLDRECORD = { 0x8E, 0x10 };
            public static readonly byte[] COMM_SETCONTROLLERDOOR = { 0x8F, 0x10 };
            public static readonly byte[] COMM_SETREADERPASSWORD = { 0x91, 0x10 };
            public static readonly byte[] COMM_CLEAR_REGISTERCARD = { 0x93, 0x10 };
            public static readonly byte[] COMM_ADD_REGISTERCARD = { 0x94, 0x10 };
            public static readonly byte[] COMM_UPDATENEW_REGISTERCARD = { 0x9B, 0x10 };
            public static readonly byte[] COMM_READ_REGISTERCARD = { 0x95, 0x10 };
            public static readonly byte[] COMM_UPDATE_REGISTERCARD = { 0x9E, 0x10 };
            public static readonly byte[] COMM_READ_CONTROLSEG = { 0x96, 0x10 };
            public static readonly byte[] COMM_UPDATE_CONTROLSEG = { 0x97, 0x10 };
            public static readonly byte[] COMM_CLEAR_CONTROLSEG = { 0x98, 0x10 };
            public static readonly byte[] COMM_WARNRESET = { 0x99, 0x10 };
            public static readonly byte[] COMM_APPERROR_RESET = { 0x9A, 0x10 };
            public static readonly byte[] COMM_WRITE_EEPROM = { 0xF4, 0x10 };
            public static readonly byte[] COMM_WRITE_DATAFLASH = { 0xF5, 0x10 };
            public static readonly byte[] COMM_SPECIAL_PRODUCTFORMAT = { 0xFF, 0x10 };
            public static readonly byte[] COMM_SET_REGISTERCARD_SAME = { 0x05, 0x11 };
            public static readonly byte[] COMM_INSERT_REGISTERCARD_BYSAME = { 0x06, 0x11 };
            public static readonly byte[] COMM_INSERT_REGISTERCARD_AUTO = { 0x07, 0x11 };
            public static readonly byte[] COMM_DELETE_REGISTERCARD_AUTO = { 0x08, 0x11 };
            public static readonly byte[] COMM_INSERT_REGISTERCARD_BYSTART2END = { 0x0A, 0x11 };

            public static readonly byte[] COMM_GET_TCPIPCONFIG = { 0x01, 0x11 };
            public static readonly byte[] COMM_SET_TCPIPCONFIG = { 0x02, 0x11 };
            public static readonly byte[] COMM_RESTOREDEFAULT_TCPIPCONFIG = { 0x03, 0x11 };

            //Not in Original
            public static readonly byte[] COMM_OPEN_DOOR = { 0x9D, 0x10 };

        }

        public class MiscDef
        {

            public const int WGCOMM_OK = 0;
            public const int WGCOMM_BADPORT = -1;
            public const int WGCOMM_OPENFAIL = -5;
            public const int WGCOMM_BADPARM = -7;
            public const int WGCOMM_WIN32FAIL = -8;
            public const int WGCOMM_WRITETIMEOUT = -12;
            public const int WGCOMM_READTIMEOUT = -13;
            public const int WGCOMM_NOCONNECT = -13;
            public const int WGCOMM_FRAMEINVALID = -14;
            public const int WGCOMM_BOARDDISABLED = -15;
            public const int WGCOMM_UNKNOWNFAIL = -16;
            public const int WGCOMM_NOTREADRECORDWG = -17;
            public const int WGCOMM_CONTROLLER_NOEXIST = -19;
            public const int WGCOMM_CERROR_CARD_ID = -41;
            public const int WGCOMM_CERROR_BADPARM = -42;
            public const int WGCOMM_WRONG_COMMAND = -45;
            public const int WGCOMM_FAIL = -53;
            public const int WGCOMM_STOP = -54;
            public const int WGCOMM_WRITEFAIL = -55;
            public const int WGCOMM_LINEERROR = -56;
            public const int WGCOMM_CARD_NOEXIST = -70;
            public const int WGCOMM_CARD_COLLISION = -71;
            public const int WGCOMM_NETCONTROL_WRONG_PASSWORD = -90;
            public const int FRAME_SIZE = 34;
            public const int FRAME_MAIN_MAXLEN = 28;
            public const int FRAME_DATA_MAXLEN = 24;
            public const byte FLAG_LOCATION = 4;
            public const byte VALIDDATALENGTH_LOCATION = 5;
            public const byte VALIDDATASTART_LOCATION = 7;

            public const int STYLE06_V11 = 11;
            public const int STYLE06_V12 = 12;
            public const int STYLE06_V13 = 13;
            public const int STYLE06_V14 = 14;
            public const int STYLE06_V17 = 17;
            public const int STYLE06_V21 = 21;
            public const int STYLE06_V24 = 24;
            public const int STYLE08_V50 = 80;
            public const byte conFrameStart = 126;
            public const byte conFrameEnd = 13;
            public const byte conFrameLength = 34;
            public const string COMM_UDP = "UDP";
            public const string COMM_BROADCAST = "SMALL";
            public const string COMM_IP = "IP";
            public const long COMM_UDP_PORT = 60000;

            public const long DATAFLASH_MAXPAGE = 4095;
            public const long CARDRECORD_MAX = 600000;
            public const int REGCARD_START_PAGE_DEFAULT_CONST_V50 = 544;
            public const int REGCARD_START_PAGE_DEFAULT_SECOND_CONST_V50 = 1304;
            public const int TIMEOUT_TWOSWIPE_FOR_CHECK_INSIDE_BY_SWIPE = 20;
        }

    }
}
