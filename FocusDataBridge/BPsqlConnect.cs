using System;
using System.Collections.Generic;

using System.Data;
using System.Data.SqlClient;
using System.Configuration;
using System.Globalization;

namespace FocusDataBridge
{
    class BPsqlConnect
    {
        LogWriter log;

        public BPsqlConnect(LogWriter log)
        {
            this.log = log;
            
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private string PrepareConnectionString()
        {
            //var configFile = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            //ConfigurationManager.RefreshSection(configFile.AppSettings.SectionInformation.Name);
            ConfigurationManager.RefreshSection("appSettings");

            string DATABASE_HOST = ConfigurationManager.AppSettings["DATABASE_HOST"];
            string DATABASE_NAME = ConfigurationManager.AppSettings["DATABASE_NAME"];
            string DATABASE_USER = ConfigurationManager.AppSettings["DATABASE_USER"];
            string DATABASE_PASS = ConfigurationManager.AppSettings["DATABASE_PASS"];

            string connectionString = "Data Source=DBServer;Password=" + DATABASE_PASS + ";Persist Security Info=True;User ID=" + DATABASE_USER + ";Initial Catalog=" + DATABASE_NAME + ";Data Source=" + DATABASE_HOST;
            return connectionString;
        }



        public DataTable BP_GetPracticeLocation(string id)
        {
            try
            {
                DataTable dtLocations = new DataTable();
                dtLocations.Clear();
                dtLocations.Columns.Add("address1");
                dtLocations.Columns.Add("address2");
                dtLocations.Columns.Add("postcode");

                using (SqlConnection connection = new SqlConnection(PrepareConnectionString()))
                {
                    connection.Open();
                    using (SqlCommand cmd = new SqlCommand("BP_GetPracticeLocation", connection))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        SqlParameter p1 = new SqlParameter("@locationid", Int32.Parse(id));
                        p1.Direction = ParameterDirection.Input;
                        p1.DbType = DbType.Int32;
                        cmd.Parameters.Add(p1);
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            string address1 = "", address2 = "", postcode="";
                           
                            if (reader != null)
                            {
                                while (reader.Read())
                                {

                                    if (!reader.IsDBNull(reader.GetOrdinal("ADDRESS1")))
                                    {
                                        address1 = (string)reader["ADDRESS1"];
                                        address1 = address1.Trim();
                                    }
                                    if (!reader.IsDBNull(reader.GetOrdinal("ADDRESS2")))
                                    {
                                        address2 = (string)reader["ADDRESS2"];
                                        address2 = address2.Trim();
                                    }
                                    if (!reader.IsDBNull(reader.GetOrdinal("POSTCODE")))
                                    {
                                        postcode = (string)reader["POSTCODE"];
                                        postcode = postcode.Trim();
                                    }


                                    DataRow _r = dtLocations.NewRow();
                                    _r["address1"] = address1;
                                    _r["address2"] = address2;
                                    _r["postcode"] = postcode;
                                    dtLocations.Rows.Add(_r);
                                }
                            }
                        }


                    }
                }
                return dtLocations;


            }
            catch (Exception e)
            {
                log.Write("BPSQL:BP_GetPracticeLocation(): failed\n" + e.Message);
                return null;
            }

        }



        /// <summary>
        /// BP_GetAllUsers
        /// </summary>
        /// <returns></returns>
        public DataTable BP_GetAllUsers()
        {
            try
            {
                DataTable dtDoctors = new DataTable();
                dtDoctors.Clear();
                dtDoctors.Columns.Add("surName");
                dtDoctors.Columns.Add("firstName");
                dtDoctors.Columns.Add("userID");

                dtDoctors.Columns.Add("LocationID");

                using (SqlConnection connection = new SqlConnection(PrepareConnectionString()))
                {
                    connection.Open();
                    
                    using (SqlCommand cmd = new SqlCommand("BP_GetAllUsers", connection))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            string surName = "", firstName = "";
                            int userID = 0, LocationID=0;
                            if (reader != null)
                            {
                                while (reader.Read())
                                {
                                    if (!reader.IsDBNull(reader.GetOrdinal("SURNAME")))
                                        surName = (string)reader["SURNAME"];
                                    else
                                        surName = "";
                                    surName = surName.Trim();
                                    firstName = (string)reader["FIRSTNAME"];
                                    firstName = firstName.Trim();
                                    userID = (int)reader["UserID"];
                                    LocationID= (int)reader["LocationID"];

                                    DataRow _r = dtDoctors.NewRow();
                                    _r["surName"] = surName;
                                    _r["firstName"] = firstName;
                                    _r["userID"] = userID;
                                    _r["LocationID"] = LocationID;
                                    dtDoctors.Rows.Add(_r);
                                }
                            }
                        }

                    } 

                } 

