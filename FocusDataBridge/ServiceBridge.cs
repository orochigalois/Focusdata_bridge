using System;
using System.Data;
using System.Diagnostics;

using System.ServiceProcess;
using System.Threading.Tasks;
using System.Threading;
using System.Configuration;

using System.Globalization;
using System.Collections.Generic;

namespace FocusDataBridge
{


    public enum Relation { GreaterThan, LessThan, EqualTo };

    public partial class ServiceBridge : ServiceBase
    {

        
        /*Log*/
        LogWriter log;

        /*DB instance*/
        MysqlConnect mysqlConnect;
        BPsqlConnect bpsqlConnect;

        /*Tasks*/
#if DEBUG
        public const int Task30s_CYCLE = 5;
#else
        public const int Task30s_CYCLE = 30;
#endif
        private CancellationTokenSource cts30s = new CancellationTokenSource();
        private Task task30s = null;

    
        public const int Task3s_CYCLE = 3;
        private CancellationTokenSource cts3s = new CancellationTokenSource();
        private Task task3s = null;



  
        /*Clinic ID*/

        private List<string> arr_clinicID = new List<string>();
        

        string[] arr_CLINIC_USER_MAIL = new string[] { };



        public ServiceBridge()
        {
            InitializeComponent();         
        }

   

        public void OnDebug()
        {
            OnStart(null);
        }


        protected override void OnStart(string[] args)
        {
            log = new LogWriter();
            log.OpenConnection();

            mysqlConnect = new MysqlConnect(log);

            bpsqlConnect = new BPsqlConnect(log);

            task30s = new Task(Task30s, cts30s.Token, TaskCreationOptions.LongRunning);
            task30s.Start();

            task3s = new Task(Task3s, cts3s.Token, TaskCreationOptions.LongRunning);
            task3s.Start();


        }

 

        public List<string> GetClinicIDs()
        {
           
            List<string> result = new List<string>();
            try
            {
                ConfigurationManager.RefreshSection("appSettings");
                arr_CLINIC_USER_MAIL = ConfigurationManager.AppSettings["CLINIC_USER_MAIL"].ToString().Split(',');

                foreach (string CLINIC_USER_MAIL in arr_CLINIC_USER_MAIL)
                {
                    //CHECK IF CLINIC_USER_EMAIL IS IN fd_clinic_user
                    if (mysqlConnect.CLINIC_USER_MAIL_ExistInTable(CLINIC_USER_MAIL))
                    {
                        result.Add(mysqlConnect.GetClinicKey(CLINIC_USER_MAIL));
                        
                    }
                    else
                    {
                        //error
                        log.Write("GetClinicIDs():Clinic user " + CLINIC_USER_MAIL + " does not exist. Please register at http://www.drpages.com.au/ \n");
                        return null;
                    }
                }

                return result;

            }
            catch (Exception e)
            {
                log.Write("GetClinicID():GetClinicID failed.\n" + e.Message);
                return null;
            }
        }


        public Relation Compare(DataRow a, DataRow b)
        {
            if(a["USERID"].ToString().Equals("6")&& a["APPOINTMENTDATE"].ToString().Equals("2017-05-09")
                && a["APPOINTMENTTIME"].ToString().Equals("22:00:00"))
            {
                int test = 0;
                test++;
            }
            if (String.Compare(a["USERID"].ToString(),b["USER_ID"].ToString())>0)
                return Relation.GreaterThan;
            else if (String.Compare(a["USERID"].ToString(), b["USER_ID"].ToString()) < 0)
                return Relation.LessThan;
            else
            {
                if (String.Compare(a["APPOINTMENTDATE"].ToString(), b["APPOINTMENT_DATE"].ToString()) > 0)
                    return Relation.GreaterThan;
                else if (String.Compare(a["APPOINTMENTDATE"].ToString(), b["APPOINTMENT_DATE"].ToString()) < 0)
                    return Relation.LessThan;
                else
                {
                    if (String.Compare(a["APPOINTMENTTIME"].ToString(), b["APPOINTMENT_TIME"].ToString()) > 0)
                        return Relation.GreaterThan;
                    else if (String.Compare(a["APPOINTMENTTIME"].ToString(), b["APPOINTMENT_TIME"].ToString()) < 0)
                        return Relation.LessThan;
                    else
                        return Relation.EqualTo;
                }
            }
        }


