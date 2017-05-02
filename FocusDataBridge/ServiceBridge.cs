using System;
using System.Data;
using System.Diagnostics;

using System.ServiceProcess;
using System.Threading.Tasks;
using System.Threading;
using System.Configuration;




namespace FocusDataBridge
{
    
    public partial class ServiceBridge : ServiceBase
    {

        /*DB instance*/
        MysqlConnect mysqlConnect = new MysqlConnect();
        BPsqlConnect bpsqlConnect = new BPsqlConnect();

        /*Log*/
        LogWriter log = new LogWriter();

        /*Tasks*/
        public const int Task5s_CYCLE = 5;
        public const int Task10s_CYCLE = 10;
        public const int Task2s_CYCLE = 2;

        private CancellationTokenSource cts5s = new CancellationTokenSource();
        private CancellationTokenSource cts10s = new CancellationTokenSource();
        private CancellationTokenSource cts2s = new CancellationTokenSource();

        private Task task5s = null;
        private Task task10s = null;
        private Task task2s = null;

   

        /*Clinic ID*/
        private string clinicID="";
        private string CLINIC_USER_MAIL = "";


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
            PrepareMySql();
            PrepareBPSql();

            clinicID=GetClinicID();

            if (clinicID != null)
            {
                task10s = new Task(Task10s, cts10s.Token, TaskCreationOptions.LongRunning);
                task10s.Start();

                task2s = new Task(Task2s, cts2s.Token, TaskCreationOptions.LongRunning);
                task2s.Start();

                task5s = new Task(Task5s, cts5s.Token, TaskCreationOptions.LongRunning);
                task5s.Start();
            }
            else
            {
                LogWriter.LogWrite("OnStart():Cannot start the tasks.Please make sure DBs are connected and restart service\n");
            }
        }

        public void PrepareMySql()
        {
            try
            {
                mysqlConnect = new MysqlConnect();
                mysqlConnect.OpenConnection();
            }
            catch (Exception e)
            {
                LogWriter.LogWrite("PrepareMySql():Mysql DB connecting failed.\n" + e.Message);
            }
        }
        public void PrepareBPSql()
        {
            try
            {
                bpsqlConnect = new BPsqlConnect();
                bpsqlConnect.OpenConnection();
            }
            catch (Exception e)
            {
                LogWriter.LogWrite("PrepareBPSql():BP DB connecting failed.\n" + e.Message);
            }
        }

        public string GetClinicID()
        {
            try
            { 
                CLINIC_USER_MAIL = ConfigurationManager.AppSettings["CLINIC_USER_MAIL"];
                //CHECK IF CLINIC_USER_EMAIL IS IN fd_clinic_user
                if (mysqlConnect.CLINIC_USER_MAIL_ExistInTable(CLINIC_USER_MAIL))
                {
                    return mysqlConnect.GetClinicKey(CLINIC_USER_MAIL);
                }
                else
                {
                    //error
                    LogWriter.LogWrite("GetClinicID():Clinic user does not exist. Please register at http://www.drpages.com.au/ \n");
                    return null;
                }
            }
            catch (Exception e)
            {
                LogWriter.LogWrite("GetClinicID():GetClinicID failed.\n" + e.Message);
                return null;
            }
        }


        private void Task5s()//Keep DB alive
        {
            CancellationToken cancellation = cts5s.Token;
            TimeSpan interval = TimeSpan.FromSeconds(Task5s_CYCLE);

            while (!cancellation.WaitHandle.WaitOne(interval))
            {
                try
                {
                    try
                    {
                        if(mysqlConnect.getConnectionState() == ConnectionState.Closed)
                        {
                            mysqlConnect.OpenConnection();
                            LogWriter.LogWrite("Reconnect to mysql successfully");
                        }

                    }
                    catch (Exception e)
                    {
                        LogWriter.LogWrite("Task5s():Reconnect to mysql failed.\n" + e.Message);
                    }

                    try
                    {
                        if (bpsqlConnect.getConnectionState() == ConnectionState.Closed)
                        {
                            bpsqlConnect.OpenConnection();
                            LogWriter.LogWrite("Reconnect to BP DB successfully");
                        }

                    }
                    catch (Exception e)
                    {
                        LogWriter.LogWrite("Task5s():Reconnect to BP DB failed.\n" + e.Message);
                    }


                    if (cancellation.IsCancellationRequested)
                    {
                        break;
                    }
                    interval = TimeSpan.FromSeconds(Task5s_CYCLE);
                }
                catch (Exception ex)
                {

                    LogWriter.LogWrite("Task5s():Reconnect to DB failed\n" + ex.Message);
                    //Add to System Log
                    eventLog1.WriteEntry("Bridge encountered an error '" +
                    ex.Message + "'", EventLogEntryType.Error);

                    interval = TimeSpan.FromSeconds(Task5s_CYCLE);
                }
            }
        }






