using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Reflection;

using System.Security.Cryptography;

namespace FreesideKeyService.FSLocalDb
{
    public static class KeyDbManager
    {

        //Database constants
        public const string DB_DIRECTORY = "FSKeyDB";
        public const string DB_NAME = "FSKeyPrimary";

        private static string DB_OUTPUT_FOLDER
        {
            get { return Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), DB_DIRECTORY); }
        }
        private static string DB_FILE
        {
            get { return Path.Combine(DB_OUTPUT_FOLDER, DB_NAME + ".mdf"); }
        }
        private static string LOG_FILE
        {
            get { return Path.Combine(DB_OUTPUT_FOLDER, DB_NAME + "_log.ldf"); }
        }


        //Database Setup
        public static bool DeleteDB(out String ErrorMsg)
        {
            ErrorMsg = null;

            //Check if Directory Even Exists
            if (!Directory.Exists(DB_OUTPUT_FOLDER))
                return true;

            //Check if Files Exist. If So Delete Them
            try
            {
                if (File.Exists(DB_FILE))
                {
                    //Disconnect Others before Delete. Sorry, this whole mess is to try and disconnect other sessions
                    try
                    {
                        SqlConnection connection = new SqlConnection($"Data Source=(LocalDB)\\.;AttachDBFileName={DB_FILE};Initial Catalog={DB_NAME};Integrated Security=True;");
                        connection.Open();
                        SqlCommand cmd = connection.CreateCommand();
                        cmd.CommandText = $"USE master";
                        cmd.ExecuteNonQuery();
                        cmd.CommandText = $"ALTER DATABASE {DB_NAME} SET SINGLE_USER WITH ROLLBACK IMMEDIATE;";
                        cmd.ExecuteNonQuery();
                        cmd.CommandText = $"DROP DATABASE {DB_NAME};";
                        cmd.ExecuteNonQuery();
                        cmd.CommandText = $"sys.sp_detach_db '{DB_NAME}'";
                        cmd.ExecuteNonQuery();
                        connection.Close();



                    }
                    catch (Exception e)
                    {
                        try
                        {
                            File.Delete(DB_FILE);
                            return true;
                        }
                        catch { }

                        ErrorMsg = $"DB File Disconnect Failed. Reason: {e.Message}";
                        return false;
                    }
                    //Delete File
                    File.Delete(DB_FILE);
                }

                //Delete Log File
                if (File.Exists(LOG_FILE))
                    File.Delete(LOG_FILE);
            }
            catch (Exception e)
            {
                ErrorMsg = $"Delete File Failed. Reason: {e.Message}";
                return false;
            }
            return true;
        }

        public static bool CreateDB(out String ErrorMsg)
        {
            ErrorMsg = null;

            //Create Directory if Needed
            if (!Directory.Exists(DB_OUTPUT_FOLDER))
                Directory.CreateDirectory(DB_OUTPUT_FOLDER);

            //Cancel If File already Exists
            if (File.Exists(DB_FILE))
            {
                ErrorMsg = $"CreateDB Failed. Reason: DB File Already Exists";
                return false;
            }

            //Create the Databaase
            try
            {
                SqlConnection connection = new SqlConnection($"Data Source=(LocalDB)\\.;Initial Catalog=master;Integrated Security=True");
                connection.Open();

                SqlCommand cmd = connection.CreateCommand();


                //Detatch Existing Database If Present
                cmd.CommandText = $"SELECT database_id FROM sys.databases WHERE Name = '{DB_NAME}'";

                int dbCount = 0;

                if (int.TryParse(cmd.ExecuteScalar()?.ToString(), out dbCount) && (dbCount > 0)) //Some Magic for brevity
                {
                    cmd.CommandText = $"exec sp_detach_db '{DB_NAME}'";
                    cmd.ExecuteNonQuery();
                }

                //Create DB 
                cmd.CommandText = $"CREATE DATABASE {DB_NAME} ON(NAME = N'{DB_NAME}', FILENAME = '{DB_FILE}'";

                cmd.ExecuteNonQuery();

                connection.Close();
            }
            catch (Exception e)
            {
                ErrorMsg = $"CreateDB Failed. Reason: {e.Message}";
                return false;
            }

            //Check File Actually Exists
            if (!File.Exists(DB_FILE))
            {
                ErrorMsg = $"CreateDB Failed. Reason: DB File Not Created";
                return false;
            }

            //DB Successfuly Created
            return true;

        }

        public static bool InitDB(out String ErrorMsg)
        {
            ErrorMsg = null;
            try
            {
                //Open The Database

                SqlConnection connection = new SqlConnection($"Data Source=(LocalDB)\\.;AttachDBFileName={DB_FILE};Initial Catalog={DB_NAME};Integrated Security=True;");
                connection.Open();

                //Create Tables
                SqlCommand cmd = connection.CreateCommand();

                //API Keys Table
                cmd.CommandText = @"CREATE TABLE dbo.ApiKeys (  ApiKey int IDENTITY(1,1) PRIMARY KEY,
                                                                UserName varchar(64) NOT NULL,
                                                                UserSID varchar(64) NOT NULL,
	                                                            TokenID varchar(64) NOT NULL UNIQUE,
	                                                            ApiToken nchar(64) NOT NULL UNIQUE
	                                                         )  ON [PRIMARY]";
                cmd.ExecuteNonQuery();

                //Controllers Table
                cmd.CommandText = @"CREATE TABLE dbo.Controllers (  ControllerKey int IDENTITY(1,1) PRIMARY KEY,
                                                              ControllerName varchar(64) NOT NULL,
                                                              ControllerSN int NOT NULL,
	                                                          Door1Name varchar(64),
                                                              Door2Name varchar(64),
                                                              Door3Name varchar(64),
                                                              Door4Name varchar(64),
	                                                       )  ON [PRIMARY]";
                cmd.ExecuteNonQuery();

                //Groups Table
                cmd.CommandText = @"CREATE TABLE dbo.Groups (   GroupKey int IDENTITY(1,1) PRIMARY KEY,
	                                                            GroupName varchar(64) NOT NULL UNIQUE
	                                                         )  ON [PRIMARY]";
                cmd.ExecuteNonQuery();

                //Group Door Permissions Table
                cmd.CommandText = @"CREATE TABLE dbo.GroupPerms (  GroupKey int NOT NULL,
                                                                   ControllerSN int NOT NULL,
                                                                   DoorIndex int NOT NULL,
	                                                            )  ON [PRIMARY]";
                cmd.ExecuteNonQuery();


                //Users Table
                cmd.CommandText = @"CREATE TABLE dbo.Users (  UserKey int IDENTITY(1,1) PRIMARY KEY,
	                                                          UserName varchar(64) NOT NULL,
                                                              CardID int NOT NULL,  
                                                              CardActive bit NOT NULL
	                                                       )  ON [PRIMARY]";
                cmd.ExecuteNonQuery();

                //Group Membership Table
                cmd.CommandText = @"CREATE TABLE dbo.GroupMembership (  GroupMembership int IDENTITY(1,1) PRIMARY KEY,
	                                                                    UserKey int NOT NULL,
                                                                        GroupKey int NOT NULL  
	                                                                 )  ON [PRIMARY]";
                cmd.ExecuteNonQuery();



