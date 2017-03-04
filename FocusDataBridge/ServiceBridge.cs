using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using System.Threading;
using System.Configuration;


namespace FocusDataBridge
{
    public partial class ServiceBridge : ServiceBase
    {
        public const int Task10s_CYCLE = 10;
        public const int Task2s_CYCLE = 2;
        private CancellationTokenSource cts10s = new CancellationTokenSource();
        private CancellationTokenSource cts2s = new CancellationTokenSource();
        private Task task10s = null;
        private Task task2s = null;

        private DataTable dtSessions = new DataTable();
        private DataTable dtAppointments = new DataTable();


        public ServiceBridge()
        {
            InitializeComponent();
            InitializeDataTable();
        }

        public void InitializeDataTable()
        {
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

            dtAppointments.Clear();
            dtAppointments.Columns.Add("DOCTOR_ID");
            dtAppointments.Columns.Add("APPOINTMENT_DATE");
            dtAppointments.Columns.Add("APPOINTMENT_TIME");
            dtAppointments.Columns.Add("ACTIVE_STATUS");

        }

        public void OnDebug()
        {
            OnStart(null);
        }

        protected override void OnStart(string[] args)
        {

            task10s = new Task(Task10s, cts10s.Token, TaskCreationOptions.LongRunning);
            task10s.Start();

            task2s = new Task(Task2s, cts2s.Token, TaskCreationOptions.LongRunning);
            task2s.Start();


        }


        private void Task2s()
        {
            CancellationToken cancellation = cts2s.Token;

            TimeSpan interval = TimeSpan.FromSeconds(Task2s_CYCLE);

            while (!cancellation.WaitHandle.WaitOne(interval))
            {
                try
                {

                    MysqlConnect mysqlConnect;
                    SqlDataReader rdr = null;
                    SqlCommand cmd = null;

                    mysqlConnect = new MysqlConnect();

                    string DATABASE_HOST = ConfigurationManager.AppSettings["DATABASE_HOST"];
                    string DATABASE_NAME = ConfigurationManager.AppSettings["DATABASE_NAME"];
                    string DATABASE_USER = ConfigurationManager.AppSettings["DATABASE_USER"];
                    string DATABASE_PASS = ConfigurationManager.AppSettings["DATABASE_PASS"];

                    SqlConnection con = new SqlConnection("Data Source=DBServer;Password=" + DATABASE_PASS + ";Persist Security Info=True;User ID=" + DATABASE_USER + ";Initial Catalog=" + DATABASE_NAME + ";Data Source=" + DATABASE_HOST);
                    try
                    {
                        con.Open();

                        DataTable myRequests = mysqlConnect.GetAppointmentRequests();

                        if (myRequests.Rows.Count != 0)
                        {
                            //1.reset trigger
                            mysqlConnect.ResetAllRequestFlag();

                            foreach (DataRow row in myRequests.Rows)
                            {

                                //2.add patient
                                cmd = new SqlCommand("BP_AddPatient", con);
                                cmd.CommandType = CommandType.StoredProcedure;

                                SqlParameter p1 = new SqlParameter("@TITLECODE", 1+Convert.ToInt32(row["TITLE_ID"].ToString()));
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
                                SqlParameter p7 = new SqlParameter("@ADDRESS2", row["CUSTOMER_ADDR"].ToString());
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

                                SqlParameter p13 = new SqlParameter("@DOB", DateTime.Now);
                                p13.Direction = ParameterDirection.Input;
                                p13.DbType = DbType.DateTime;
                                cmd.Parameters.Add(p13);

                                SqlParameter p14 = new SqlParameter("@SEXCODE", 1+Convert.ToInt32(row["GENDER_ID"].ToString()));
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

                                SqlParameter p21 = new SqlParameter("@PENSIONCODE",1);
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

                                SqlParameter p33 = new SqlParameter("@EMERGENCYCONTACT", 0);
                                p33.Direction = ParameterDirection.Input;
                                p33.DbType = DbType.Int32;
                                cmd.Parameters.Add(p33);


                                var returnParameter = cmd.Parameters.Add("@PATIENTID", SqlDbType.Int);
                                returnParameter.Direction = ParameterDirection.ReturnValue;


                                cmd.ExecuteNonQuery();
                                int Clinic_PATIENTID = (int)returnParameter.Value;
                                Console.WriteLine(Clinic_PATIENTID);

                                //3.[BP_IsAppointmentBooked]

                                cmd = new SqlCommand("BP_IsAppointmentBooked", con);
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
                                int IsAppointmentBooked = (int)returnM.Value;
                                if (IsAppointmentBooked == 1)
                                {
                                    mysqlConnect.SetSuccessfulTo2(row["DOCTOR_APPOINTMENT_TIME_ID"].ToString());
                                }
                                else
                                {
                                    //4.[BP_AddAppointment]
                                    cmd = new SqlCommand("BP_AddAppointment", con);
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

                                    SqlParameter q5 = new SqlParameter("@patientid", Clinic_PATIENTID);
                                    q5.Direction = ParameterDirection.Input;
                                    q5.DbType = DbType.Int32;
                                    cmd.Parameters.Add(q5);


                                    var returnQ = cmd.Parameters.Add("@appointmentID", SqlDbType.Int);
                                    returnQ.Direction = ParameterDirection.ReturnValue;


                                    cmd.ExecuteNonQuery();

                                    mysqlConnect.SetSuccessfulTo1(row["DOCTOR_APPOINTMENT_TIME_ID"].ToString());
                                }
                                    


                                



                            }


                        }



                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.Message);
                        LogWriter.LogWrite("Local database connecting failed.\n" + e.Message);
                    }





                    if (cancellation.IsCancellationRequested)
                    {
                        break;
                    }
                    interval = TimeSpan.FromSeconds(Task2s_CYCLE);
                }
                catch (Exception ex)
                {
                    // Log the exception.

                    eventLog1.WriteEntry("Bridge encountered an error '" +
                    ex.Message + "'", EventLogEntryType.Error);

                    interval = TimeSpan.FromSeconds(Task2s_CYCLE);
                }
            }
        }




