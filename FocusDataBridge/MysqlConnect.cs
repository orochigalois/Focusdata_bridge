using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using MySql.Data.MySqlClient;
using System.Configuration;
using System.Data;

namespace FocusDataBridge
{
    class MysqlConnect
    {
        LogWriter log;

        private MySqlConnection connection;
        private string FOCUSDATA_DATABASE_HOST;
        private string FOCUSDATA_DATABASE_NAME;
        private string FOCUSDATA_DATABASE_USER;
        private string FOCUSDATA_DATABASE_PASS;

        //Constructor
        public MysqlConnect(LogWriter log)
        {
            this.log = log;
            FOCUSDATA_DATABASE_HOST = ConfigurationManager.AppSettings["FOCUSDATA_DATABASE_HOST"];
            FOCUSDATA_DATABASE_NAME = ConfigurationManager.AppSettings["FOCUSDATA_DATABASE_NAME"];
            FOCUSDATA_DATABASE_USER = ConfigurationManager.AppSettings["FOCUSDATA_DATABASE_USER"];
            FOCUSDATA_DATABASE_PASS = ConfigurationManager.AppSettings["FOCUSDATA_DATABASE_PASS"];

            string connectionString;
            connectionString = "SERVER=" + FOCUSDATA_DATABASE_HOST + ";" + "DATABASE=" + FOCUSDATA_DATABASE_NAME + ";" + "UID=" + FOCUSDATA_DATABASE_USER + ";" + "PASSWORD=" + FOCUSDATA_DATABASE_PASS + ";";

            connection = new MySqlConnection(connectionString);
            
        }

        public ConnectionState getConnectionState()
        {
            return connection.State;
        }

        

        //open connection to database
        public bool OpenConnection()
        {
            try
            {
                connection.Open();
                log.Write("Connect to mysql successfully");
                return true;
            }
            catch (MySqlException ex)
            {
                //When handling errors, you can your application's response based on the error number.
                //The two most common error numbers when connecting are as follows:
                //0: Cannot connect to server.
                //1045: Invalid user name and/or password.

                switch (ex.Number)
                {

                    case 1045:
                        log.Write("MYSQL:OpenConnection():Invalid focusdata server username/password, please try again\n" + ex.Message);
                        break;
                    default:
                        log.Write("MYSQL:OpenConnection():Cannot connect to focusdata server.\n" + ex.Message);
                        break;

                }
                return false;
            }
        }

        //Close connection
        public bool CloseConnection()
        {
            try
            {
                connection.Close();
                log.Write("Connection to mysql is closed");
                return true;
            }
            catch (MySqlException ex)
            {
                log.Write("MYSQL:CloseConnection():Cannot close mysql connection.\n" + ex.Message);
                return false;
            }
        }


        //______________________________________________________________________________________________________Main Logic

        public string GetClinicKey(string mail)
        {
            string result = "";
            try
            {
                string query = "SELECT CLINIC_USER_ID FROM fd_clinic_user where CLINIC_USER_MAIL='" + mail + "'";

                //Open connection
                if (connection.State == ConnectionState.Open)
                {
                    //Create Command
                    MySqlCommand cmd = new MySqlCommand(query, connection);
                    //Create a data reader and Execute the command
                    MySqlDataReader dataReader = cmd.ExecuteReader();

                    //Read the data and store them in the list
                    while (dataReader.Read())
                    {
                        result = dataReader["CLINIC_USER_ID"].ToString();
                    }

                    //close Data Reader
                    dataReader.Close();

      
                    //return list to be displayed
                    return result;
                }
                else
                {
                    log.Write("MYSQL:GetClinicKey(): DB connection is not connected or broken");
                    return result;
                }
            }
            catch (Exception e)
            {
                log.Write("MYSQL:GetClinicKey(): failed\n" + e.Message);
                return result;
            }
        }


