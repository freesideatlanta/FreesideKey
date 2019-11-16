using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.VisualBasic;

namespace WGToolKit
{
    public static class WGTools
    {
        public static DateTime NetToDateTime32(byte[] buf, int offset)
        {
            UInt16 YMD = NetToUInt16(buf, offset);

            UInt32 year = (UInt32)2000 + ((YMD & 0xFE00u) >> 9);
            UInt32 month = (UInt32)((YMD & 0x01E0u) >> 5);
            UInt32 day = (UInt32)(YMD & 0x001F);

            UInt16 HMS = NetToUInt16(buf, offset + 2);

            UInt32 hour = (UInt32)((HMS & 0xF800u) >> 11);
            UInt32 minute = (UInt32)((HMS & 0x07E0u) >> 5);
            UInt32 second = (UInt32)(HMS & 0x001F) * 2;

            try
            {
                return new DateTime((int)year, (int)month, (int)day, (int)hour, (int)minute, (int)second);
            }
            catch
            {
                return DateTime.MinValue;
            }
        }

        public static DateTime NetToDateTime16(byte[] buf, int offset)
        {
            UInt16 YMD = NetToUInt16(buf, offset);

            UInt32 year = 2000 + ((YMD & 0xFE00u) >> 9);
            UInt32 month = ((YMD & 0x01E0u) >> 5);
            UInt32 day = (YMD & 0xFE00u);


            try
            {
                return new DateTime((int)year, (int)month, (int)day);
            }
            catch
            {
                return DateTime.MinValue;
            }

        }

        public static UInt16 NetToUInt16(byte[] buf, int offset)
        {
            byte[] buf2 = new byte[2];
            Array.Copy(buf, offset, buf2, 0, 2);
            if (!BitConverter.IsLittleEndian)
                Array.Reverse(buf2);
            return BitConverter.ToUInt16(buf2, 0);

        }

        public static void UInt16ToNet(UInt16 i, ref byte[] outBuff, int offset)
        {

            byte[] buf = BitConverter.GetBytes(i);
            if (!BitConverter.IsLittleEndian)
                Array.Reverse(buf);

            Array.Copy(buf, 0, outBuff, offset, 2);

            return;

        }


        public static UInt64 NetToUInt64(byte[] buf, int offset)
        {
            byte[] buf2 = new byte[8];
            Array.Copy(buf, offset, buf2, 0, 8);
            if (!BitConverter.IsLittleEndian)
                Array.Reverse(buf2);
            return BitConverter.ToUInt64(buf2, 0);
        }

        public static UInt32 NetToUInt32(byte[] buf, int offset)
        {
            byte[] buf2 = new byte[4];
            Array.Copy(buf, offset, buf2, 0, 4);
            if (!BitConverter.IsLittleEndian)
                Array.Reverse(buf2);
            return BitConverter.ToUInt32(buf, 0);
        }

        public static void UInt32ToNet(UInt32 i, ref byte[] outBuff, int offset)
        {

            byte[] buf = BitConverter.GetBytes(i);
            if (!BitConverter.IsLittleEndian)
                Array.Reverse(buf);

            Array.Copy(buf, 0, outBuff, offset, 4);

            return;

        }


        public static string NetToMacString(byte[] buf, int offset)
        {
            return BitConverter.ToString(buf, offset, 6).Replace("-", ":");
        }

        public static string NetToIPString(byte[] buf, int offset)
        {
            return $"{buf[offset]}.{buf[offset + 1]}.{buf[offset + 2]}.{buf[offset + 3]}";
        }
        //public static long strToLng(string strVal)
        //{
        //    long num = 0;
        //    try
        //    {
        //        string str = strVal.PadRight(8, '0');
        //        num = (long)IntegerType.FromString("&H" + Strings.Mid(str, 7, 2) + Strings.Mid(str, 5, 2) + Strings.Mid(str, 3, 2) + Strings.Mid(str, 1, 2));
        //    }
        //    catch (Exception ex)
        //    {
        //        ProjectData.SetProjectError(ex);
        //        ProjectData.ClearProjectError();
        //    }
        //    return num;
        //}


        public static long msToTicks(long ms)
        {
            return checked(ms * 1000L * 10L);
        }

        //Convert String to Byte Array
        public static byte[] strTobyte(string s)
        {
            byte[] dgram = Enumerable.Range(0, s.Length)
                     .Where(x => x % 2 == 0)
                     .Select(x => Convert.ToByte(s.Substring(x, 2), 16))
                     .ToArray();

            return dgram;

        }

        //Convert Byte Array to Hex String
        public static string byteTostr(byte[] ba)
        {
            StringBuilder hex = new StringBuilder(ba.Length * 2);
            foreach (byte b in ba)
                hex.AppendFormat("{0:x2}", b);
            return hex.ToString();
        }

        //Helper Function to Get Door Number.
        //I really have no idea about the logic behind this. Ripped from WCCard
        public static int getDoorFromRecordStatus(UInt16 cardno, byte status)
        {
            bool flag1 = false;
            bool flag2 = false;
            int num = 0;

            num = checked((int)((status & 3L) + 1L));
            switch (cardno)
            {
                case 0:
                case 1:
                case 2:
                case 3:
                    switch (status)
                    {
                        case 0:
                        case 1:
                        case 2:
                            flag1 = true;
                            break;
                        case 3:
                        case 4:
                        case 5:
                            flag1 = true;
                            break;
                        case 129:
                            flag2 = true;
                            break;
                        case 130:
                            flag1 = true;
                            break;
                        case 132:
                            flag1 = true;
                            break;
                    }
                    if (flag1)
                    {
                        //Door number originally looked up from database. Just Call it Door "-1"
                        num = -1;
                        break;
                    }
                    if (flag2)
                    {
                        num = checked((int)(cardno + 1L));
                        break;
                    }
                    break;
                case 4:
                    if (status == 160L)
                    {
                        num = 1;
                        break;
                    }
                    break;
                case 5:
                    switch (status)
                    {
                        case 0:
                        case 1:
                        case 2:
                        case 3:
                            flag2 = true;
                            break;
                        case 16:
                        case 17:
                        case 18:
                        case 19:
                            flag2 = true;
                            break;
                        case 32:
                        case 33:
                        case 34:
                        case 35:
                            flag2 = true;
                            break;
                    }
                    if (flag2)
                    {
                        num = checked((int)((status & 3L) + 1L));
                        break;
                    }
                    break;
                case 6:
                    if (status == 160L)
                    {
                        num = 1;
                        break;
                    }
                    break;
                case 7:
                    if (status == 160L || status == 161L || status == 162L)
                    {
                        num = 1;
                        break;
                    }
                    break;
                case 8:
                case 9:
                case 10:
                case 11:
                case 12:
                case 13:
                case 14:
                case 15:
                    if (status == 0L)
                    {
                        //Door number originally looked up from database. Just Call it Door "-1"
                        num = -1;
                        break;
                    }
                    break;
            }
            if (num > 4)
                num = 0;

            return num;
        }
    }
}