        private void Task10s()
        {
            CancellationToken cancellation = cts10s.Token;

            TimeSpan interval = TimeSpan.FromSeconds(Task10s_CYCLE);


            while (!cancellation.WaitHandle.WaitOne(interval))
            {
                try
                {

                    MysqlConnect mysqlConnect;
                    SqlDataReader rdr = null;
                    SqlCommand cmd = null;

                    mysqlConnect = new MysqlConnect();

                    string DATABASE_HOST = ConfigurationManager.AppSettings["DATABASE_HOST"];
                    string DATABASE_NAME = ConfigurationManager.AppSettings["DATABASE_NAME"];
                    string DATABASE_USER = ConfigurationManager.AppSettings["DATABASE_USER"];
                    string DATABASE_PASS = ConfigurationManager.AppSettings["DATABASE_PASS"];

                    SqlConnection con = new SqlConnection("Data Source=DBServer;Password=" + DATABASE_PASS + ";Persist Security Info=True;User ID=" + DATABASE_USER + ";Initial Catalog=" + DATABASE_NAME + ";Data Source=" + DATABASE_HOST);
                    try
                    {
                        con.Open();
                        LogWriter.LogWrite("Connect to local database successfully");


                        //1. SQL.Users -> MYSQL.fd_doctor

                        string surName = "", firstName = "";
                        int userID = 0;
                        cmd = new SqlCommand(
                        "BP_GetAllUsers", con);
                        cmd.CommandType = CommandType.StoredProcedure;

                        try
                        {
                            rdr = cmd.ExecuteReader();
                            while (rdr.Read())
                            {
                                surName = (string)rdr["SURNAME"];
                                surName = surName.Trim();
                                firstName = (string)rdr["FIRSTNAME"];
                                firstName = firstName.Trim();
                                userID = (int)rdr["UserID"];

                                //CHECK IF USERID IS IN FD_DOCTOR
                                if(mysqlConnect.IDExistInTable(userID))
                                {
                                    //CHECK if need to update
                                    if (!(firstName + " " + surName).ToString().Equals(mysqlConnect.GetDoctorName(userID)))
                                        mysqlConnect.UpdateDoctor(surName, firstName, userID);

                                }
                                else
                                    mysqlConnect.InsertDoctor(surName, firstName, userID);
                            }
                            rdr.Close();

                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e.Message);
                            LogWriter.LogWrite("Read local table failed.\n" + e.Message);
                        }

                        //2. SQL.Sessions -> MYSQL.fd_rel_doctor_appointment_time

                        //2.1 Fill dtSessions
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

                        
                        cmd = new SqlCommand(
                        "BP_GetAllSessions", con);
                        cmd.CommandType = CommandType.StoredProcedure;

                        dtSessions.Clear();
                        
                        try
                        {
                            rdr = cmd.ExecuteReader();
                            while (rdr.Read())
                            {
                                clinic1_sessions_UserID = (int)rdr["UserID"];
                                clinic1_sessions_LocationID = (int)rdr["LocationID"];
                                clinic1_sessions_DayOfWeek = (int)rdr["DayOfWeek"];
                                clinic1_sessions_StartTime = (int)rdr["StartTime"];
                                clinic1_sessions_EndTime = (int)rdr["EndTime"];
                                clinic1_sessions_Length = (int)rdr["Length"];

                                clinic1_sessions_StartDate = (System.DateTime)rdr["StartDate"];
                                clinic1_sessions_EndDate = (System.DateTime)rdr["EndDate"];
                                clinic1_sessions_Weeks = (int)rdr["Weeks"];
                                clinic1_sessions_CycleWeek = (int)rdr["CycleWeek"];
                                clinic1_sessions_CycleDate = (System.DateTime)rdr["CycleDate"];



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

                            }
                            rdr.Close();
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e.Message);
                            LogWriter.LogWrite("Read local table failed.\n" + e.Message);
                        }

                        //2.2 Calculate & Fill dtAppointments
                        dtAppointments.Clear();

                        foreach (DataRow row in dtSessions.Rows)
                        {
                            
                            List<DateTime> myDates = getDays(Int32.Parse(row["DayOfWeek"].ToString()),
                                DateTime.Parse(row["StartDate"].ToString()),
                                DateTime.Parse(row["EndDate"].ToString()));
                            List<string> myTimes = getTimes(
                                Int32.Parse(row["StartTime"].ToString()),
                                Int32.Parse(row["EndTime"].ToString()),
                                Int32.Parse(row["Length"].ToString())
                                );
                            foreach (var date in myDates)
                            {

                                foreach(var time in myTimes)
                                {
                                    DataRow _r = dtAppointments.NewRow();
                                    _r["DOCTOR_ID"] = row["UserID"];
                                    _r["APPOINTMENT_DATE"] = date.Date;
                                    _r["APPOINTMENT_TIME"] = time;

                                    cmd = new SqlCommand("BP_IsAppointmentBooked", con);
                                    cmd.CommandType = CommandType.StoredProcedure;
                                    SqlParameter p1 = new SqlParameter("@userid", Int32.Parse(row["UserID"].ToString()));
                                    p1.Direction = ParameterDirection.Input;
                                    p1.DbType = DbType.Int32;
                                    cmd.Parameters.Add(p1);

                                    SqlParameter p2 = new SqlParameter("@aptdate", date.Date);
                                    p2.Direction = ParameterDirection.Input;
                                    p2.DbType = DbType.DateTime;
                                    cmd.Parameters.Add(p2);


                                    TimeSpan MySpan = TimeSpan.Parse(time);


                                    SqlParameter p3 = new SqlParameter("@apttime", Convert.ToInt32(MySpan.TotalSeconds));
                                    p3.Direction = ParameterDirection.Input;
                                    p3.DbType = DbType.Int32;
                                    cmd.Parameters.Add(p3);


                                    var returnParameter = cmd.Parameters.Add("@ReturnVal", SqlDbType.Int);
                                    returnParameter.Direction = ParameterDirection.ReturnValue;

                         
                                    cmd.ExecuteNonQuery();
                                    int result = (int) returnParameter.Value;
                                    if(result==1)
                                        _r["ACTIVE_STATUS"] = 0;
                                    else
                                        _r["ACTIVE_STATUS"] = 1;

                                    dtAppointments.Rows.Add(_r);

                                }
                                
                           
                            }

                        }
                        //2.3 dtAppointments -> fd_rel_doctor_appointment_time
                        foreach (DataRow dataRow in dtAppointments.Rows)
                        {

                            //CHECK IF APP IS IN fd_rel_doctor_appointment_time
                            if (mysqlConnect.AppExistInTable(dataRow))
                            {
                                //CHECK if need to update
                                if (!(dataRow["ACTIVE_STATUS"]).ToString().Equals(mysqlConnect.GetAppActive(dataRow)))
                                    mysqlConnect.UpdateAppointment(dataRow);

                            }
                            else
                                mysqlConnect.InsertAppointment(dataRow);

                        }




                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.Message);
                        LogWriter.LogWrite("Local database connecting failed.\n" + e.Message);
                    }


                    