        private string GetDoctorID(string user_id, DataTable dict)
        {
            foreach (DataRow row in dict.Rows)
            {
                if(row["DOCTOR_ID_IMPORT"].ToString().Equals(user_id))
                {
                    return row["DOCTOR_ID"].ToString();
                }
            }
            return null;
        }
        private void Task30s()
        {

            CultureInfo en = new CultureInfo("zh-CN");
            Thread.CurrentThread.CurrentCulture = en;
            CancellationToken cancellation = cts30s.Token;
            //TimeSpan interval = TimeSpan.Zero;
            TimeSpan interval = TimeSpan.FromSeconds(Task30s_CYCLE);

            while (!cancellation.WaitHandle.WaitOne(interval))
            {
                try
                {
  
                    if (log.getConnectionState() == Constant.CLOSED)
                    {
                        Console.WriteLine("logWriter is disconnected");
                        log.OpenConnection();
                    }
                    log.Write("Synchronise from clinic database\n");

                    arr_clinicID = GetClinicIDs();
                    if (arr_clinicID == null)
                        continue;


                    //___________________________________________________________________________________________________BP->mysql
                    try
                    {

                        //1. SQL.Users -> MYSQL.fd_doctor
                        DataTable dtDoctors_BP = bpsqlConnect.BP_GetAllUsers();
                        if (dtDoctors_BP == null)
                            continue;

                        DataTable dtDoctors_MYSQL = mysqlConnect.GetAllDoctors(arr_clinicID);




                        foreach (DataRow rowBP in dtDoctors_BP.Rows)
                        {
                            if (cancellation.IsCancellationRequested)
                            {
                                return;
                            }
                            string surName = rowBP["surName"].ToString();
                            string firstName = rowBP["firstName"].ToString();
                            string fullName = firstName + ' ' + surName;
                            string userID = rowBP["userID"].ToString();

                            string address1 = "", address2 = "", postcode = "";


                            DataTable dtLocations_BP = bpsqlConnect.BP_GetPracticeLocation(rowBP["LocationID"].ToString());
                            if (dtLocations_BP == null)
                                continue;
                            foreach (DataRow rowLocation in dtLocations_BP.Rows)
                            {
                                address1 = rowLocation["address1"].ToString();
                                address2 = rowLocation["address2"].ToString();
                                postcode = rowLocation["postcode"].ToString();
                            }

                            string clinicID = mysqlConnect.Get_ClinicID_By_Location(address1, address2, postcode);





                            bool found = false;
                 
                            foreach (DataRow rowMYSQL in dtDoctors_MYSQL.Rows)
                            {
                                if(userID.Equals(rowMYSQL["userID"].ToString()))
                                {
                                    found = true;
                                    if(!fullName.Equals(rowMYSQL["fullName"].ToString()))
                                    {
                                        //update
                                        mysqlConnect.UpdateDoctor(fullName, userID, arr_clinicID, arr_CLINIC_USER_MAIL);
                                    }
                                }
                            }

                            if(!found)
                            {
                                //insert
                                string doctorID = mysqlConnect.InsertDoctor(fullName, userID, arr_CLINIC_USER_MAIL);
                                mysqlConnect.Insert_fd_rel_clinic_doctor(doctorID, clinicID, arr_CLINIC_USER_MAIL);
                                log.Write("Sync a doctor ID:" + doctorID);
                            }

                  
                        }

                        if (cancellation.IsCancellationRequested)
                        {
                            return;
                        }
                        //2. SQL.Sessions -> MYSQL.fd_rel_doctor_appointment_time

                        string CLINIC_USER_MAIL = string.Join(",", arr_CLINIC_USER_MAIL);

                        DataTable dtSessions = bpsqlConnect.GetAllSessions();
                        if (dtSessions == null)
                            continue;
                        
                        DataTable dtAppointments_BP = bpsqlConnect.GetDTAppointments(dtSessions);
                        if (dtAppointments_BP == null)
                            continue;
         
                        DataTable dtAppointments_MYSQL = mysqlConnect.GetAllAppointments(arr_clinicID);

                        DataTable dtDoctorDict_MYSQL = mysqlConnect.GetDoctorDict(arr_clinicID);
                       

                        if (cancellation.IsCancellationRequested)
                        {
                            return;
                        }


                        int a = 0, b = 0;
                        int length_A = dtAppointments_BP.Rows.Count;
                        int length_B = dtAppointments_MYSQL.Rows.Count;

                        int insertCount = 0;
                        bool someRemain=false;
                        string insertAppQuery = "INSERT INTO fd_rel_doctor_appointment_time (DOCTOR_ID, APPOINTMENT_DATE, APPOINTMENT_TIME, ACTIVE_STATUS,CREATE_USER,CREATE_DATE) VALUES";
                       
                        while (a<length_A && b<length_B)
                        {
                            if (cancellation.IsCancellationRequested)
                            {
                                return;
                            }
                            DataRow bp = dtAppointments_BP.Rows[a];
                            DataRow mysql = dtAppointments_MYSQL.Rows[b];
                            Relation result = Compare(bp, mysql);
                            if (result == Relation.GreaterThan)
                                b++;
                            else if(result == Relation.EqualTo)
                            {
                                //update
                                if (!bp["ACTIVE"].ToString().Equals(mysql["ACTIVE_STATUS"].ToString()))
                                    mysqlConnect.UpdateAppointment(mysql,bp, arr_CLINIC_USER_MAIL);
                                a++;
                                b++;
                            }
                            else if(result == Relation.LessThan)
                            {
                                string DOCTOR_ID = GetDoctorID(bp["USERID"].ToString(), dtDoctorDict_MYSQL);
                                if (DOCTOR_ID == null)
                                    continue;

                                
                                insertAppQuery += "(" + DOCTOR_ID +
                                    ",'" + bp["APPOINTMENTDATE"].ToString() + "', '"
                                    + bp["APPOINTMENTTIME"].ToString() + "', '" + bp["ACTIVE"].ToString()
                                    + "','" + CLINIC_USER_MAIL + "','" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "'),";
                                insertCount++;
                                //log.Write("Prepare " + insertCount.ToString() + " appointments OK\n");

                                if (insertCount==1000)
                                {
                                    insertAppQuery = insertAppQuery.TrimEnd(',');
                                    mysqlConnect.InsertAppointment(insertAppQuery);
                                    log.Write("Sync 1000 appointments complete\n");

                                    insertAppQuery = "INSERT INTO fd_rel_doctor_appointment_time (DOCTOR_ID, APPOINTMENT_DATE, APPOINTMENT_TIME, ACTIVE_STATUS,CREATE_USER,CREATE_DATE) VALUES";
                                    insertCount = 0;
                                    someRemain = false;
                                }
                                else
                                {
                                    someRemain = true;
                                }


                              
                                a++;
                            }
                        }

                        while(a < length_A)
                        {

                            if (cancellation.IsCancellationRequested)
                            {
                                return;
                            }

                            DataRow bp = dtAppointments_BP.Rows[a];
                            string DOCTOR_ID = GetDoctorID(bp["USERID"].ToString(), dtDoctorDict_MYSQL);
                            if (DOCTOR_ID == null)
                                continue;
                            insertAppQuery += "(" + DOCTOR_ID +
                                ",'" + bp["APPOINTMENTDATE"].ToString() + "', '"
                                + bp["APPOINTMENTTIME"].ToString() + "', '" + bp["ACTIVE"].ToString()
                                + "','" + CLINIC_USER_MAIL + "','" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "'),";
                           

                            insertCount++;
                            //log.Write("Prepare "+ insertCount.ToString() + " appointments OK\n");

                            if (insertCount == 1000)
                            {
                                insertAppQuery = insertAppQuery.TrimEnd(',');
                                mysqlConnect.InsertAppointment(insertAppQuery);
                                log.Write("Sync 1000 appointments complete\n");

                                insertAppQuery = "INSERT INTO fd_rel_doctor_appointment_time (DOCTOR_ID, APPOINTMENT_DATE, APPOINTMENT_TIME, ACTIVE_STATUS,CREATE_USER,CREATE_DATE) VALUES";
                                insertCount = 0;
                                someRemain = false;
                            }
                            else
                            {
                                someRemain = true;
                            }
                            

                            a++;


                        }

                        if (someRemain)
                        {
                            insertAppQuery = insertAppQuery.TrimEnd(',');
                            mysqlConnect.InsertAppointment(insertAppQuery);
                            log.Write("Sync " + insertCount.ToString() + "appointments complete\n");
                        }

                        

                    }
                    catch (Exception e)
                    {
                        log.Write("Sync BP->mysql failed.\n" + e.Message);
                    }



                    if (cancellation.IsCancellationRequested)
                    {
                        break;
                    }
                    //interval = WaitAfterSuccessInterval;
                    interval = TimeSpan.FromSeconds(Task30s_CYCLE);
                }
                catch (Exception ex)
                {

                    log.Write("Task30s():Reconnect to DB failed\n" + ex.Message);
                    //Add to System Log
                    eventLog1.WriteEntry("Bridge encountered an error '" +
                    ex.Message + "'", EventLogEntryType.Error);

                    interval = TimeSpan.FromSeconds(Task30s_CYCLE);
                }
            }
        }