        private void Task2s()//mysql->BP
        {
            CancellationToken cancellation = cts2s.Token;
            TimeSpan interval = TimeSpan.FromSeconds(Task2s_CYCLE);

            while (!cancellation.WaitHandle.WaitOne(interval))
            {
                try
                {
                    try
                    {
                        DataTable myRequests = mysqlConnect.GetAppointmentRequests(clinicID);

                        if (myRequests.Rows.Count != 0)
                        {
                            //1.reset trigger
                            mysqlConnect.ResetAllRequestFlag(clinicID);

                            foreach (DataRow row in myRequests.Rows)
                            {

                                //1.5 check if the patient exists
                                int patientID;
                                patientID=bpsqlConnect.getPatientID(row["CUSTOMER_LASTNAME"].ToString(), row["CUSTOMER_FIRSTNAME"].ToString(), row["CUSTOMER_BIRTHDAY"].ToString());
                                if(patientID==Constant.INVALID_ID)
                                {
                                    //2.add patient
                                    patientID=bpsqlConnect.AddPatient(row);
                                }

                                //3.[BP_IsAppointmentBooked]
                                int IsAppointmentBooked =bpsqlConnect.IsAppointmentBooked(row);

                                if (IsAppointmentBooked == 1)
                                {
                                    mysqlConnect.SetSuccessfulTo2(row["DOCTOR_APPOINTMENT_TIME_ID"].ToString());
                                }
                                else if (IsAppointmentBooked == 0)
                                {
                                    //4.[BP_AddAppointment]
                                    if(bpsqlConnect.AddAppointment(row, patientID))
                                        mysqlConnect.SetSuccessfulTo1(row["DOCTOR_APPOINTMENT_TIME_ID"].ToString());
                                }

                            }

                        }

                    }
                    catch (Exception e)
                    {     
                        LogWriter.LogWrite("Task2s():Sync mysql->BP failed.\n" + e.Message);
                    }

                    if (cancellation.IsCancellationRequested)
                    {
                        break;
                    }
                    interval = TimeSpan.FromSeconds(Task2s_CYCLE);
                }
                catch (Exception ex)
                {
                    
                    LogWriter.LogWrite("Task2s():Sync mysql->BP failed.\n" + ex.Message);
                    //Add to System Log
                    eventLog1.WriteEntry("Bridge encountered an error '" +
                    ex.Message + "'", EventLogEntryType.Error);

                    interval = TimeSpan.FromSeconds(Task2s_CYCLE);
                }
            }
        }




        private void Task10s()//BP->mysql
        {
            CancellationToken cancellation = cts10s.Token;
            TimeSpan interval = TimeSpan.FromSeconds(Task10s_CYCLE);
            while (!cancellation.WaitHandle.WaitOne(interval))
            {
                try
                {
 
                    try
                    {

                        //1. SQL.Users -> MYSQL.fd_doctor
                        DataTable dtDoctors=bpsqlConnect.GetAllUsers();
                        if (dtDoctors == null)
                            return;

                        foreach (DataRow row in dtDoctors.Rows)
                        {

                            string surName = row["surName"].ToString();
                            string firstName = row["firstName"].ToString();
                            int userID = Int32.Parse(row["userID"].ToString());
                            //CHECK IF USERID IS IN FD_DOCTOR
                            if (mysqlConnect.IDExistInTable(userID))
                            {
                                //CHECK if need to update
                                if (!(firstName + " " + surName).Equals(mysqlConnect.GetDoctorName(userID)))
                                    mysqlConnect.UpdateDoctor(surName, firstName, userID);
                            }
                            else
                            {
                                string doctorID = mysqlConnect.InsertDoctor(surName, firstName, userID);
                                mysqlConnect.Insert_fd_rel_clinic_doctor(doctorID, clinicID, CLINIC_USER_MAIL);
                            }
                        }
                            

                        //2. SQL.Sessions -> MYSQL.fd_rel_doctor_appointment_time

                        //2.1 Fill dtSessions
                        DataTable dtSessions=bpsqlConnect.GetAllSessions();
                        if (dtSessions == null)
                            return;

                        //2.2 Calculate & Fill dtAppointments
                        DataTable dtAppointments = bpsqlConnect.GetDTAppointments(dtSessions);
                        if (dtAppointments == null)
                            return;

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
                        LogWriter.LogWrite("Task10s():Sync BP->mysql failed.\n" + e.Message);
                    }



                    if (cancellation.IsCancellationRequested)
                    {
                        break;
                    }
                    interval = TimeSpan.FromSeconds(Task10s_CYCLE);
                }
                catch (Exception ex)
                {
                    LogWriter.LogWrite("Task10s():Sync BP->mysql failed.\n" + ex.Message);
                    //Add to System Log
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

            if (mysqlConnect.getConnectionState() == ConnectionState.Open)
                mysqlConnect.CloseConnection();

            if (bpsqlConnect.getConnectionState() == ConnectionState.Open)
                bpsqlConnect.CloseConnection();
        }


    }
}