                return dtDoctors;
            }
            catch (Exception e)
            {
                log.Write("BPSQL:BP_GetAllUsers(): failed\n" + e.Message);
                return null;
            }

        }


        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public DataTable GetAllSessions()
        {
            try
            {
                DataTable dtSessions = new DataTable();
                dtSessions.Clear();
                dtSessions.Columns.Add("UserID");
                dtSessions.Columns.Add("LocationID");
                dtSessions.Columns.Add("DayOfWeek");
                dtSessions.Columns.Add("StartTime");
                dtSessions.Columns.Add("EndTime");
                dtSessions.Columns.Add("Length");
                dtSessions.Columns.Add("StartDate");
                dtSessions.Columns.Add("EndDate");
                dtSessions.Columns.Add("Weeks");
                dtSessions.Columns.Add("CycleWeek");
                dtSessions.Columns.Add("CycleDate");
 
                int clinic1_sessions_UserID = 0;
                int clinic1_sessions_LocationID = 0;
                int clinic1_sessions_DayOfWeek = 0;
                int clinic1_sessions_StartTime = 0;
                int clinic1_sessions_EndTime = 0;
                int clinic1_sessions_Length = 0;
                System.DateTime clinic1_sessions_StartDate;
                System.DateTime clinic1_sessions_EndDate;
                int clinic1_sessions_Weeks = 0;
                int clinic1_sessions_CycleWeek = 0;
                System.DateTime clinic1_sessions_CycleDate;


                using (SqlConnection connection = new SqlConnection(PrepareConnectionString()))
                {
                    connection.Open();
                    using (SqlCommand cmd = new SqlCommand("BP_GetAllSessions", connection))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader != null)
                            {
                                while (reader.Read())
                                {
                                    clinic1_sessions_UserID = (int)reader["UserID"];
                                    clinic1_sessions_LocationID = (int)reader["LocationID"];
                                    clinic1_sessions_DayOfWeek = (int)reader["DayOfWeek"];
                                    clinic1_sessions_StartTime = (int)reader["StartTime"];
                                    clinic1_sessions_EndTime = (int)reader["EndTime"];
                                    clinic1_sessions_Length = (int)reader["Length"];
                                    if (!reader.IsDBNull(reader.GetOrdinal("StartDate")))
                                        clinic1_sessions_StartDate = (System.DateTime)reader["StartDate"];
                                    else
                                        clinic1_sessions_StartDate = new DateTime(2001, 1, 1);
                                    if (!reader.IsDBNull(reader.GetOrdinal("EndDate")))
                                        clinic1_sessions_EndDate = (System.DateTime)reader["EndDate"];
                                    else
                                        clinic1_sessions_EndDate = new DateTime(2030, 1, 1);
                                    clinic1_sessions_Weeks = (int)reader["Weeks"];
                                    clinic1_sessions_CycleWeek = (int)reader["CycleWeek"];
                                    if (!reader.IsDBNull(reader.GetOrdinal("CycleDate")))
                                        clinic1_sessions_CycleDate = (System.DateTime)reader["CycleDate"];
                                    else
                                        clinic1_sessions_CycleDate = new DateTime(2001, 1, 1);


                                    DataRow _r = dtSessions.NewRow();
                                    _r["UserID"] = clinic1_sessions_UserID.ToString();
                                    _r["LocationID"] = clinic1_sessions_LocationID.ToString();
                                    _r["DayOfWeek"] = clinic1_sessions_DayOfWeek;
                                    _r["StartTime"] = clinic1_sessions_StartTime;
                                    _r["EndTime"] = clinic1_sessions_EndTime;
                                    _r["Length"] = clinic1_sessions_Length;
                                    _r["StartDate"] = clinic1_sessions_StartDate.ToString();
                                    _r["EndDate"] = clinic1_sessions_EndDate.ToString();
                                    _r["Weeks"] = clinic1_sessions_Weeks.ToString();
                                    _r["CycleWeek"] = clinic1_sessions_CycleWeek.ToString();
                                    _r["CycleDate"] = clinic1_sessions_CycleDate.ToString();
                                    dtSessions.Rows.Add(_r);
                                    //log.Write("Sync a session successfully");
                                }
                            }
                        }
                    }
                }

                DataView dv = dtSessions.DefaultView;
                dv.Sort = "UserID asc";
                DataTable sortedDT = dv.ToTable();
                return sortedDT;
            }
            catch (Exception e)
            {
                log.Write("BPSQL:GetAllSessions(): failed\n" + e.Message);
                return null;
            }

        }

        public DataTable GetDTAppointmentBooked()
        {
            try
            {
                DataTable dtAppointmentBooked = new DataTable();
                dtAppointmentBooked.Clear();
                dtAppointmentBooked.Columns.Add("USER_ID");
                dtAppointmentBooked.Columns.Add("APPOINTMENT_DATE");
                dtAppointmentBooked.Columns.Add("APPOINTMENT_TIME");
                string query = "SELECT UserID,AppointmentDate,AppointmentTime FROM Appointments WHERE RecordStatus = 1";
                using (SqlConnection connection = new SqlConnection(PrepareConnectionString()))
                {
                    connection.Open();
                    using (SqlCommand cmd = new SqlCommand(query, connection))
                    {
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader != null)
                            {
                                while (reader.Read())
                                {
                                    DataRow _r = dtAppointmentBooked.NewRow();
                                    _r["USER_ID"] = reader["UserID"].ToString();
                                    var dateTime = DateTime.Parse(reader["AppointmentDate"].ToString());
                                    _r["APPOINTMENT_DATE"] = dateTime.ToString("yyyy-MM-dd");

                                    int i = Convert.ToInt32(reader["AppointmentTime"].ToString());
                                       
                                    TimeSpan time = TimeSpan.FromSeconds(i);

                                    //here backslash is must to tell that colon is
                                    //not the part of format, it just a character that we want in output
                                    string str = time.ToString(@"hh\:mm\:ss");

                                    _r["APPOINTMENT_TIME"] = str;
                                    dtAppointmentBooked.Rows.Add(_r);
                                }
                            }
                        }
                    }
                }

                return dtAppointmentBooked;
            }
            catch (Exception e)
            {
                log.Write("BPSQL:GetDTAppointmentBooked(): failed\n" + e.Message);
                return null;
            }
        }


        public bool IsAppointmentBooked(string user_id,string aptdate, string apttime, DataTable dtAppointmentsBooked)
        {
            bool found = false;
            foreach (DataRow row in dtAppointmentsBooked.Rows)
            {
                if (row["USER_ID"].ToString().Equals(user_id)&&
                    row["APPOINTMENT_DATE"].ToString().Equals(aptdate)&&
                    row["APPOINTMENT_TIME"].ToString().Equals(apttime))
                {
                    found = true;
                    continue;
                }
            }
            return found;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="dtSessions"></param>
        /// <returns></returns>
        public DataTable GetDTAppointments(DataTable dtSessions)
        {
            try
            {
                DataTable dtAppointments = new DataTable();
                dtAppointments.Clear();
                dtAppointments.Columns.Add("USERID");
                dtAppointments.Columns.Add("APPOINTMENTDATE");
                dtAppointments.Columns.Add("APPOINTMENTTIME");
                dtAppointments.Columns.Add("ACTIVE");
  


                DataTable dtAppointmentsBooked = GetDTAppointmentBooked();
                foreach (DataRow row in dtSessions.Rows)
                {

                    List<DateTime> myDates = Constant.getDays(Int32.Parse(row["DayOfWeek"].ToString()),
                        DateTime.Parse(row["StartDate"].ToString()),
                        DateTime.Parse(row["EndDate"].ToString()));
                    List<string> myTimes = Constant.getTimes(
                        Int32.Parse(row["StartTime"].ToString()),
                        Int32.Parse(row["EndTime"].ToString()),
                        Int32.Parse(row["Length"].ToString())
                        );
                    foreach (var date in myDates)
                    {

                        foreach (var time in myTimes)
                        {
                            DataRow _r = dtAppointments.NewRow();
                            _r["USERID"] = row["UserID"];
                            _r["APPOINTMENTDATE"] = date.Date.ToString("yyyy-MM-dd"); ;
                            _r["APPOINTMENTTIME"] = time;
                            if(IsAppointmentBooked(_r["USERID"].ToString(),
                                _r["APPOINTMENTDATE"].ToString(),
                                _r["APPOINTMENTTIME"].ToString(),
                                dtAppointmentsBooked))
                                _r["ACTIVE"] = 0;
                            else
                                _r["ACTIVE"] = 1;
                            dtAppointments.Rows.Add(_r);
                            
                        }
                    }
                }


                DataView dv = dtAppointments.DefaultView;
                dv.Sort = "UserID,APPOINTMENTDATE,APPOINTMENTTIME asc";
                DataTable sortedDT = dv.ToTable();
                return sortedDT;
               
            }
            catch (Exception e)
            {
                log.Write("BPSQL:GetDTAppointments(): failed\n" + e.Message);
                return null;
            }

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="surname"></param>
        /// <param name="firstname"></param>
        /// <param name="dob"></param>
        /// <returns></returns>
        public int getPatientID(string surname, string firstname, string dob)
        {
            try
            {
                bool found = false;
                int patientID = Constant.INVALID_ID;
                using (SqlConnection connection = new SqlConnection(PrepareConnectionString()))
                {
                    connection.Open();
                    using (SqlCommand cmd = new SqlCommand("BP_GetPatientByNameDOB", connection))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        SqlParameter p1 = new SqlParameter("@surname", surname);
                        p1.Direction = ParameterDirection.Input;
                        p1.DbType = DbType.String;
                        cmd.Parameters.Add(p1);

                        SqlParameter p2 = new SqlParameter("@firstname", firstname);
                        p2.Direction = ParameterDirection.Input;
                        p2.DbType = DbType.String;
                        cmd.Parameters.Add(p2);

                        var dateTime = DateTime.ParseExact(dob, "dd/MM/yyyy", new CultureInfo("en-US"));
                        SqlParameter p3 = new SqlParameter("@DOB", dateTime);
                        p3.Direction = ParameterDirection.Input;
                        p3.DbType = DbType.DateTime;
                        cmd.Parameters.Add(p3);
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader != null)
                            {
                                while (reader.Read())
                                {
                                    patientID = (int)reader["InternalID"];
                                    found = true;
                                }
                            }
                        }
                    }
                }

                if (found)
                    return patientID;
                else
                    return Constant.INVALID_ID;

            }
            catch (Exception e)
            {
                log.Write("BPSQL:getPatientID(): getPatientID failed\n" + e.Message);
                return Constant.INVALID_ID;
            }

        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="row"></param>
        /// <returns></returns>
        public int AddPatient(DataRow row)
        {
            try
            {
                int patientID;
                using (SqlConnection connection = new SqlConnection(PrepareConnectionString()))
                {
                    connection.Open();
                    using (SqlCommand cmd = new SqlCommand("BP_AddPatient", connection))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        SqlParameter p1 = new SqlParameter("@TITLECODE", 1 + Convert.ToInt32(row["TITLE_ID"].ToString()));
                        p1.Direction = ParameterDirection.Input;
                        p1.DbType = DbType.Int32;
                        cmd.Parameters.Add(p1);

                        SqlParameter p2 = new SqlParameter("@FIRSTNAME", row["CUSTOMER_FIRSTNAME"].ToString());
                        p2.Direction = ParameterDirection.Input;
                        p2.DbType = DbType.String;
                        cmd.Parameters.Add(p2);

                        SqlParameter p3 = new SqlParameter("@MIDDLENAME", "");
                        p3.Direction = ParameterDirection.Input;
                        p3.DbType = DbType.String;
                        cmd.Parameters.Add(p3);

                        SqlParameter p4 = new SqlParameter("@SURNAME", row["CUSTOMER_LASTNAME"].ToString());
                        p4.Direction = ParameterDirection.Input;
                        p4.DbType = DbType.String;
                        cmd.Parameters.Add(p4);
                        SqlParameter p5 = new SqlParameter("@PREFERREDNAME", row["CUSTOMER_FIRSTNAME"].ToString());
                        p5.Direction = ParameterDirection.Input;
                        p5.DbType = DbType.String;
                        cmd.Parameters.Add(p5);

                        SqlParameter p6 = new SqlParameter("@ADDRESS1", row["CUSTOMER_ADDR"].ToString());
                        p6.Direction = ParameterDirection.Input;
                        p6.DbType = DbType.String;
                        cmd.Parameters.Add(p6);
                        SqlParameter p7 = new SqlParameter("@ADDRESS2", "");
                        p7.Direction = ParameterDirection.Input;
                        p7.DbType = DbType.String;
                        cmd.Parameters.Add(p7);

                        SqlParameter p8 = new SqlParameter("@CITY", row["CUSTOMER_SUBURB"].ToString());
                        p8.Direction = ParameterDirection.Input;
                        p8.DbType = DbType.String;
                        cmd.Parameters.Add(p8);

                        SqlParameter p9 = new SqlParameter("@POSTCODE", row["CUSTOMER_POSTCODE"].ToString());
                        p9.Direction = ParameterDirection.Input;
                        p9.DbType = DbType.String;
                        cmd.Parameters.Add(p9);

                        SqlParameter p10 = new SqlParameter("@POSTALADDRESS", row["CUSTOMER_ADDR"].ToString());
                        p10.Direction = ParameterDirection.Input;
                        p10.DbType = DbType.String;
                        cmd.Parameters.Add(p10);

                        SqlParameter p11 = new SqlParameter("@POSTALCITY", row["CUSTOMER_SUBURB"].ToString());
                        p11.Direction = ParameterDirection.Input;
                        p11.DbType = DbType.String;
                        cmd.Parameters.Add(p11);

                        SqlParameter p12 = new SqlParameter("@POSTALPOSTCODE", row["CUSTOMER_POSTCODE"].ToString());
                        p12.Direction = ParameterDirection.Input;
                        p12.DbType = DbType.String;
                        cmd.Parameters.Add(p12);


                        var dateTime = DateTime.ParseExact(row["CUSTOMER_BIRTHDAY"].ToString(), "dd/MM/yyyy", new CultureInfo("en-US"));
                        SqlParameter p13 = new SqlParameter("@DOB", dateTime);
                        p13.Direction = ParameterDirection.Input;
                        p13.DbType = DbType.DateTime;
                        cmd.Parameters.Add(p13);

                        SqlParameter p14 = new SqlParameter("@SEXCODE", 1 + Convert.ToInt32(row["GENDER_ID"].ToString()));
                        p14.Direction = ParameterDirection.Input;
                        p14.DbType = DbType.Int32;
                        cmd.Parameters.Add(p14);

                        SqlParameter p15 = new SqlParameter("@HOMEPHONE", row["CUSTOMER_PHONE_NO"].ToString());
                        p15.Direction = ParameterDirection.Input;
                        p15.DbType = DbType.String;
                        cmd.Parameters.Add(p15);

                        SqlParameter p16 = new SqlParameter("@WORKPHONE", row["CUSTOMER_PHONE_NO"].ToString());
                        p16.Direction = ParameterDirection.Input;
                        p16.DbType = DbType.String;
                        cmd.Parameters.Add(p16);

                        SqlParameter p17 = new SqlParameter("@MOBILEPHONE", row["CUSTOMER_PHONE_NO"].ToString());
                        p17.Direction = ParameterDirection.Input;
                        p17.DbType = DbType.String;
                        cmd.Parameters.Add(p17);

                        SqlParameter p18 = new SqlParameter("@MEDICARENO", row["MEDICAL_CARD_NO"].ToString());
                        p18.Direction = ParameterDirection.Input;
                        p18.DbType = DbType.String;
                        cmd.Parameters.Add(p18);

                        SqlParameter p19 = new SqlParameter("@MEDICARELINENO", "0");
                        p19.Direction = ParameterDirection.Input;
                        p19.DbType = DbType.String;
                        cmd.Parameters.Add(p19);

                        SqlParameter p20 = new SqlParameter("@MEDICAREEXPIRY", "0");
                        p20.Direction = ParameterDirection.Input;
                        p20.DbType = DbType.String;
                        cmd.Parameters.Add(p20);

                        SqlParameter p21 = new SqlParameter("@PENSIONCODE", 1);
                        p21.Direction = ParameterDirection.Input;
                        p21.DbType = DbType.Int32;
                        cmd.Parameters.Add(p21);

                        SqlParameter p22 = new SqlParameter("@PENSIONNO", "0");
                        p22.Direction = ParameterDirection.Input;
                        p22.DbType = DbType.String;
                        cmd.Parameters.Add(p22);

                        SqlParameter p23 = new SqlParameter("@PENSIONEXPIRY", DateTime.Now);
                        p23.Direction = ParameterDirection.Input;
                        p23.DbType = DbType.DateTime;
                        cmd.Parameters.Add(p23);

                        SqlParameter p24 = new SqlParameter("@DVACODE", 1);
                        p24.Direction = ParameterDirection.Input;
                        p24.DbType = DbType.Int32;
                        cmd.Parameters.Add(p24);

                        SqlParameter p25 = new SqlParameter("@DVANO", "0");
                        p25.Direction = ParameterDirection.Input;
                        p25.DbType = DbType.String;
                        cmd.Parameters.Add(p25);

                        SqlParameter p26 = new SqlParameter("@RECORDNO", "0");
                        p26.Direction = ParameterDirection.Input;
                        p26.DbType = DbType.String;
                        cmd.Parameters.Add(p26);

                        SqlParameter p27 = new SqlParameter("@EXTERNALID", "0");
                        p27.Direction = ParameterDirection.Input;
                        p27.DbType = DbType.String;
                        cmd.Parameters.Add(p27);

                        SqlParameter p28 = new SqlParameter("@EMAIL", row["CUSTOMER_USER_MAIL"].ToString());
                        p28.Direction = ParameterDirection.Input;
                        p28.DbType = DbType.String;
                        cmd.Parameters.Add(p28);


                        SqlParameter p29 = new SqlParameter("@HEADOFFAMILYID", 1);
                        p29.Direction = ParameterDirection.Input;
                        p29.DbType = DbType.Int32;
                        cmd.Parameters.Add(p29);

                        SqlParameter p30 = new SqlParameter("@ETHNICCODE", 1);
                        p30.Direction = ParameterDirection.Input;
                        p30.DbType = DbType.Int32;
                        cmd.Parameters.Add(p30);

                        SqlParameter p31 = new SqlParameter("@CONSENTSMSREMINDER", 1);
                        p31.Direction = ParameterDirection.Input;
                        p31.DbType = DbType.Int32;
                        cmd.Parameters.Add(p31);

                        SqlParameter p32 = new SqlParameter("@NEXTOFKINID", 1);
                        p32.Direction = ParameterDirection.Input;
                        p32.DbType = DbType.Int32;
                        cmd.Parameters.Add(p32);

                        //SqlParameter p33 = new SqlParameter("@EMERGENCYCONTACT", 0);
                        //p33.Direction = ParameterDirection.Input;
                        //p33.DbType = DbType.Int32;
                        //cmd.Parameters.Add(p33);


                        var returnParameter = cmd.Parameters.Add("@PATIENTID", SqlDbType.Int);
                        returnParameter.Direction = ParameterDirection.ReturnValue;


                        cmd.ExecuteNonQuery();
                        patientID = (int)returnParameter.Value;


                    }
                }
                return patientID;


            }
            catch (Exception e)
            {
                log.Write("BPSQL:AddPatient(): AddPatient failed\n" + e.Message);
                return Constant.INVALID_ID;
            }

        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="row"></param>
        /// <returns></returns>
        public int IsAppointmentBooked(DataRow row)
        {
            try
            {
                int IsAppointmentBooked;
                using (SqlConnection connection = new SqlConnection(PrepareConnectionString()))
                {
                    connection.Open();
                    using (SqlCommand cmd = new SqlCommand("BP_IsAppointmentBooked", connection))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        SqlParameter m1 = new SqlParameter("@userid", Int32.Parse(row["DOCTOR_ID_IMPORT"].ToString()));
                        m1.Direction = ParameterDirection.Input;
                        m1.DbType = DbType.Int32;
                        cmd.Parameters.Add(m1);

                        SqlParameter m2 = new SqlParameter("@aptdate", DateTime.Parse(row["APPOINTMENT_DATE"].ToString()));
                        m2.Direction = ParameterDirection.Input;
                        m2.DbType = DbType.DateTime;
                        cmd.Parameters.Add(m2);


                        TimeSpan SpanM = TimeSpan.Parse(row["APPOINTMENT_TIME"].ToString());


                        SqlParameter m3 = new SqlParameter("@apttime", Convert.ToInt32(SpanM.TotalSeconds));
                        m3.Direction = ParameterDirection.Input;
                        m3.DbType = DbType.Int32;
                        cmd.Parameters.Add(m3);


                        var returnM = cmd.Parameters.Add("@ReturnVal", SqlDbType.Int);
                        returnM.Direction = ParameterDirection.ReturnValue;


                        cmd.ExecuteNonQuery();
                        IsAppointmentBooked = (int)returnM.Value;

                    }
                }
                return IsAppointmentBooked;
            }
            catch (Exception e)
            {
                log.Write("BPSQL:IsAppointmentBooked(): failed\n" + e.Message);
                return Constant.INVALID_ID;
            }

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="row"></param>
        /// <param name="patientID"></param>
        /// <returns></returns>
        public int AddAppointment(DataRow row, int patientID)
        {
            int appID = -1;
            try
            {
                using (SqlConnection connection = new SqlConnection(PrepareConnectionString()))
                {
                    connection.Open();
                    using (SqlCommand cmd = new SqlCommand("BP_AddAppointment", connection))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;

                        SqlParameter q1 = new SqlParameter("@aptdate", row["APPOINTMENT_DATE"].ToString());
                        q1.Direction = ParameterDirection.Input;
                        q1.DbType = DbType.String;
                        cmd.Parameters.Add(q1);

                        TimeSpan MySpan = TimeSpan.Parse(row["APPOINTMENT_TIME"].ToString());

                        SqlParameter q2 = new SqlParameter("@apttime", Convert.ToInt32(MySpan.TotalSeconds));
                        q2.Direction = ParameterDirection.Input;
                        q2.DbType = DbType.Int32;
                        cmd.Parameters.Add(q2);

                        SqlParameter q3 = new SqlParameter("@aptlen", 900);
                        q3.Direction = ParameterDirection.Input;
                        q3.DbType = DbType.Int32;
                        cmd.Parameters.Add(q3);

                        SqlParameter q4 = new SqlParameter("@practitionerid", Int32.Parse(row["DOCTOR_ID_IMPORT"].ToString()));
                        q4.Direction = ParameterDirection.Input;
                        q4.DbType = DbType.Int32;
                        cmd.Parameters.Add(q4);

                        SqlParameter q5 = new SqlParameter("@patientid", patientID);
                        q5.Direction = ParameterDirection.Input;
                        q5.DbType = DbType.Int32;
                        cmd.Parameters.Add(q5);


                        var returnQ = cmd.Parameters.Add("@appointmentID", SqlDbType.Int);
                        returnQ.Direction = ParameterDirection.ReturnValue;

                        cmd.ExecuteNonQuery();
                        appID = (int)returnQ.Value;

                        

                    }
                }

                return appID;
            }
            catch (Exception e)
            {
                log.Write("BPSQL:AddAppointment(): failed\n" + e.Message);
                return appID;
            }

        }



        public void CancelAppointment(string appID)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(PrepareConnectionString()))
                {
                    connection.Open();
                    using (SqlCommand cmd = new SqlCommand("BP_CancelAppointment", connection))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;

                        SqlParameter q1 = new SqlParameter("@aptid", Convert.ToInt32(appID));
                        q1.Direction = ParameterDirection.Input;
                        q1.DbType = DbType.Int32;
                        cmd.Parameters.Add(q1);

                        SqlParameter q2 = new SqlParameter("@loginid", 0);
                        q2.Direction = ParameterDirection.Input;
                        q2.DbType = DbType.Int32;
                        cmd.Parameters.Add(q2);

                       
                        cmd.ExecuteNonQuery();

                        log.Write("Cancel an appointment successfully\n");

                    }
                }
            }
            catch (Exception e)
            {
                log.Write("BPSQL:CancelAppointment(): failed\n" + e.Message);
            }

        }




    }
}