        private void Task3s()//mysql->BP
        {


            CultureInfo en = new CultureInfo("zh-CN");
            Thread.CurrentThread.CurrentCulture = en;
            CancellationToken cancellation = cts3s.Token;
            TimeSpan interval = TimeSpan.FromSeconds(Task3s_CYCLE);

            while (!cancellation.WaitHandle.WaitOne(interval))
            {
                try
                {

                    if (log.getConnectionState() == Constant.CLOSED)
                    {
                        Console.WriteLine("logWriter is disconnected");
                        log.OpenConnection();
                    }
                    log.Write("Synchronise from DrPages\n");

                    arr_clinicID = GetClinicIDs();
                    if (arr_clinicID == null)
                        continue;



                    //________________________________________________________________mysql->BP
                    try
                    {

                        //cancel appointment logic
                        DataTable dtCancel = mysqlConnect.GetCancel(arr_clinicID);
                       /* if (dtCancel == null)
                            continue;

                        if (dtCancel.Rows.Count == 0)
                            continue;
                            */
                        foreach (DataRow row in dtCancel.Rows)
                        {
                            bpsqlConnect.CancelAppointment(row["BP_APPOINTMENT_ID"].ToString());
                        }




                        DataTable dtRequests = mysqlConnect.GetAppointmentRequests(arr_clinicID);
                        if (dtRequests == null)
                            continue;

                        if (dtRequests.Rows.Count == 0)
                            continue;

                        //1.reset trigger
                        mysqlConnect.ResetAllRequestFlag(arr_clinicID, arr_CLINIC_USER_MAIL);

                        foreach (DataRow row in dtRequests.Rows)
                        {

                            //1.5 check if the patient exists
                            int patientID;
                            patientID = bpsqlConnect.getPatientID(row["CUSTOMER_LASTNAME"].ToString(), row["CUSTOMER_FIRSTNAME"].ToString(), row["CUSTOMER_BIRTHDAY"].ToString());
                            if (patientID == Constant.INVALID_ID)
                            {
                                //2.add patient
                                patientID = bpsqlConnect.AddPatient(row);
                            }

                            //3.[BP_IsAppointmentBooked]
                            int IsAppointmentBooked = bpsqlConnect.IsAppointmentBooked(row);

                            if (IsAppointmentBooked == 1)
                            {
                                mysqlConnect.SetSuccessfulTo2(row["DOCTOR_APPOINTMENT_TIME_ID"].ToString(), arr_CLINIC_USER_MAIL);
                            }
                            else if (IsAppointmentBooked == 0)
                            {
                                //4.[BP_AddAppointment]
                                int appID = bpsqlConnect.AddAppointment(row, patientID);
                                if (appID!=-1)
                                {
                                    log.Write("Add an appointment successfully");
                                    mysqlConnect.SetSuccessfulTo1(appID,row["DOCTOR_APPOINTMENT_TIME_ID"].ToString(), arr_CLINIC_USER_MAIL);
                                }
                            }

                        }



                        




                    }
                    catch (Exception e)
                    {
                        log.Write("Sync mysql->BP failed.\n" + e.Message);
                    }

                    if (cancellation.IsCancellationRequested)
                    {
                        break;
                    }
                    interval = TimeSpan.FromSeconds(Task3s_CYCLE);
                }
                catch (Exception ex)
                {

                    log.Write("Task3s():Sync mysql->BP failed.\n" + ex.Message);
                    //Add to System Log
                    eventLog1.WriteEntry("Bridge encountered an error '" +
                    ex.Message + "'", EventLogEntryType.Error);

                    interval = TimeSpan.FromSeconds(Task3s_CYCLE);
                }
            }
        }


        protected override void OnStop()
        {
            cts3s.Cancel();
            task3s.Wait();

            cts30s.Cancel();
            task30s.Wait();
        }

    }
}