        public bool CLINIC_USER_MAIL_ExistInTable(String CLINIC_USER_EMAIL)
        {
            int Count = -1;
            try
            { 
                string query = "SELECT Count(*) FROM fd_clinic_user where CLINIC_USER_MAIL='" + CLINIC_USER_EMAIL+"'";
                
                //Open Connection
                if (connection.State == ConnectionState.Open)
                {
                    //Create Mysql Command
                    MySqlCommand cmd = new MySqlCommand(query, connection);

                    //ExecuteScalar will return one value
                    Count = int.Parse(cmd.ExecuteScalar() + "");

                    //close Connection
                    //this.CloseConnection();

                    return Count > 0;
                }
                else
                {
                    log.Write("MYSQL:CLINIC_USER_MAIL_ExistInTable(): DB connection is not connected or broken");
                    return Count > 0;
                }
            }
            catch (Exception e)
            {
                log.Write("MYSQL:CLINIC_USER_MAIL_ExistInTable(): failed\n" + e.Message);
                return Count > 0;
            }
        }




        public bool IDExistInTable(int userID)
        {
            int Count = -1;
            try
            {
                string query = "SELECT Count(*) FROM fd_doctor where DOCTOR_ID_IMPORT=" + userID.ToString();
                
                if (connection.State == ConnectionState.Open)
                {
                    //Create Mysql Command
                    MySqlCommand cmd = new MySqlCommand(query, connection);

                    //ExecuteScalar will return one value
                    Count = int.Parse(cmd.ExecuteScalar() + "");

                    //close Connection
                    //this.CloseConnection();

                    return Count > 0;
                }
                else
                {
                    log.Write("MYSQL:IDExistInTable(): DB connection is not connected or broken");
                    return Count > 0;
                }
            }
            catch (Exception e)
            {
                log.Write("MYSQL:IDExistInTable(): failed\n" + e.Message);
                return Count > 0;
            }
        }
        

        public string GetDoctorName(int userID)
        {
            string result = "";
            try
            {
                string query = "SELECT DOCTOR_NAME FROM fd_doctor where DOCTOR_ID_IMPORT=" + userID.ToString();

                if (connection.State == ConnectionState.Open)
                {
                    //Create Command
                    MySqlCommand cmd = new MySqlCommand(query, connection);
                    //Create a data reader and Execute the command
                    MySqlDataReader dataReader = cmd.ExecuteReader();

                    //Read the data and store them in the list
                    while (dataReader.Read())
                    {
                        result = dataReader["DOCTOR_NAME"].ToString();
                    }

                    //close Data Reader
                    dataReader.Close();

                    //close Connection
                    //this.CloseConnection();

                    //return list to be displayed
                    return result;
                }
                else
                {
                    log.Write("MYSQL:GetDoctorName(): DB connection is not connected or broken");
                    return result;
                }
            }
            catch (Exception e)
            {
                log.Write("MYSQL:GetDoctorName(): failed\n" + e.Message);
                return result;
            }
        }

        public void UpdateDoctor(string surName, string firstName, int userID)
        {
            try
            {
                string query = "UPDATE fd_doctor SET ACTIVE_STATUS='0', DOCTOR_NAME='" + firstName + " " + surName + "' WHERE DOCTOR_ID_IMPORT=" + userID.ToString();

                //Open connection
                if (connection.State == ConnectionState.Open)
                {
                    //create mysql command
                    MySqlCommand cmd = new MySqlCommand();
                    //Assign the query using CommandText
                    cmd.CommandText = query;
                    //Assign the connection using Connection
                    cmd.Connection = connection;

                    //Execute query
                    cmd.ExecuteNonQuery();

                }
                else
                {
                    log.Write("MYSQL:UpdateDoctor(): DB connection is not connected or broken");
                }
            }
            catch (Exception e)
            {
                log.Write("MYSQL:UpdateDoctor(): failed\n" + e.Message);
            }
        }