                    if (cancellation.IsCancellationRequested)
                    {
                        break;
                    }
                    interval = TimeSpan.FromSeconds(Task10s_CYCLE);
                }
                catch (Exception ex)
                {
                    // Log the exception.

                    eventLog1.WriteEntry("Bridge encountered an error '" +
                    ex.Message + "'", EventLogEntryType.Error);

                    interval = TimeSpan.FromSeconds(Task10s_CYCLE);
                }
            }
        }

        

        protected override void OnStop()
        {
            //System.IO.File.Create(AppDomain.CurrentDomain.BaseDirectory + "OnStopAlex.txt");

            cts10s.Cancel();
            task10s.Wait();

            cts2s.Cancel();
            task2s.Wait();
        }



        public static List<string> getTimes(int start,int end,int length)
        {
            List<string> results = new List<string>();
            for(int i=start;i<end;i+=length)
            {
                TimeSpan time = TimeSpan.FromSeconds(i);

                //here backslash is must to tell that colon is
                //not the part of format, it just a character that we want in output
                string str = time.ToString(@"hh\:mm\:ss");
                results.Add(str);
            }
            return results;
        }

        public static List<DateTime> getDays(int d, DateTime start, DateTime end)
        {
            DayOfWeek dayOfWeek = DayOfWeek.Sunday;

            switch (d)
            {
                case 1:
                    dayOfWeek = DayOfWeek.Sunday;
                    break;
                case 2:
                    dayOfWeek = DayOfWeek.Monday;
                    break;
                case 3:
                    dayOfWeek = DayOfWeek.Tuesday;
                    break;
                case 4:
                    dayOfWeek = DayOfWeek.Wednesday;
                    break;
                case 5:
                    dayOfWeek = DayOfWeek.Thursday;
                    break;
                case 6:
                    dayOfWeek = DayOfWeek.Friday;
                    break;
                case 7:
                    dayOfWeek = DayOfWeek.Saturday;
                    break;
                default:
                    break;

            }
            List<DateTime> results = new List<DateTime>();
            int intMonth = DateTime.Now.Month;
            int intYear = DateTime.Now.Year;
            int intDay = DateTime.Now.Day;

            for (int i = 0; i < 30; i++)
            {
                if (d != 0)
                {
                    if (DateTime.Now.AddDays(i).DayOfWeek == dayOfWeek)
                    {
                        if(DateTime.Now.AddDays(i)>= start&& DateTime.Now.AddDays(i)<=end)
                            results.Add(DateTime.Now.AddDays(i));
                    }

                }
                else
                {
                    if (DateTime.Now.AddDays(i).DayOfWeek != DayOfWeek.Saturday &&
                        DateTime.Now.AddDays(i).DayOfWeek != DayOfWeek.Sunday)
                    {
                        if (DateTime.Now.AddDays(i) >= start && DateTime.Now.AddDays(i) <= end)
                            results.Add(DateTime.Now.AddDays(i));
                    }
                }

            }
            return results;
        }



    }
}