#if !_DEBUG

                cmd.CommandText = $"INSERT INTO dbo.ApiKeys ( UserName, UserSID, TokenID, ApiToken ) VALUES ( 'DebugUser', 'DebugSid', 'UserToken', 'AB04493ED7582865B5D31C9B02C315BD5AAA4CE0FBC4CE38BEC1739142412E70' )";
                cmd.ExecuteNonQuery();
#endif

                connection.Close();
            }

            catch (Exception e)
            {
                ErrorMsg = $"CreateTables Failed. Reason: {e.Message}";
            }
            return true;
        }

        private struct ValidateDbResult
        {
            public String TableName;
            public String ColumnName;
            public String DataType;
            public Int32 CharMaxLength;

            public ValidateDbResult(String TableName, String ColumnName, String DataType, Int32 CharMaxLength)
            {
                this.TableName = TableName;
                this.ColumnName = ColumnName;
                this.DataType = DataType;
                this.CharMaxLength = CharMaxLength;
            }

            public override String ToString()
            {
                return $"Table: {TableName}; Column: {ColumnName}; DataType: {DataType}; Capacity: {CharMaxLength}";
            }
        }

        public static bool ValidateDB(out String ErrorMsg)
        {
            ErrorMsg = null;

            try
            {
                //Open The Database

                SqlConnection connection = new SqlConnection($"Data Source=(LocalDB)\\.;AttachDBFileName={DB_FILE};Initial Catalog={DB_NAME};Integrated Security=True;");
                connection.Open();

                //Create Tables
                SqlCommand cmd = connection.CreateCommand();

                cmd.CommandText = @"SELECT TABLE_NAME, COLUMN_NAME, DATA_TYPE, CHARACTER_MAXIMUM_LENGTH FROM INFORMATION_SCHEMA.COLUMNS";

                SqlDataReader rdr = cmd.ExecuteReader();

                List<ValidateDbResult> validateResults = new List<ValidateDbResult>();

                while (rdr.Read())
                {
                    ValidateDbResult result = new ValidateDbResult();
                    result.TableName = rdr.GetString(0);
                    result.ColumnName = rdr.GetString(1);
                    result.DataType = rdr.GetString(2);


                    result.CharMaxLength = rdr.IsDBNull(3) ? -1 : rdr.GetInt32(3);

                    validateResults.Add(result);
                }

                rdr.Close();
                connection.Close();

                List<ValidateDbResult> checkData = new List<ValidateDbResult>();


                //API Keys Table
                checkData.Add(new ValidateDbResult("ApiKeys", "ApiKey", "int", -1));
                checkData.Add(new ValidateDbResult("ApiKeys", "UserName", "varchar", 64));
                checkData.Add(new ValidateDbResult("ApiKeys", "UserSID", "varchar", 64));
                checkData.Add(new ValidateDbResult("ApiKeys", "TokenID", "varchar", 64));
                checkData.Add(new ValidateDbResult("ApiKeys", "ApiToken", "nchar", 64));
                checkData.Add(new ValidateDbResult("Controllers", "ControllerKey", "int", -1));
                checkData.Add(new ValidateDbResult("Controllers", "ControllerName", "varchar", 64));
                checkData.Add(new ValidateDbResult("Controllers", "ControllerSN", "int", -1));
                checkData.Add(new ValidateDbResult("Controllers", "Door1Name", "varchar", 64));
                checkData.Add(new ValidateDbResult("Controllers", "Door2Name", "varchar", 64));
                checkData.Add(new ValidateDbResult("Controllers", "Door3Name", "varchar", 64));
                checkData.Add(new ValidateDbResult("Controllers", "Door4Name", "varchar", 64));
                checkData.Add(new ValidateDbResult("Groups", "GroupKey", "int", -1));
                checkData.Add(new ValidateDbResult("Groups", "GroupName", "varchar", 64));
                checkData.Add(new ValidateDbResult("GroupPerms", "GroupKey", "int", -1));
                checkData.Add(new ValidateDbResult("GroupPerms", "ControllerSN", "int", -1));
                checkData.Add(new ValidateDbResult("GroupPerms", "DoorIndex", "int", -1));
                checkData.Add(new ValidateDbResult("Users", "UserKey", "int", -1));
                checkData.Add(new ValidateDbResult("Users", "UserName", "varchar", 64));
                checkData.Add(new ValidateDbResult("Users", "CardID", "int", -1));
                checkData.Add(new ValidateDbResult("Users", "CardActive", "bit", -1));
                checkData.Add(new ValidateDbResult("GroupMembership", "GroupMembership", "int", -1));
                checkData.Add(new ValidateDbResult("GroupMembership", "UserKey", "int", -1));
                checkData.Add(new ValidateDbResult("GroupMembership", "GroupKey", "int", -1));

                foreach (ValidateDbResult r in checkData)
                {
                    if (!validateResults.Contains(r))
                    {
                        ErrorMsg = "Validate Failed: " + checkData.ToString();
                        return false;
                    }
                }
            }
            catch (Exception e)
            {
                ErrorMsg = $"CreateDB Failed. Reason: {e.Message}";
                return false;
            }

            return true;
        }


        //API Keys
        public class TokenResponse
        {
            public String userName;
            public String userSID;
            public String tokenId;
            public String apiToken;

            public TokenResponse(String _userName, String _userSID, String _tokenID, String _apiToken)
            {
                userName = _userName;
                userSID = _userSID;
                tokenId = _tokenID;
                apiToken = _apiToken;
            }
        }

        public static TokenResponse CreateAPIKey(String UserName, String UserSID, String TokenID, out String ErrorMsg)
        {
            ErrorMsg = null;
            //Create New API Key (64 Hex Characters / 32 Bytes);
            RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider();

            Byte[] tokenBytes = new Byte[32];
            rng.GetBytes(tokenBytes);

            String tokenBytesString = BitConverter.ToString(tokenBytes).Replace("-", "");

            try
            {
                SqlConnection connection = new SqlConnection($"Data Source=(LocalDB)\\.;AttachDBFileName={DB_FILE};Initial Catalog={DB_NAME};Integrated Security=True;");
                connection.Open();
                SqlCommand cmd;

                cmd = new SqlCommand(@"SELECT COUNT(*) FROM dbo.ApiKeys WHERE TokenID = @TokenID", connection);
                cmd.Parameters.Add(new SqlParameter("TokenID", TokenID));

                Int32 count = (Int32)cmd.ExecuteScalar();
                if (count > 0)
                {
                    ErrorMsg = $"CreateAPIKey Failed. TokenID already Exists";
                    return null;
                }
                cmd = new SqlCommand(@"INSERT INTO dbo.ApiKeys ( UserName, UserSID, TokenID, ApiToken )
                                              VALUES ( @UserName, @UserSID, @TokenID, @tokenBytesString )", connection);
                cmd.Parameters.Add(new SqlParameter("UserName", UserName));
                cmd.Parameters.Add(new SqlParameter("UserSID", UserSID));
                cmd.Parameters.Add(new SqlParameter("TokenID", TokenID));
                cmd.Parameters.Add(new SqlParameter("tokenBytesString", tokenBytesString));

                cmd.ExecuteNonQuery();

                connection.Close();
            }
            catch (Exception e)
            {
                ErrorMsg = $"CreateAPIKey Failed. Reason: {e.Message}";
                return null;
            }

            return new TokenResponse(UserName, UserSID, TokenID, tokenBytesString);
        }

        public static TokenResponse DeleteAPIKey(String apiToken, out String ErrorMsg)
        {
            ErrorMsg = null;
            try
            {
                SqlConnection connection = new SqlConnection($"Data Source=(LocalDB)\\.;AttachDBFileName={DB_FILE};Initial Catalog={DB_NAME};Integrated Security=True;");
                connection.Open();
                SqlCommand cmd;

                //See If Token Exists
                cmd = new SqlCommand(@"SELECT UserName, UserSID, TokenID, ApiToken FROM dbo.ApiKeys WHERE ApiToken = @apiToken", connection);
                cmd.Parameters.Add(new SqlParameter("apiToken", apiToken));

                SqlDataReader r = cmd.ExecuteReader();

                if (!r.Read())
                {
                    ErrorMsg = "Delete API Key Failed. Reason: TokenID Does not Exist";
                    return null;
                }
                //Prep Response
                TokenResponse resp = new TokenResponse(r.GetString(0), r.GetString(1), r.GetString(2), r.GetString(3));

                r.Close();

                //Delete Token
                cmd = new SqlCommand(@"DELETE FROM dbo.ApiKeys WHERE ApiToken = @apiToken", connection);
                cmd.Parameters.Add(new SqlParameter("apiToken", apiToken));


                cmd.ExecuteNonQuery();

                connection.Close();

                return resp;


            }
            catch (Exception e)
            {
                ErrorMsg = $"DeleteAPIKey Failed. Reason: {e.Message}";
                return null;
            }


        }


        public static List<TokenResponse> ListAPIKeys(out String ErrorMsg)
        {
            ErrorMsg = null;
            try
            {
                SqlConnection connection = new SqlConnection($"Data Source=(LocalDB)\\.;AttachDBFileName={DB_FILE};Initial Catalog={DB_NAME};Integrated Security=True;");
                connection.Open();
                SqlCommand cmd = connection.CreateCommand();

                List<TokenResponse> apiKeyList = new List<TokenResponse>();

                //Get All Tokens
                cmd.CommandText = $"SELECT UserName, UserSID, TokenID, ApiToken FROM dbo.ApiKeys WHERE TokenID <> 'UserToken' ";
                SqlDataReader r = cmd.ExecuteReader();

                while (r.Read())
                    apiKeyList.Add(new TokenResponse(r.GetString(0), r.GetString(1), r.GetString(2), r.GetString(3)));

                r.Close();

                connection.Close();

                return apiKeyList;


            }
            catch (Exception e)
            {
                ErrorMsg = $"ListAPIKey Failed. Reason: {e.Message}";
                return null;
            }

        }


        public static TokenResponse LookupApiToken(String apiToken, out String ErrorMsg)
        {
            ErrorMsg = null;
            try
            {
                SqlConnection connection = new SqlConnection($"Data Source=(LocalDB)\\.;AttachDBFileName={DB_FILE};Initial Catalog={DB_NAME};Integrated Security=True;");
                connection.Open();
                SqlCommand cmd;

                //Get ApiToken Tokens
                cmd = new SqlCommand(@"SELECT UserName, UserSID, TokenID, ApiToken FROM dbo.ApiKeys WHERE ApiToken = @apiToken", connection);
                cmd.Parameters.Add(new SqlParameter("apiToken", apiToken));

                SqlDataReader r = cmd.ExecuteReader();

                TokenResponse resp = null;
                while (r.Read())
                    resp = new TokenResponse(r.GetString(0), r.GetString(1), r.GetString(2), r.GetString(3));

                if (resp == null)
                {
                    ErrorMsg = "Api Key Not Found";
                    return null;
                }

                r.Close();
                connection.Close();

                return resp;
            }
            catch (Exception e)
            {
                ErrorMsg = $"LookupApiToken Failed. Reason: {e.Message}";
                return null;
            }
        }




        public static TokenResponse GetApiToken(String UserName, String SID, out String ErrorMsg)
        {
            ErrorMsg = null;
            try
            {
                SqlConnection connection = new SqlConnection($"Data Source=(LocalDB)\\.;AttachDBFileName={DB_FILE};Initial Catalog={DB_NAME};Integrated Security=True;");
                connection.Open();
                SqlCommand cmd;



                //Get Existing Token If Available
                cmd = new SqlCommand(@"SELECT UserName, UserSID, TokenID, ApiToken FROM dbo.ApiKeys WHERE TokenID='UserToken' AND UserSID=@SID", connection);
                cmd.Parameters.Add(new SqlParameter("SID", SID));

                SqlDataReader rdr = cmd.ExecuteReader();



                if (rdr.Read())
                {
                    TokenResponse resp = new TokenResponse(rdr.GetString(0), rdr.GetString(1), rdr.GetString(2), rdr.GetString(3));
                    rdr.Close();
                    return resp;
                }

                rdr.Close();

                //Generate New Token if Not AvailableByte[] tokenBytes = new Byte[32];
                RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider();

                Byte[] tokenBytes = new Byte[32];
                rng.GetBytes(tokenBytes);
                String tokenBytesString = BitConverter.ToString(tokenBytes).Replace("-", "");

                cmd = new SqlCommand(@"INSERT INTO dbo.ApiKeys ( UserName, UserSID, TokenID, ApiToken )
                                            VALUES ( @UserName, @SID, 'UserToken', @tokenBytesString )", connection);

                cmd.Parameters.Add(new SqlParameter("UserName", UserName));
                cmd.Parameters.Add(new SqlParameter("SID", SID));
                cmd.Parameters.Add(new SqlParameter("tokenBytesString", tokenBytesString));


                cmd.ExecuteNonQuery();
                connection.Close();

                return new TokenResponse(UserName, SID, "UserToken", tokenBytesString);
            }
            catch (Exception e)
            {
                ErrorMsg = $"GetApiToken Failed. Reason: {e.Message}";
                return null;
            }

        }



        #region Controller Functions
        public static bool AddController(String controllerName, UInt16 controllerSerial, String door1Name, String door2Name, String door3Name, String door4Name, out String ErrorMsg)
        {

            ErrorMsg = null;
            try
            {
                SqlConnection connection = new SqlConnection($"Data Source=(LocalDB)\\.;AttachDBFileName={DB_FILE};Initial Catalog={DB_NAME};Integrated Security=True;");
                connection.Open();
                SqlCommand cmd;


                //Check if Controller already exists
                cmd = new SqlCommand(@"SELECT COUNT(*) FROM dbo.Controllers WHERE ControllerSN = @controllerSerial", connection);
                cmd.Parameters.Add(new SqlParameter("controllerSerial", (Int32)controllerSerial));

                Int32 controllerCount = (Int32)cmd.ExecuteScalar();

                if (controllerCount > 0)
                {
                    ErrorMsg = "Controller Already Added";
                    return false;
                }

                cmd = new SqlCommand(@"INSERT INTO dbo.Controllers ( ControllerName, ControllerSN, Door1Name, Door2Name, Door3Name, Door4Name ) 
                                        VALUES ( @controllerName,@controllerSerial, @door1Name,@door2Name,@door3Name,@door4Name)", connection);

                cmd.Parameters.Add(new SqlParameter("controllerName", controllerName));
                cmd.Parameters.Add(new SqlParameter("controllerSerial", (Int32)controllerSerial));
                cmd.Parameters.Add(new SqlParameter("door1Name", door1Name));
                cmd.Parameters.Add(new SqlParameter("door2Name", door2Name));
                cmd.Parameters.Add(new SqlParameter("door3Name", door3Name));
                cmd.Parameters.Add(new SqlParameter("door4Name", door4Name));

                cmd.ExecuteNonQuery();

                connection.Close();

                return true;
            }
            catch (Exception e)
            {
                ErrorMsg = $"AddController Failed. Reason: {e.Message}";
                return false;
            }

        }

        public static bool DeleteController(UInt16 controllerSerial, out String ErrorMsg)
        {

            ErrorMsg = null;
            try
            {
                SqlConnection connection = new SqlConnection($"Data Source=(LocalDB)\\.;AttachDBFileName={DB_FILE};Initial Catalog={DB_NAME};Integrated Security=True;");
                connection.Open();
                SqlCommand cmd;


                //Check if Controller already exists
                cmd = new SqlCommand(@"SELECT COUNT(*) FROM dbo.Controllers WHERE ControllerSN = @controllerSerial", connection);
                cmd.Parameters.Add(new SqlParameter("controllerSerial", (Int32)controllerSerial));

                Int32 controllerCount = (Int32)cmd.ExecuteScalar();

                if (controllerCount == 0)
                {
                    ErrorMsg = "Controller Doesn't Exist";
                    return false;
                }
                cmd = new SqlCommand(@"DELETE FROM dbo.Controllers WHERE ControllerSN = @controllerSerial", connection);
                cmd.Parameters.Add(new SqlParameter("controllerSerial", (Int32)controllerSerial));

                cmd.ExecuteNonQuery();

                connection.Close();

                return true;
            }
            catch (Exception e)
            {
                ErrorMsg = $"Delete Controller Failed. Reason: {e.Message}";
                return false;
            }

        }

        public static bool EditController(String controllerName, UInt16 controllerSerial, String door1Name, String door2Name, String door3Name, String door4Name, out String ErrorMsg)
        {

            ErrorMsg = null;
            try
            {
                SqlConnection connection = new SqlConnection($"Data Source=(LocalDB)\\.;AttachDBFileName={DB_FILE};Initial Catalog={DB_NAME};Integrated Security=True;");
                connection.Open();
                SqlCommand cmd;


                //Check if Controller already exists
                cmd = new SqlCommand(@"SELECT COUNT(*) FROM dbo.Controllers WHERE ControllerSN = @controllerSerial", connection);

                cmd.Parameters.Add(new SqlParameter("controllerSerial", (Int32)controllerSerial));

                if ((Int32)cmd.ExecuteScalar() <= 0)
                {
                    ErrorMsg = "Controller Doesn't Exist";
                    return false;
                }

                cmd = new SqlCommand(@"UPDATE dbo.Controllers SET   ControllerName = @controllerName,
                                                                    Door1Name = @door1Name,
                                                                    Door2Name = @door2Name, 
                                                                    Door3Name = @door3Name, 
                                                                    Door4Name = @door4Name 
                                    WHERE ControllerSN = @controllerSerial", connection);

                cmd.Parameters.Add(new SqlParameter("controllerName", controllerName));
                cmd.Parameters.Add(new SqlParameter("door1Name", door1Name));
                cmd.Parameters.Add(new SqlParameter("door2Name", door2Name));
                cmd.Parameters.Add(new SqlParameter("door3Name", door3Name));
                cmd.Parameters.Add(new SqlParameter("door4Name", door4Name));
                cmd.Parameters.Add(new SqlParameter("controllerSerial", (Int32)controllerSerial));

                cmd.ExecuteNonQuery();

                connection.Close();

                return true;
            }
            catch (Exception e)
            {
                ErrorMsg = $"Edit Controller Failed. Reason: {e.Message}";
                return false;
            }

        }


        public class ControllerInfo
        {
            public String controllerName;
            public UInt16 controllerSerial;
            public String door1Name;
            public String door2Name;
            public String door3Name;
            public String door4Name;

            public ControllerInfo()
            {
                return;
            }
            public ControllerInfo(String controllerName, UInt16 controllerSerial, String door1Name, String door2Name, String door3Name, String door4Name)
            {
                this.controllerName = controllerName;
                this.controllerSerial = controllerSerial;
                this.door1Name = door1Name;
                this.door2Name = door2Name;
                this.door3Name = door3Name;
                this.door4Name = door4Name;
            }
        }

        public static List<ControllerInfo> ListControllers(out String ErrorMsg)
        {
            ErrorMsg = null;
            List<ControllerInfo> result = new List<ControllerInfo>();

            try
            {
                SqlConnection connection = new SqlConnection($"Data Source=(LocalDB)\\.;AttachDBFileName={DB_FILE};Initial Catalog={DB_NAME};Integrated Security=True;");
                connection.Open();
                SqlCommand cmd = connection.CreateCommand();


                //Get All Controllers
                cmd.CommandText = $"SELECT ControllerName, ControllerSN, Door1Name, Door2Name, Door3Name, Door4Name FROM dbo.Controllers";
                SqlDataReader rdr = cmd.ExecuteReader();

                while (rdr.Read())
                {
                    ControllerInfo ci = new ControllerInfo();
                    ci.controllerName = rdr.GetString(0);
                    ci.controllerSerial = (UInt16)rdr.GetInt32(1);
                    ci.door1Name = rdr.GetString(2);
                    ci.door2Name = rdr.GetString(3);
                    ci.door3Name = rdr.GetString(4);
                    ci.door4Name = rdr.GetString(5);

                    result.Add(ci);
                }

                connection.Close();

                return result;
            }
            catch (Exception e)
            {
                ErrorMsg = $"ListControllers Failed. Reason: {e.Message}";
                return null;
            }
        }

        #endregion

        /// <summary>
        /// Door Permission Description
        /// </summary>
        public class GroupPermDesc
        {
            public Int32 controllerSerial;
            public Int32 doorIndex;
            public String doorName;

            public GroupPermDesc(Int32 controllerSerial, Int32 doorIndex, String c)
            {
                this.controllerSerial = controllerSerial;
                this.doorIndex = doorIndex;
                this.doorName = doorName;
            }

        }

        /// <summary>
        /// Door Summary Holding Class
        /// </summary>
        public class GroupPermEntry
        {
            public Int32 groupKey;
            public Int32 controllerSerial;
            public Int32 doorIndex;
            
            public GroupPermEntry(Int32 groupKey, Int32 controllerSerial, Int32 doorIndex)
            {
                this.groupKey = groupKey;
                this.controllerSerial = controllerSerial;
                this.doorIndex = doorIndex;
            }

        }

        /// <summary>
        /// Group Summary Holding Class
        /// </summary>
        public class GroupSummary
        {
            public Int32 groupKey;
            public String groupName;
            public List<GroupPermEntry> allowedDoors;

            public GroupSummary(Int32 groupKey, String groupName)
            {
                this.groupKey = groupKey;
                this.groupName = groupName;
                this.allowedDoors = new List<GroupPermEntry>();
            }
        }


        public static List<GroupSummary> ListGroups(out String ErrorMsg)
        {
            ErrorMsg = null;
            try
            {
                SqlConnection connection = new SqlConnection($"Data Source=(LocalDB)\\.;AttachDBFileName={DB_FILE};Initial Catalog={DB_NAME};Integrated Security=True;");
                connection.Open();
                SqlCommand cmd;

                //First Get all Groups
                List<GroupSummary> groupList = new List<GroupSummary>();
                cmd = new SqlCommand(@"SELECT GroupKey, GroupName FROM dbo.Groups", connection);
                
                SqlDataReader rdr = cmd.ExecuteReader();

                while (rdr.Read())
                    groupList.Add(new GroupSummary(rdr.GetInt32(0), rdr.GetString(1)));
                rdr.Close();

                foreach(GroupSummary gs in groupList)
                {
                    cmd = new SqlCommand(@"SELECT GroupKey, ControllerSN, DoorIndex FROM dbo.GroupPerms", connection);
                    rdr = cmd.ExecuteReader();
                    while (rdr.Read())
                        gs.allowedDoors.Add(new GroupPermEntry(rdr.GetInt32(0), rdr.GetInt32(1), rdr.GetInt32(2)));
                    rdr.Close();
                }

                connection.Close();

                return groupList;
            }
            catch (Exception e)
            {
                ErrorMsg = $"LookupApiToken Failed. Reason: {e.Message}";
                return null;
            }

        }
    }
}