        public string InsertDoctor(string surName, string firstName,int userID)
        {
            string result = "";
            try
            {
                string name = firstName + " " + surName;
                string query = "INSERT INTO fd_doctor (DOCTOR_NAME, ACTIVE_STATUS,DOCTOR_ID_IMPORT) VALUES('" + name + "', '0','" + userID.ToString() + "')";
                
                //open connection
                if (connection.State == ConnectionState.Open)
                {
                    //create command and assign the query and connection from the constructor
                    MySqlCommand cmd = new MySqlCommand(query, connection);

                    //Execute command
                    cmd.ExecuteNonQuery();

                    query = "SELECT LAST_INSERT_ID() as PRIMARYKEY;";

                    //Create Command
                    cmd = new MySqlCommand(query, connection);
                    //Create a data reader and Execute the command
                    MySqlDataReader dataReader = cmd.ExecuteReader();

                    //Read the data and store them in the list
                    while (dataReader.Read())
                    {
                        result = dataReader["PRIMARYKEY"].ToString();

                    }

                    //close Data Reader
                    dataReader.Close();

                    //close Connection
                    //this.CloseConnection();

                    //return list to be displayed
                    return result;


                }
                else
                {
                    log.Write("MYSQL:InsertDoctor(): DB connection is not connected or broken");
                    return result;
                }
            }
            catch (Exception e)
            {
                log.Write("MYSQL:InsertDoctor(): failed\n" + e.Message);
                return result;
            }
        }


       


        public void Insert_fd_rel_clinic_doctor(string doctorKey, string clinicKey, string clinicMail)
        {
            try
            {
                string query = "INSERT INTO fd_rel_clinic_doctor (CLINIC_USER_ID, DOCTOR_ID, CREATE_USER) VALUES('" + clinicKey + "','" + doctorKey + "','" + clinicMail + "')";

                //open connection
                if (connection.State == ConnectionState.Open)
                {
                    //create command and assign the query and connection from the constructor
                    MySqlCommand cmd = new MySqlCommand(query, connection);

                    //Execute command
                    cmd.ExecuteNonQuery();

                }
                else
                {
                    log.Write("MYSQL:Insert_fd_rel_clinic_doctor(): DB connection is not connected or broken");
                }
            }
            catch (Exception e)
            {
                log.Write("MYSQL:Insert_fd_rel_clinic_doctor(): failed\n" + e.Message);
            }
        }
        

        public string GetHuangYeDOCTOR_ID(string ClinicID)
        {
            string DOCTOR_ID = "";
            try
            {
                string query = "SELECT DOCTOR_ID FROM fd_doctor where DOCTOR_ID_IMPORT=" + ClinicID;

                if (connection.State == ConnectionState.Open)
                {
                    MySqlCommand cmd = new MySqlCommand(query, connection);

                    MySqlDataReader dataReader = cmd.ExecuteReader();

                    //Read the data and store them in the list
                    while (dataReader.Read())
                    {
                        DOCTOR_ID = dataReader["DOCTOR_ID"].ToString();
                    }

                    //close Data Reader
                    dataReader.Close();
                    return DOCTOR_ID;
                }
                else
                {
                    log.Write("MYSQL:GetHuangYeDOCTOR_ID(): DB connection is not connected or broken");
                    return DOCTOR_ID;
                }
            }
            catch (Exception e)
            {
                log.Write("MYSQL:GetHuangYeDOCTOR_ID(): failed\n" + e.Message);
                return DOCTOR_ID;
            }
        }

        public bool AppExistInTable(DataRow dr)
        {
            int Count = -1;
            try
            {
                if (connection.State == ConnectionState.Open)
                {
                    if (GetHuangYeDOCTOR_ID(dr["DOCTOR_ID"].ToString()).Equals(""))
                        return false;

                    string query = "SELECT  Count(*) FROM fd_rel_doctor_appointment_time where DOCTOR_ID='"
                        + GetHuangYeDOCTOR_ID(dr["DOCTOR_ID"].ToString())
                        + "' AND APPOINTMENT_DATE= '"
                        + dr["APPOINTMENT_DATE"]
                        + "' AND APPOINTMENT_TIME= '"
                        + dr["APPOINTMENT_TIME"]
                        + "'"
                        ;
                    MySqlCommand cmd = new MySqlCommand(query, connection);

                    Count = int.Parse(cmd.ExecuteScalar() + "");

                    return Count > 0;
                }
                else
                {
                    log.Write("MYSQL:AppExistInTable(): DB connection is not connected or broken");
                    return Count > 0;
                }
            }
            catch (Exception e)
            {
                log.Write("MYSQL:AppExistInTable(): failed\n" + e.Message);
                return Count > 0;
            }



        }

        

