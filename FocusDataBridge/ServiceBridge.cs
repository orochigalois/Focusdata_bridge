using System;
using System.Data;
using System.Diagnostics;

using System.ServiceProcess;
using System.Threading.Tasks;
using System.Threading;
using System.Configuration;




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
        public const int Task5s_CYCLE = 5;
        private CancellationTokenSource cts5s = new CancellationTokenSource();
        private Task task5s = null;

    
        public const int Task2s_CYCLE = 2;
        private CancellationTokenSource cts2s = new CancellationTokenSource();
        private Task task2s = null;



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

            task2s = new Task(Task2s, cts2s.Token, TaskCreationOptions.LongRunning);
            task2s.Start();


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


        public Relation Compare(DataRow a, DataRow b)
        {
            if(a["USERID"].ToString().Equals("6")&& a["APPOINTMENTDATE"].ToString().Equals("2017-05-07"))
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

        private void Task5s()
        {
            CancellationToken cancellation = cts5s.Token;
            //TimeSpan interval = TimeSpan.Zero;
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
                    log.Write("Task5s alive\n");

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
                 
                            foreach (DataRow rowMYSQL in dtDoctors_MYSQL.Rows)
                            {
                                if(userID.Equals(rowMYSQL["userID"].ToString()))
                                {
                                    found = true;
                                    if(!fullName.Equals(rowMYSQL["fullName"].ToString()))
                                    {
                                        //update
                                        mysqlConnect.UpdateDoctor(fullName, userID,clinicID, CLINIC_USER_MAIL);
                                    }
                                }
                            }

                            if(!found)
                            {
                                //insert
                                string doctorID = mysqlConnect.InsertDoctor(fullName, userID, CLINIC_USER_MAIL);
                                mysqlConnect.Insert_fd_rel_clinic_doctor(doctorID, clinicID, CLINIC_USER_MAIL);
                                log.Write("Sync a doctor ID:" + doctorID);
                            }

                  
                        }


                        //2. SQL.Sessions -> MYSQL.fd_rel_doctor_appointment_time

                        DataTable dtSessions = bpsqlConnect.GetAllSessions();
                        if (dtSessions == null)
                            continue;
                        
                        DataTable dtAppointments_BP = bpsqlConnect.GetDTAppointments(dtSessions);
                        if (dtAppointments_BP == null)
                            continue;
         
                        DataTable dtAppointments_MYSQL = mysqlConnect.GetAllAppointments(clinicID);

                        int a = 0, b = 0;
                        int length_A = dtAppointments_BP.Rows.Count;
                        int length_B = dtAppointments_MYSQL.Rows.Count;

                        string insertAppQuery = "INSERT INTO fd_rel_doctor_appointment_time (DOCTOR_ID, APPOINTMENT_DATE, APPOINTMENT_TIME, ACTIVE_STATUS,CREATE_USER,CREATE_DATE) VALUES";
                        bool need_to_insert = false;
                        while (a<length_A && b<length_B)
                        {
                            DataRow bp = dtAppointments_BP.Rows[a];
                            DataRow mysql = dtAppointments_MYSQL.Rows[b];
                            Relation result = Compare(bp, mysql);
                            if (result == Relation.GreaterThan)
                                b++;
                            else if(result == Relation.EqualTo)
                            {
                                //update
                                if (!bp["ACTIVE"].ToString().Equals(mysql["ACTIVE_STATUS"].ToString()))
                                    mysqlConnect.UpdateAppointment(mysql, clinicID, CLINIC_USER_MAIL);
                                a++;
                                b++;
                            }
                            else if(result == Relation.LessThan)
                            {
                                string DOCTOR_ID = mysqlConnect.GetDoctorID(bp["USERID"].ToString(), clinicID);
                                insertAppQuery += "(" + DOCTOR_ID +
                                    ",'" + bp["APPOINTMENTDATE"].ToString() + "', '"
                                    + bp["APPOINTMENTTIME"].ToString() + "', '" + bp["ACTIVE"].ToString()
                                    + "','" + CLINIC_USER_MAIL + "','" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "'),";
                                need_to_insert = true;
                                a++;
                            }
                        }

                        while(a < length_A)
                        {
                            DataRow bp = dtAppointments_BP.Rows[a];
                            string DOCTOR_ID = mysqlConnect.GetDoctorID(bp["USERID"].ToString(), clinicID);
                            insertAppQuery += "(" + DOCTOR_ID +
                                ",'" + bp["APPOINTMENTDATE"].ToString() + "', '"
                                + bp["APPOINTMENTTIME"].ToString() + "', '" + bp["ACTIVE"].ToString()
                                + "','" + CLINIC_USER_MAIL + "','" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "'),";
                            need_to_insert = true;
                            a++;
                        }

                        if (need_to_insert)
                        {
                            insertAppQuery = insertAppQuery.TrimEnd(',');
                            mysqlConnect.InsertAppointment(insertAppQuery);
                            log.Write("Sync app complete\n");
                        }


                        ////////////////////////////////
                        //string insertAppQuery = "INSERT INTO fd_rel_doctor_appointment_time (DOCTOR_ID, APPOINTMENT_DATE, APPOINTMENT_TIME, ACTIVE_STATUS,CREATE_USER,CREATE_DATE) VALUES";
                        //bool need_to_insert = false;
                        //foreach (DataRow rowBP in dtAppointments_BP.Rows)
                        //{


                        //    string USER_ID = rowBP["USERID"].ToString();
                        //    string APPOINTMENT_DATE = rowBP["APPOINTMENTDATE"].ToString();
                        //    string APPOINTMENT_TIME = rowBP["APPOINTMENTTIME"].ToString();
                        //    string ACTIVE_STATUS = rowBP["ACTIVE"].ToString();


                        //    bool found = false;

                        //    foreach (DataRow rowMYSQL in dtAppointments_MYSQL.Rows)
                        //    {


                        //        if (USER_ID.Equals(rowMYSQL["USER_ID"].ToString())
                        //            && APPOINTMENT_DATE.Equals(rowMYSQL["APPOINTMENT_DATE"].ToString())
                        //            && APPOINTMENT_TIME.Equals(rowMYSQL["APPOINTMENT_TIME"].ToString())
                        //            )
                        //        {
                        //            found = true;


                        //            if (!ACTIVE_STATUS.Equals(rowMYSQL["ACTIVE_STATUS"].ToString()))
                        //            {
                        //                //update
                        //                mysqlConnect.UpdateAppointment(rowMYSQL, clinicID, CLINIC_USER_MAIL);
                        //            }
                        //            continue;

                        //        }
                        //    }

                        //    if (!found)
                        //    {
                        //        string DOCTOR_ID = mysqlConnect.GetDoctorID(USER_ID, clinicID);
                        //        insertAppQuery += "(" + DOCTOR_ID +
                        //            ",'" + APPOINTMENT_DATE + "', '"
                        //            + APPOINTMENT_TIME + "', '" + ACTIVE_STATUS
                        //            + "','" + CLINIC_USER_MAIL + "','" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "'),";
                        //        need_to_insert = true;


                        //    }

                        //}

                        //if (need_to_insert)
                        //{
                        //    insertAppQuery = insertAppQuery.TrimEnd(',');
                        //    mysqlConnect.InsertAppointment(insertAppQuery);
                        //    log.Write("Sync app complete\n");
                        //}










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


        private void Task2s()//mysql->BP
        {
            CancellationToken cancellation = cts2s.Token;
            TimeSpan interval = TimeSpan.FromSeconds(Task2s_CYCLE);

            while (!cancellation.WaitHandle.WaitOne(interval))
            {
                try
                {

                    if (log.getConnectionState() == Constant.CLOSED)
                    {
                        Console.WriteLine("logWriter is disconnected");
                        log.OpenConnection();
                    }
                    log.Write("Task2s alive\n");

                    clinicID = GetClinicID();
                    if (clinicID == null)
                        continue;
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
                    interval = TimeSpan.FromSeconds(Task2s_CYCLE);
                }
                catch (Exception ex)
                {

                    log.Write("Task2s():Sync mysql->BP failed.\n" + ex.Message);
                    //Add to System Log
                    eventLog1.WriteEntry("Bridge encountered an error '" +
                    ex.Message + "'", EventLogEntryType.Error);

                    interval = TimeSpan.FromSeconds(Task2s_CYCLE);
                }
            }
        }


        protected override void OnStop()
        {
            cts2s.Cancel();
            task2s.Wait();

            cts5s.Cancel();
            task5s.Wait();
        }

    }
}