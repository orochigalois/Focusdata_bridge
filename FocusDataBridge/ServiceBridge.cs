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
        public const int CYCLE = 10;
        private CancellationTokenSource cts = new CancellationTokenSource();
        private Task mainTask = null;

        private DataTable dtSessions = new DataTable();
        private DataTable dtAppointments = new DataTable();
        private DataTable dtAppointments_mysql = new DataTable();

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

            dtAppointments_mysql.Clear();
            dtAppointments_mysql.Columns.Add("DOCTOR_ID");
            dtAppointments_mysql.Columns.Add("APPOINTMENT_DATE");
            dtAppointments_mysql.Columns.Add("APPOINTMENT_TIME");
            dtAppointments_mysql.Columns.Add("ACTIVE_STATUS");
        }

        public void OnDebug()
        {
            OnStart(null);
        }

        protected override void OnStart(string[] args)
        {

            mainTask = new Task(Poll, cts.Token, TaskCreationOptions.LongRunning);
            mainTask.Start();


        }

        private void Poll()
        {
            CancellationToken cancellation = cts.Token;

            //polling every 10s
            TimeSpan interval = TimeSpan.FromSeconds(CYCLE);


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
                    interval = TimeSpan.FromSeconds(CYCLE);
                }
                catch (Exception ex)
                {
                    // Log the exception.

                    eventLog1.WriteEntry("Bridge encountered an error '" +
                    ex.Message + "'", EventLogEntryType.Error);

                    interval = TimeSpan.FromSeconds(CYCLE);
                }
            }
        }

        

        protected override void OnStop()
        {
            //System.IO.File.Create(AppDomain.CurrentDomain.BaseDirectory + "OnStopAlex.txt");

            cts.Cancel();
            mainTask.Wait();
        }



        //public Boolean IsRowInMysql(DataRow dr)
        //{
        //    Boolean result = false;
        //    foreach (DataRow dataRow in dtAppointments_mysql.Rows)
        //    {
        //        if (dr["DOCTOR_ID"].Equals(dataRow["DOCTOR_ID"])
        //            && dr["APPOINTMENT_DATE"].Equals(dataRow["APPOINTMENT_DATE"])
        //            && dr["APPOINTMENT_TIME"].Equals(dataRow["APPOINTMENT_TIME"])
        //            && dr["ACTIVE_STATUS"].Equals(dataRow["ACTIVE_STATUS"])
        //            )
        //            result = true;
        //    }
        //    return result;
        //}
        //public Boolean IsRowInClinic(DataRow dr)
        //{
        //    Boolean result = false;
        //    foreach (DataRow dataRow in dtAppointments.Rows)
        //    {
        //        if (dr["DOCTOR_ID"].Equals(dataRow["DOCTOR_ID"])
        //            && dr["APPOINTMENT_DATE"].Equals(dataRow["APPOINTMENT_DATE"])
        //            && dr["APPOINTMENT_TIME"].Equals(dataRow["APPOINTMENT_TIME"])
        //            && dr["ACTIVE_STATUS"].Equals(dataRow["ACTIVE_STATUS"])
        //            )
        //            result = true;
        //    }
        //    return result;
        //}

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