        public void InsertAppointment(DataRow dr)
        {

            try
            {
                //open connection
                if (connection.State == ConnectionState.Open)
                {

                    string query = "INSERT INTO fd_rel_doctor_appointment_time (DOCTOR_ID, APPOINTMENT_DATE, APPOINTMENT_TIME, ACTIVE_STATUS) VALUES('"
                    + GetHuangYeDOCTOR_ID(dr["DOCTOR_ID"].ToString())
                    + "','"
                    + dr["APPOINTMENT_DATE"].ToString()
                    + "','"
                    + dr["APPOINTMENT_TIME"].ToString()
                    + "','"
                    + dr["ACTIVE_STATUS"].ToString()
                    + "')";

                    //create command and assign the query and connection from the constructor
                    MySqlCommand cmd = new MySqlCommand(query, connection);

                    //Execute command
                    cmd.ExecuteNonQuery();

                }
                else
                {
                    log.Write("MYSQL:InsertAppointment(): DB connection is not connected or broken");
                }
            }
            catch (Exception e)
            {
                log.Write("MYSQL:InsertAppointment(): failed\n" + e.Message);
            }
        }

        public string GetAppActive(DataRow dr)
        {
            string result = "";

            try
            {
                if (connection.State == ConnectionState.Open)
                {

                    string query = "SELECT ACTIVE_STATUS FROM fd_rel_doctor_appointment_time where DOCTOR_ID='"
                    + GetHuangYeDOCTOR_ID(dr["DOCTOR_ID"].ToString())
                    + "' AND APPOINTMENT_DATE= '"
                        + dr["APPOINTMENT_DATE"]
                        + "' AND APPOINTMENT_TIME= '"
                        + dr["APPOINTMENT_TIME"]
                        + "'";
                    //Create Command
                    MySqlCommand cmd = new MySqlCommand(query, connection);
                    //Create a data reader and Execute the command
                    MySqlDataReader dataReader = cmd.ExecuteReader();

                    //Read the data and store them in the list
                    while (dataReader.Read())
                    {
                        result = dataReader["ACTIVE_STATUS"].ToString();
                    }

                    dataReader.Close();
                    return result;
                }
                else
                {
                    log.Write("MYSQL:GetAppActive(): DB connection is not connected or broken");
                    return result;
                }
            }
            catch (Exception e)
            {
                log.Write("MYSQL:GetAppActive(): failed\n" + e.Message);
                return result;
            }
        }


        
        public void UpdateAppointment(DataRow dr)
        {

            try
            {
                if (connection.State == ConnectionState.Open)
                {

                    string query = "UPDATE fd_rel_doctor_appointment_time SET ACTIVE_STATUS="
                    + dr["ACTIVE_STATUS"]
                    + " WHERE DOCTOR_ID="
                    + GetHuangYeDOCTOR_ID(dr["DOCTOR_ID"].ToString())
                    + " AND APPOINTMENT_DATE= '"
                        + dr["APPOINTMENT_DATE"]
                        + "' AND APPOINTMENT_TIME= '"
                        + dr["APPOINTMENT_TIME"]
                        + "'";
                    //create mysql command
                    MySqlCommand cmd = new MySqlCommand();
                    //Assign the query using CommandText
                    cmd.CommandText = query;
                    //Assign the connection using Connection
                    cmd.Connection = connection;

                    cmd.ExecuteNonQuery();
                }
                else
                {
                    log.Write("MYSQL:UpdateAppointment(): DB connection is not connected or broken");
                }
            }
            catch (Exception e)
            {
                log.Write("MYSQL:UpdateAppointment(): failed\n" + e.Message);
            }
        }





