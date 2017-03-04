using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.IO;
//Add MySql Library
using MySql.Data.MySqlClient;
using System.Configuration;
using System.Data;

namespace FocusDataBridge
{
    class MysqlConnect
    {
        private MySqlConnection connection;
        private string server;
        private string database;
        private string uid;
        private string password;

        //Constructor
        public MysqlConnect()
        {
            Initialize();
        }

        //Initialize values
        private void Initialize()
        {

            string FOCUSDATA_DATABASE_HOST = ConfigurationManager.AppSettings["FOCUSDATA_DATABASE_HOST"];
            string FOCUSDATA_DATABASE_NAME = ConfigurationManager.AppSettings["FOCUSDATA_DATABASE_NAME"];
            string FOCUSDATA_DATABASE_USER = ConfigurationManager.AppSettings["FOCUSDATA_DATABASE_USER"];
            string FOCUSDATA_DATABASE_PASS = ConfigurationManager.AppSettings["FOCUSDATA_DATABASE_PASS"];

            string connectionString;
            connectionString = "SERVER=" + FOCUSDATA_DATABASE_HOST + ";" + "DATABASE=" + FOCUSDATA_DATABASE_NAME + ";" + "UID=" + FOCUSDATA_DATABASE_USER + ";" + "PASSWORD=" + FOCUSDATA_DATABASE_PASS + ";";

            connection = new MySqlConnection(connectionString);
        }


        //open connection to database
        private bool OpenConnection()
        {
            try
            {
                connection.Open();
                LogWriter.LogWrite("Connect to Focusdata successfully");
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
                    case 0:
                        Console.WriteLine("Cannot connect to server.  Contact administrator");
                        LogWriter.LogWrite("Cannot connect to focusdata server.  Contact administrator");
                        break;

                    case 1045:
                        Console.WriteLine("Invalid username/password, please try again");
                        LogWriter.LogWrite("Invalid focusdata server username/password, please try again");
                        break;
                }
                return false;
            }
        }

        //Close connection
        private bool CloseConnection()
        {
            try
            {
                connection.Close();
                return true;
            }
            catch (MySqlException ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }
        }

      
        /***            fd_doctor BEGIN         ***/

        public bool IDExistInTable(int userID)
        {
            string query = "SELECT Count(*) FROM fd_doctor where DOCTOR_ID_IMPORT="+userID.ToString();
            int Count = -1;

            //Open Connection
            if (this.OpenConnection() == true)
            {
                //Create Mysql Command
                MySqlCommand cmd = new MySqlCommand(query, connection);

                //ExecuteScalar will return one value
                Count = int.Parse(cmd.ExecuteScalar() + "");

                //close Connection
                this.CloseConnection();

                return Count>0;
            }
            else
            {
                return Count > 0;
            }
        }
        

        public string GetDoctorName(int userID)
        {
            string query = "SELECT DOCTOR_NAME FROM fd_doctor where DOCTOR_ID_IMPORT="+ userID.ToString();

            string result="";
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
                    result = dataReader["DOCTOR_NAME"].ToString();
                }

                //close Data Reader
                dataReader.Close();

                //close Connection
                this.CloseConnection();

