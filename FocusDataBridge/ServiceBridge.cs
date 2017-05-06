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

        /*Log*/
        LogWriter log;

        /*DB instance*/
        MysqlConnect mysqlConnect;
        BPsqlConnect bpsqlConnect;

        /*Tasks*/
        public const int Task5s_CYCLE = 5;
        private CancellationTokenSource cts5s = new CancellationTokenSource();
        private Task task5s = null;


        /*Clinic ID*/
        private string clinicID=null;
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
            log = new LogWriter();
            log.OpenConnection();

            mysqlConnect = new MysqlConnect(log);

            bpsqlConnect = new BPsqlConnect(log);

            task5s = new Task(Task5s, cts5s.Token, TaskCreationOptions.LongRunning);
            task5s.Start();

        }

 

        public string GetClinicID()
        {
            try
            {
                ConfigurationManager.RefreshSection("appSettings");
                CLINIC_USER_MAIL = ConfigurationManager.AppSettings["CLINIC_USER_MAIL"];
                //CHECK IF CLINIC_USER_EMAIL IS IN fd_clinic_user
                if (mysqlConnect.CLINIC_USER_MAIL_ExistInTable(CLINIC_USER_MAIL))
                {
                    return mysqlConnect.GetClinicKey(CLINIC_USER_MAIL);
                }
                else
                {
                    //error
                    log.Write("GetClinicID():Clinic user does not exist. Please register at http://www.drpages.com.au/ \n");
                    return null;
                }
            }
            catch (Exception e)
            {
                log.Write("GetClinicID():GetClinicID failed.\n" + e.Message);
                return null;
            }
        }


        private void Task5s()
        {
            CancellationToken cancellation = cts5s.Token;
            TimeSpan interval = TimeSpan.FromSeconds(Task5s_CYCLE);

            while (!cancellation.WaitHandle.WaitOne(interval))
            {
                try
                {
  
                    if (log.getConnectionState() == Constant.CLOSED)
                    {
                        Console.WriteLine("logWriter is disconnected");
                        log.OpenConnection();
                    }
                    log.Write("Keeping alive\n");

                    clinicID = GetClinicID();
                    if (clinicID == null)
                        continue;


                    //___________________________________________________________________________________________________BP->mysql
                    try
                    {

                        //1. SQL.Users -> MYSQL.fd_doctor
                        DataTable dtDoctors_BP = bpsqlConnect.BP_GetAllUsers();
                        if (dtDoctors_BP == null)
                            continue;

                        DataTable dtDoctors_MYSQL = mysqlConnect.GetAllDoctors(clinicID);




                        foreach (DataRow rowBP in dtDoctors_BP.Rows)
                        {

                            string surName = rowBP["surName"].ToString();
                            string firstName = rowBP["firstName"].ToString();
                            string fullName = firstName + ' ' + surName;
                            string userID = rowBP["userID"].ToString();

                            bool found = false;
                            bool need_update = false;
                            foreach (DataRow rowMYSQL in dtDoctors_MYSQL.Rows)
                            {
                                if(userID.Equals(rowMYSQL["userID"].ToString()))
                                {
                                    found = true;
                                    if(fullName.Equals(rowMYSQL["fullName"].ToString()))
                                    {
                                        need_update = false;
                                    }
                                }
                            }

                            if(!found)
                            {
                                //insert
                            }

                                //int userID = Int32.Parse(row["userID"].ToString());
                                //CHECK IF USERID IS IN FD_DOCTOR
                                if (mysqlConnect.DoctorExist(userID, clinicID))
                            {
                                //CHECK if need to update
                                if (!(firstName + " " + surName).Equals(mysqlConnect.GetDoctorName(userID, clinicID)))
                                    mysqlConnect.UpdateDoctor(surName, firstName, userID, clinicID, CLINIC_USER_MAIL);
                            }
                            else
                            {
                                string doctorID = mysqlConnect.InsertDoctor(surName, firstName, userID, CLINIC_USER_MAIL);
                                mysqlConnect.Insert_fd_rel_clinic_doctor(doctorID, clinicID, CLINIC_USER_MAIL);
                                log.Write("Sync a doctor ID:" + doctorID);
                            }
                        }


                        //2. SQL.Sessions -> MYSQL.fd_rel_doctor_appointment_time

                        //2.1 Fill dtSessions
                        DataTable dtSessions = bpsqlConnect.GetAllSessions();
                        if (dtSessions == null)
                            continue;

                        //2.2 Calculate & Fill dtAppointments
                        DataTable dtAppointments = bpsqlConnect.GetDTAppointments(dtSessions);
                        if (dtAppointments == null)
                            continue;

                        //2.3 dtAppointments -> fd_rel_doctor_appointment_time
                        foreach (DataRow dataRow in dtAppointments.Rows)
                        {

                            //CHECK IF APP IS IN fd_rel_doctor_appointment_time
                            if (mysqlConnect.AppointmentExist(dataRow, clinicID))
                            {
                                //CHECK if need to update
                                if (!(dataRow["ACTIVE_STATUS"]).ToString().Equals(mysqlConnect.GetAppointmentActiveStatus(dataRow, clinicID)))
                                    mysqlConnect.UpdateAppointment(dataRow, clinicID,CLINIC_USER_MAIL);
                            }
                            else
                                mysqlConnect.InsertAppointment(dataRow, clinicID, CLINIC_USER_MAIL);
                        }

                    }
                    catch (Exception e)
                    {
                        log.Write("Sync BP->mysql failed.\n" + e.Message);
                    }

                    //________________________________________________________________mysql->BP
                    try
                    {
                        DataTable dtRequests = mysqlConnect.GetAppointmentRequests(clinicID);
                        if (dtRequests == null)
                            continue;

                        if (dtRequests.Rows.Count == 0)
                            continue;

                        //1.reset trigger
                        mysqlConnect.ResetAllRequestFlag(clinicID, CLINIC_USER_MAIL);

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
                                mysqlConnect.SetSuccessfulTo2(row["DOCTOR_APPOINTMENT_TIME_ID"].ToString(), CLINIC_USER_MAIL);
                            }
                            else if (IsAppointmentBooked == 0)
                            {
                                //4.[BP_AddAppointment]
                                if (bpsqlConnect.AddAppointment(row, patientID))
                                {
                                    log.Write("Add an appointment successfully");
                                    mysqlConnect.SetSuccessfulTo1(row["DOCTOR_APPOINTMENT_TIME_ID"].ToString(), CLINIC_USER_MAIL);
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
                    interval = TimeSpan.FromSeconds(Task5s_CYCLE);
                }
                catch (Exception ex)
                {

                    log.Write("Task5s():Reconnect to DB failed\n" + ex.Message);
                    //Add to System Log
                    eventLog1.WriteEntry("Bridge encountered an error '" +
                    ex.Message + "'", EventLogEntryType.Error);

                    interval = TimeSpan.FromSeconds(Task5s_CYCLE);
                }
            }
        }



        protected override void OnStop()
        {
            //cts5s.Cancel();
            //task5s.Wait();
        }

    }
}