        //_____________________________________________________________________________________FD->BP 2s task
        public DataTable GetAppointmentRequests(string clinicID)
        {
            try
            {
                string query = "SELECT DOCTOR_APPOINTMENT_TIME_ID,DOCTOR_ID_IMPORT,APPOINTMENT_DATE,APPOINTMENT_TIME,CUSTOMER_USER_MAIL,CUSTOMER_FIRSTNAME,CUSTOMER_LASTNAME,TITLE_ID,GENDER_ID,CUSTOMER_BIRTHDAY,CUSTOMER_ADDR,CUSTOMER_POSTCODE,CUSTOMER_SUBURB,STATE_ID,CUSTOMER_PHONE_NO,MEDICAL_CARD_NO FROM fd_rel_doctor_appointment_time a LEFT JOIN fd_doctor b ON a.DOCTOR_ID = b.DOCTOR_ID left join fd_rel_clinic_doctor c on a.DOCTOR_ID = c.DOCTOR_ID LEFT JOIN fd_customer_user d ON a.REQUESTING_USER_ID = d.CUSTOMER_USER_ID  WHERE REQUESTING_FLAG=1 and CLINIC_USER_ID="
                    + clinicID;

                DataTable re = new DataTable();
                re.Clear();
                re.Columns.Add("DOCTOR_APPOINTMENT_TIME_ID");
                re.Columns.Add("DOCTOR_ID_IMPORT");
                re.Columns.Add("APPOINTMENT_DATE");
                re.Columns.Add("APPOINTMENT_TIME");
                re.Columns.Add("CUSTOMER_USER_MAIL");
                re.Columns.Add("CUSTOMER_FIRSTNAME");
                re.Columns.Add("CUSTOMER_LASTNAME");
                re.Columns.Add("TITLE_ID");
                re.Columns.Add("GENDER_ID");
                re.Columns.Add("CUSTOMER_BIRTHDAY");
                re.Columns.Add("CUSTOMER_ADDR");
                re.Columns.Add("CUSTOMER_POSTCODE");
                re.Columns.Add("CUSTOMER_SUBURB");
                re.Columns.Add("STATE_ID");
                re.Columns.Add("CUSTOMER_PHONE_NO");
                re.Columns.Add("MEDICAL_CARD_NO");

                //Open connection
                if (connection.State == ConnectionState.Open)
                {
                    //Create Command
                    MySqlCommand cmd = new MySqlCommand(query, connection);
                    //Create a data reader and Execute the command
                    MySqlDataReader dataReader = cmd.ExecuteReader();

                    //Read the data and store them in the list
                    while (dataReader.Read())
                    {
                        DataRow _r = re.NewRow();
                        _r["DOCTOR_APPOINTMENT_TIME_ID"] = dataReader["DOCTOR_APPOINTMENT_TIME_ID"].ToString();
                        _r["DOCTOR_ID_IMPORT"] = dataReader["DOCTOR_ID_IMPORT"].ToString();
                        _r["APPOINTMENT_DATE"] = dataReader["APPOINTMENT_DATE"].ToString();
                        _r["APPOINTMENT_TIME"] = dataReader["APPOINTMENT_TIME"].ToString();
                        _r["CUSTOMER_USER_MAIL"] = dataReader["CUSTOMER_USER_MAIL"].ToString();
                        _r["CUSTOMER_FIRSTNAME"] = dataReader["CUSTOMER_FIRSTNAME"].ToString();
                        _r["CUSTOMER_LASTNAME"] = dataReader["CUSTOMER_LASTNAME"].ToString();
                        _r["TITLE_ID"] = dataReader["TITLE_ID"].ToString();
                        _r["GENDER_ID"] = dataReader["GENDER_ID"].ToString();
                        _r["CUSTOMER_BIRTHDAY"] = dataReader["CUSTOMER_BIRTHDAY"].ToString();
                        _r["CUSTOMER_ADDR"] = dataReader["CUSTOMER_ADDR"].ToString();
                        _r["CUSTOMER_POSTCODE"] = dataReader["CUSTOMER_POSTCODE"].ToString();
                        _r["CUSTOMER_SUBURB"] = dataReader["CUSTOMER_SUBURB"].ToString();
                        _r["STATE_ID"] = dataReader["STATE_ID"].ToString();
                        _r["CUSTOMER_PHONE_NO"] = dataReader["CUSTOMER_PHONE_NO"].ToString();
                        _r["MEDICAL_CARD_NO"] = dataReader["MEDICAL_CARD_NO"].ToString();
                        re.Rows.Add(_r);
                    }


                    dataReader.Close();

                    return re;
                }
                else
                {
                    log.Write("MYSQL:GetAppointmentRequests(): DB connection is not connected or broken");
                    return null;
                }
            }
            catch (Exception e)
            {
                log.Write("MYSQL:GetAppointmentRequests(): failed\n" + e.Message);
                return null;
            }

        }