                //return list to be displayed
                return result;
            }
            else
            {
                return result;
            }
        }

        public void UpdateDoctor(string surName, string firstName, int userID)
        {
            string query = "UPDATE fd_doctor SET DOCTOR_NAME='"+ firstName +" "+ surName + "' WHERE DOCTOR_ID_IMPORT="+ userID.ToString();

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


        public void InsertDoctor(String surName,String firstName,int userID)
        {
            string name = firstName + " " + surName;
            string query = "INSERT INTO fd_doctor (DOCTOR_NAME, ACTIVE_STATUS,DOCTOR_ID_IMPORT) VALUES('"+name +"', '1','"+ userID.ToString() + "')";

            //open connection
            if (this.OpenConnection() == true)
            {
                //create command and assign the query and connection from the constructor
                MySqlCommand cmd = new MySqlCommand(query, connection);
                
                //Execute command
                cmd.ExecuteNonQuery();

                //close connection
                this.CloseConnection();
            }
        }

        /***            fd_doctor END         ***/


        /***            fd_rel_doctor_appointment_time BEGIN         ***/
        public string GetHuangYeDOCTOR_ID(string ClinicID)
        {
            string query = "SELECT DOCTOR_ID FROM fd_doctor where DOCTOR_ID_IMPORT=" + ClinicID;
            string DOCTOR_ID = "";

            

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

        public bool AppExistInTable(DataRow dr)
        {
            int Count = -1;
            if (this.OpenConnection() == true)
            {

                if (GetHuangYeDOCTOR_ID(dr["DOCTOR_ID"].ToString()).Equals(""))
                    return false;

                string query = "SELECT  Count(*) FROM fd_rel_doctor_appointment_time where DOCTOR_ID='" 
                    + GetHuangYeDOCTOR_ID(dr["DOCTOR_ID"].ToString())
                    + "' AND APPOINTMENT_DATE= '"
                    + dr["APPOINTMENT_DATE"]
                    + "' AND APPOINTMENT_TIME= '"
                    + dr["APPOINTMENT_TIME"]
                    +"'"
                    ;
                MySqlCommand cmd = new MySqlCommand(query, connection);

                Count = int.Parse(cmd.ExecuteScalar() + "");

                //close Connection
                this.CloseConnection();

                return Count > 0;


            }

            return false;

        }

        

        public void InsertAppointment(DataRow dr)
        {

  
            //open connection
            if (this.OpenConnection() == true)
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

                //close connection
                this.CloseConnection();
            }
        }

        public string GetAppActive(DataRow dr)
        {
            

            string result = "";
            //Open connection
            if (this.OpenConnection() == true)
            {

                string query = "SELECT ACTIVE_STATUS FROM fd_rel_doctor_appointment_time where DOCTOR_ID='"
                + GetHuangYeDOCTOR_ID(dr["DOCTOR_ID"].ToString())
                + "' AND APPOINTMENT_DATE= '"
                    + dr["APPOINTMENT_DATE"]
                    + "' AND APPOINTMENT_TIME= '"
                    + dr["APPOINTMENT_TIME"]
                    +"'";
                //Create Command
                MySqlCommand cmd = new MySqlCommand(query, connection);
                //Create a data reader and Execute the command
                MySqlDataReader dataReader = cmd.ExecuteReader();

                //Read the data and store them in the list
                while (dataReader.Read())
                {
                    result = dataReader["ACTIVE_STATUS"].ToString();
                }

                //close Data Reader
                dataReader.Close();

                //close Connection
                this.CloseConnection();

                //return list to be displayed
                return result;
            }
            else
            {
                return result;
            }
        }


        
        public void UpdateAppointment(DataRow dr)
        {

            //Open connection
            if (this.OpenConnection() == true)
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

                //Execute query
                cmd.ExecuteNonQuery();

                //close connection
                this.CloseConnection();
            }
        }
        /***            fd_rel_doctor_appointment_time END         ***/




        /***            ForClinic BEGIN         ***/
        public DataTable GetAppointmentRequests()
        {
            string query = "SELECT DOCTOR_APPOINTMENT_TIME_ID,DOCTOR_ID_IMPORT,APPOINTMENT_DATE,APPOINTMENT_TIME,CUSTOMER_USER_MAIL,CUSTOMER_FIRSTNAME,CUSTOMER_LASTNAME,TITLE_ID,GENDER_ID,CUSTOMER_BIRTHDAY,CUSTOMER_ADDR,CUSTOMER_POSTCODE,CUSTOMER_SUBURB,STATE_ID,CUSTOMER_PHONE_NO,MEDICAL_CARD_NO FROM fd_rel_doctor_appointment_time a left join fd_doctor b on a.DOCTOR_ID=b.DOCTOR_ID left join fd_customer_user c on a.REQUESTING_USER_ID=c.CUSTOMER_USER_ID WHERE REQUESTING_FLAG=1";

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
            if (this.OpenConnection() == true)
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

                //close Data Reader
                dataReader.Close();

                //close Connection
                this.CloseConnection();

                //return list to be displayed
                return re;
            }
            else
            {
                return re;
            }
        }

        public void ResetAllRequestFlag()
        {
            //Open connection
            if (this.OpenConnection() == true)
            {

                string query = "UPDATE fd_rel_doctor_appointment_time SET REQUESTING_FLAG=0,REQUESTING_USER_ID=0";
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

        public void SetSuccessfulTo2(string id)//2 means ocupied, failed
        {
            //Open connection
            if (this.OpenConnection() == true)
            {
                string query = "UPDATE fd_rel_doctor_appointment_time SET SUCCESSFUL_FLAG=2 where DOCTOR_APPOINTMENT_TIME_ID="+id;
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

        public void SetSuccessfulTo1(string id)//1 means successful
        {
            //Open connection
            if (this.OpenConnection() == true)
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

                //close connection
                this.CloseConnection();
            }
        }

        /***            ForClinic END         ***/


        /***            DEMO BEGIN         ***/
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
                psi.Arguments = string.Format(@"-u{0} -p{1} -h{2} {3}", uid, password, server, database);
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
                Console.WriteLine("Error , unable to backup!");
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
                psi.Arguments = string.Format(@"-u{0} -p{1} -h{2} {3}", uid, password, server, database);
                psi.UseShellExecute = false;

                
                Process process = Process.Start(psi);
                process.StandardInput.WriteLine(input);
                process.StandardInput.Close();
                process.WaitForExit();
                process.Close();
            }
            catch (IOException ex)
            {
                Console.WriteLine("Error , unable to Restore!");
            }
        }
        /***            DEMO END         ***/
    }
}