        public void ResetAllRequestFlag(string clinicID)
        {
            try
            {
                if (connection.State == ConnectionState.Open)
                {
                    string query = "SET SQL_SAFE_UPDATES = 0; UPDATE fd_rel_doctor_appointment_time a left JOIN `fd_rel_clinic_doctor` b ON a.`DOCTOR_ID` = b.`DOCTOR_ID` SET a.REQUESTING_FLAG = 0,a.REQUESTING_USER_ID = 0 WHERE b.`CLINIC_USER_ID` = " +
                        clinicID;

                    MySqlCommand cmd = new MySqlCommand();

                    cmd.CommandText = query;
                    cmd.Connection = connection;
                    cmd.ExecuteNonQuery();

                }
                else
                {
                    log.Write("MYSQL:ResetAllRequestFlag(): DB connection is not connected or broken");
                }
            }
            catch (Exception e)
            {
                log.Write("MYSQL:ResetAllRequestFlag(): failed\n" + e.Message);
            }
        }

        public void SetSuccessfulTo2(string id)//2 means ocupied, failed
        {
            try
            {
                //Open connection
                if (connection.State == ConnectionState.Open)
                {
                    string query = "UPDATE fd_rel_doctor_appointment_time SET SUCCESSFUL_FLAG=2 where DOCTOR_APPOINTMENT_TIME_ID=" + id;
                    //create mysql command
                    MySqlCommand cmd = new MySqlCommand();
                    //Assign the query using CommandText
                    cmd.CommandText = query;
                    //Assign the connection using Connection
                    cmd.Connection = connection;

                    //Execute query
                    cmd.ExecuteNonQuery();

                }
                else
                {
                    log.Write("MYSQL:SetSuccessfulTo2(): DB connection is not connected or broken");
                }
            }
            catch (Exception e)
            {
                log.Write("MYSQL:SetSuccessfulTo2(): failed\n" + e.Message);
            }

        }

        public void SetSuccessfulTo1(string id)//1 means successful
        {
            try
            {
                //Open connection
                if (connection.State == ConnectionState.Open)
                {
                    string query = "UPDATE fd_rel_doctor_appointment_time SET SUCCESSFUL_FLAG=1 where DOCTOR_APPOINTMENT_TIME_ID=" + id;
                    //create mysql command
                    MySqlCommand cmd = new MySqlCommand();
                    //Assign the query using CommandText
                    cmd.CommandText = query;
                    //Assign the connection using Connection
                    cmd.Connection = connection;

                    //Execute query
                    cmd.ExecuteNonQuery();
                }
                else
                {
                    log.Write("MYSQL:SetSuccessfulTo1(): DB connection is not connected or broken");
                }
            }
            catch (Exception e)
            {
                log.Write("MYSQL:SetSuccessfulTo1(): failed\n" + e.Message);
            }
        }


        //_____________________________________________________________________________________________________________DEMO LIBs
        //Update statement
        public void Update()
        {
            string query = "UPDATE tableinfo SET name='Joe', age='22' WHERE name='John Smith'";

            //Open connection
            if (this.OpenConnection() == true)
            {
                //create mysql command
                MySqlCommand cmd = new MySqlCommand();
                //Assign the query using CommandText
                cmd.CommandText = query;
                //Assign the connection using Connection
                cmd.Connection = connection;

                //Execute query
                cmd.ExecuteNonQuery();

                //close connection
                this.CloseConnection();
            }
        }

        //Delete statement
        public void Delete()
        {
            string query = "DELETE FROM tableinfo WHERE name='John Smith'";

            if (this.OpenConnection() == true)
            {
                MySqlCommand cmd = new MySqlCommand(query, connection);
                cmd.ExecuteNonQuery();
                this.CloseConnection();
            }
        }

        //Select statement
        public List<string>[] Select()
        {
            string query = "SELECT * FROM tableinfo";

            //Create a list to store the result
            List<string>[] list = new List<string>[3];
            list[0] = new List<string>();
            list[1] = new List<string>();
            list[2] = new List<string>();

            //Open connection
            if (this.OpenConnection() == true)
            {
                //Create Command
                MySqlCommand cmd = new MySqlCommand(query, connection);
                //Create a data reader and Execute the command
                MySqlDataReader dataReader = cmd.ExecuteReader();
                
                //Read the data and store them in the list
                while (dataReader.Read())
                {
                    list[0].Add(dataReader["id"] + "");
                    list[1].Add(dataReader["name"] + "");
                    list[2].Add(dataReader["age"] + "");
                }

                //close Data Reader
                dataReader.Close();

                //close Connection
                this.CloseConnection();

                //return list to be displayed
                return list;
            }
            else
            {
                return list;
            }
        }

        //Count statement
        public int Count()
        {
            string query = "SELECT Count(*) FROM tableinfo";
            int Count = -1;

            //Open Connection
            if (this.OpenConnection() == true)
            {
                //Create Mysql Command
                MySqlCommand cmd = new MySqlCommand(query, connection);

                //ExecuteScalar will return one value
                Count = int.Parse(cmd.ExecuteScalar()+"");
                
                //close Connection
                this.CloseConnection();

                return Count;
            }
            else
            {
                return Count;
            }
        }

        //Backup
        public void Backup()
        {
            try
            {
                DateTime Time = DateTime.Now;
                int year = Time.Year;
                int month = Time.Month;
                int day = Time.Day;
                int hour = Time.Hour;
                int minute = Time.Minute;
                int second = Time.Second;
                int millisecond = Time.Millisecond;

                //Save file to C:\ with the current date as a filename
                string path;
                path = "C:\\" + year + "-" + month + "-" + day + "-" + hour + "-" + minute + "-" + second + "-" + millisecond + ".sql";
                StreamWriter file = new StreamWriter(path);

                
                ProcessStartInfo psi = new ProcessStartInfo();
                psi.FileName = "mysqldump";
                psi.RedirectStandardInput = false;
                psi.RedirectStandardOutput = true;
                psi.Arguments = string.Format(@"-u{0} -p{1} -h{2} {3}", FOCUSDATA_DATABASE_USER, FOCUSDATA_DATABASE_PASS, FOCUSDATA_DATABASE_HOST, FOCUSDATA_DATABASE_NAME);
                psi.UseShellExecute = false;

                Process process = Process.Start(psi);

                string output;
                output = process.StandardOutput.ReadToEnd();
                file.WriteLine(output);
                process.WaitForExit();
                file.Close();
                process.Close();
            }
            catch (IOException ex)
            {
                Console.WriteLine("Error , unable to backup!"+ex.Message);
            }
        }

        //Restore
        public void Restore()
        {
            try
            {
                //Read file from C:\
                string path;
                path = "C:\\MySqlBackup.sql";
                StreamReader file = new StreamReader(path);
                string input = file.ReadToEnd();
                file.Close();


                ProcessStartInfo psi = new ProcessStartInfo();
                psi.FileName = "mysql";
                psi.RedirectStandardInput = true;
                psi.RedirectStandardOutput = false;
                psi.Arguments = string.Format(@"-u{0} -p{1} -h{2} {3}", FOCUSDATA_DATABASE_USER, FOCUSDATA_DATABASE_PASS, FOCUSDATA_DATABASE_HOST, FOCUSDATA_DATABASE_NAME);
                psi.UseShellExecute = false;

                
                Process process = Process.Start(psi);
                process.StandardInput.WriteLine(input);
                process.StandardInput.Close();
                process.WaitForExit();
                process.Close();
            }
            catch (IOException ex)
            {
                Console.WriteLine("Error , unable to Restore!"+ex.Message);
            }
        }
        /***            DEMO END         ***/
    }
}