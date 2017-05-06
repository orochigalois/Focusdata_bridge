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

        //Constructor
        public MysqlConnect(LogWriter log)
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

            string FOCUSDATA_DATABASE_HOST = ConfigurationManager.AppSettings["FOCUSDATA_DATABASE_HOST"];
            string FOCUSDATA_DATABASE_NAME = ConfigurationManager.AppSettings["FOCUSDATA_DATABASE_NAME"];
            string FOCUSDATA_DATABASE_USER = ConfigurationManager.AppSettings["FOCUSDATA_DATABASE_USER"];
            string FOCUSDATA_DATABASE_PASS = ConfigurationManager.AppSettings["FOCUSDATA_DATABASE_PASS"];

            string connectionString = "SERVER=" + FOCUSDATA_DATABASE_HOST + ";" + "DATABASE=" + FOCUSDATA_DATABASE_NAME + ";" + "UID=" + FOCUSDATA_DATABASE_USER + ";" + "PASSWORD=" + FOCUSDATA_DATABASE_PASS + ";";
            return connectionString;
        }



        public DataTable GetAllDoctors(string clinicID)
        {
            try
            {
                DataTable dtDoctors = new DataTable();
                dtDoctors.Clear();
                dtDoctors.Columns.Add("fullName");
                dtDoctors.Columns.Add("userID");

                string query = "SELECT a.DOCTOR_NAME,a.DOCTOR_ID_IMPORT FROM fd_doctor a left join fd_rel_clinic_doctor b on a.DOCTOR_ID = b.DOCTOR_ID where b.CLINIC_USER_ID=" + clinicID;


                using (MySqlConnection connection = new MySqlConnection(PrepareConnectionString()))
                {
                    connection.Open();
                    using (MySqlCommand cmd = new MySqlCommand(query, connection))
                    {
                        using (MySqlDataReader reader = cmd.ExecuteReader())
                        {
                            string fullName = "";
                            int userID = 0;
                            if (reader != null)
                            {
                                while (reader.Read())
                                {
                                    fullName = (string)reader["DOCTOR_NAME"];
                                    userID = (int)reader["UserID"];

                                    DataRow _r = dtDoctors.NewRow();
                                    _r["fullName"] = fullName;
                                    _r["userID"] = userID;
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
                log.Write("MYSQL:GetAllDoctors(): failed\n" + e.Message);
                return null;
            }

        }



        /// <summary>
        /// 
        /// </summary>
        /// <param name="CLINIC_USER_EMAIL"></param>
        /// <returns></returns>
        public bool CLINIC_USER_MAIL_ExistInTable(String CLINIC_USER_EMAIL)
        {
            int Count = -1;
            try
            {
                string query = "SELECT Count(*) FROM fd_clinic_user where CLINIC_USER_MAIL='" + CLINIC_USER_EMAIL + "'";
                using (MySqlConnection connection = new MySqlConnection(PrepareConnectionString()))
                {
                    connection.Open();
                    using (MySqlCommand cmd = new MySqlCommand(query, connection))
                    {
                        Count = int.Parse(cmd.ExecuteScalar() + "");
                    }
                }
                return Count > 0;


            }
            catch (Exception e)
            {
                log.Write("MYSQL:CLINIC_USER_MAIL_ExistInTable(): failed\n" + e.Message);
                return false;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="mail"></param>
        /// <returns></returns>
        public string GetClinicKey(string mail)
        {
            string result = "";
            string query = "SELECT CLINIC_USER_ID FROM fd_clinic_user where CLINIC_USER_MAIL='" + mail + "'";
            try
            {
                using (MySqlConnection connection = new MySqlConnection(PrepareConnectionString()))
                {
                    connection.Open();
                    using (MySqlCommand cmd = new MySqlCommand(query, connection))
                    {
                        using (MySqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader != null)
                            {
                                while (reader.Read())
                                {
                                    result = reader["CLINIC_USER_ID"].ToString();
                                }
                            }
                        }
                    }
                }
                return result;

            }
            catch (Exception e)
            {
                log.Write("MYSQL:GetClinicKey(): failed\n" + e.Message);
                return result;
            }

        }



        /// <summary>
        /// 
        /// </summary>
        /// <param name="userID"></param>
        /// <param name="clinicID"></param>
        /// <returns></returns>
        public bool DoctorExist(string userID, string clinicID)
        {
            int Count = -1;
            string query = "SELECT Count(*) FROM fd_doctor a left join fd_rel_clinic_doctor b on a.DOCTOR_ID = b.DOCTOR_ID where b.CLINIC_USER_ID=" + clinicID + " and a.DOCTOR_ID_IMPORT=" + userID;
            try
            {
                using (MySqlConnection connection = new MySqlConnection(PrepareConnectionString()))
                {
                    connection.Open();
                    using (MySqlCommand cmd = new MySqlCommand(query, connection))
                    {
                        Count = int.Parse(cmd.ExecuteScalar() + "");
                        
                    }
                }
                return Count > 0;
            }
            catch (Exception e)
            {
                log.Write("MYSQL:DoctorExist(): failed\n" + e.Message);
                return false;
            }

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="userID"></param>
        /// <param name="clinicID"></param>
        /// <returns></returns>
        public string GetDoctorName(string userID, string clinicID)
        {
            string result = "";
            string query = "SELECT a.DOCTOR_NAME FROM fd_doctor a left join fd_rel_clinic_doctor b on a.DOCTOR_ID = b.DOCTOR_ID where b.CLINIC_USER_ID=" + clinicID + " and a.DOCTOR_ID_IMPORT=" + userID;
            try
            {
                using (MySqlConnection connection = new MySqlConnection(PrepareConnectionString()))
                {
                    connection.Open();
                    using (MySqlCommand cmd = new MySqlCommand(query, connection))
                    {
                        using (MySqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader != null)
                            {
                                while (reader.Read())
                                {
                                    result = reader["DOCTOR_NAME"].ToString();
                                }
                            }
                        }
                    }
                }
                return result;
            }
            catch (Exception e)
            {
                log.Write("MYSQL:GetDoctorName(): failed\n" + e.Message);
                return result;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="surName"></param>
        /// <param name="firstName"></param>
        /// <param name="userID"></param>
        /// <param name="clinicID"></param>
        public void UpdateDoctor(string surName, string firstName, string userID, string clinicID, string update_user)
        {

            try
            {
                string query = "UPDATE fd_doctor a left join fd_rel_clinic_doctor b on a.DOCTOR_ID = b.DOCTOR_ID SET a.ACTIVE_STATUS='0', a.DOCTOR_NAME='" + firstName + " " + surName + "',a.UPDATE_USER='"+ update_user + "',a.UPDATE_DATE='"+ DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")+ "' WHERE b.CLINIC_USER_ID=" + clinicID + " and a.DOCTOR_ID_IMPORT=" + userID;

                using (MySqlConnection connection = new MySqlConnection(PrepareConnectionString()))
                {
                    connection.Open();
                    using (MySqlCommand cmd = new MySqlCommand(query, connection))
                    {
                        cmd.ExecuteNonQuery();
                    }
                }

            }
            catch (Exception e)
            {
                log.Write("MYSQL:UpdateDoctor(): failed\n" + e.Message);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="surName"></param>
        /// <param name="firstName"></param>
        /// <param name="userID"></param>
        /// <returns></returns>
        public string InsertDoctor(string surName, string firstName, string userID, string create_user)
        {
            string result = "";
            try
            {
                string name = firstName + " " + surName;
                string query = "INSERT INTO fd_doctor (DOCTOR_NAME, ACTIVE_STATUS,DOCTOR_ID_IMPORT,CREATE_USER,CREATE_DATE) VALUES('" + name + "', '0','" + userID + "','"+ create_user + "','"+ DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "')";
                using (MySqlConnection connection = new MySqlConnection(PrepareConnectionString()))
                {
                    connection.Open();
                    using (MySqlCommand cmd = new MySqlCommand(query, connection))
                    {
                        cmd.ExecuteNonQuery();
                    }
                    query = "SELECT LAST_INSERT_ID() as PRIMARYKEY;";
                    using (MySqlCommand cmd = new MySqlCommand(query, connection))
                    {
                        using (MySqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader != null)
                            {
                                while (reader.Read())
                                {
                                    result = reader["PRIMARYKEY"].ToString();
                                }
                            }
                        }
                    }
                }

                return result;
            }
            catch (Exception e)
            {
                log.Write("MYSQL:InsertDoctor(): failed\n" + e.Message);
                return result;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="doctorKey"></param>
        /// <param name="clinicKey"></param>
        /// <param name="create_user"></param>
        public void Insert_fd_rel_clinic_doctor(string doctorKey, string clinicKey, string create_user)
        {
            try
            {
                string query = "INSERT INTO fd_rel_clinic_doctor (CLINIC_USER_ID, DOCTOR_ID, CREATE_USER, CREATE_DATE) VALUES('" + clinicKey + "','" + doctorKey + "','" + create_user + "','" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "')";

                using (MySqlConnection connection = new MySqlConnection(PrepareConnectionString()))
                {
                    connection.Open();
                    using (MySqlCommand cmd = new MySqlCommand(query, connection))
                    {
                        cmd.ExecuteNonQuery();
                    }
                }

            }
            catch (Exception e)
            {
                log.Write("MYSQL:Insert_fd_rel_clinic_doctor(): failed\n" + e.Message);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="userID"></param>
        /// <param name="clinicID"></param>
        /// <returns></returns>
        public string GetDoctorID(string userID, string clinicID)
        {
            string DOCTOR_ID = "";
            string query = "SELECT a.DOCTOR_ID FROM fd_doctor a left join fd_rel_clinic_doctor b on a.DOCTOR_ID = b.DOCTOR_ID where b.CLINIC_USER_ID=" + clinicID + " and a.DOCTOR_ID_IMPORT=" + userID;
            try
            {
                using (MySqlConnection connection = new MySqlConnection(PrepareConnectionString()))
                {
                    connection.Open();
                    using (MySqlCommand cmd = new MySqlCommand(query, connection))
                    {
                        using (MySqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader != null)
                            {
                                while (reader.Read())
                                {
                                    DOCTOR_ID = reader["DOCTOR_ID"].ToString();
                                }
                            }
                        }
                    }
                }
             
                return DOCTOR_ID;
            }
            catch (Exception e)
            {
                log.Write("MYSQL:GetDoctorID(): failed\n" + e.Message);
                return DOCTOR_ID;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dr"></param>
        /// <param name="clinicID"></param>
        /// <returns></returns>
        public bool AppointmentExist(DataRow dr, string clinicID)
        {
            int Count = -1;
            try
            {
                string doctorID = GetDoctorID(dr["DOCTOR_ID"].ToString(), clinicID);
                if (doctorID.Equals(""))
                    return false;

                string query = "SELECT Count(*) FROM fd_rel_doctor_appointment_time where DOCTOR_ID='"
                    + doctorID
                    + "' AND APPOINTMENT_DATE= '"
                    + dr["APPOINTMENT_DATE"]
                    + "' AND APPOINTMENT_TIME= '"
                    + dr["APPOINTMENT_TIME"]
                    + "'"
                    ;
                using (MySqlConnection connection = new MySqlConnection(PrepareConnectionString()))
                {
                    connection.Open();
                    using (MySqlCommand cmd = new MySqlCommand(query, connection))
                    {
                        Count = int.Parse(cmd.ExecuteScalar() + "");
                    }
                }
                return Count > 0;
                
            }
            catch (Exception e)
            {
                log.Write("MYSQL:AppointmentExist(): failed\n" + e.Message);
                return false;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dr"></param>
        /// <param name="clinicID"></param>
        /// <returns></returns>
        public string GetAppointmentActiveStatus(DataRow dr, string clinicID)
        {
            string result = "";

            try
            {
                string doctorID = GetDoctorID(dr["DOCTOR_ID"].ToString(),clinicID);
                string query = "SELECT ACTIVE_STATUS FROM fd_rel_doctor_appointment_time where DOCTOR_ID='"
                    + doctorID
                    + "' AND APPOINTMENT_DATE= '"
                        + dr["APPOINTMENT_DATE"]
                        + "' AND APPOINTMENT_TIME= '"
                        + dr["APPOINTMENT_TIME"]
                        + "'";

                using (MySqlConnection connection = new MySqlConnection(PrepareConnectionString()))
                {
                    connection.Open();
                    using (MySqlCommand cmd = new MySqlCommand(query, connection))
                    {
                        using (MySqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader != null)
                            {
                                while (reader.Read())
                                {
                                    result = reader["ACTIVE_STATUS"].ToString();
                                }
                            }
                        }
                    }
                }

                return result;
            }
            catch (Exception e)
            {
                log.Write("MYSQL:GetAppActive(): failed\n" + e.Message);
                return result;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dr"></param>
        /// <param name="clinicID"></param>
        /// <param name="update_user"></param>
        public void UpdateAppointment(DataRow dr,string clinicID,string update_user)
        {
            try
            {
                string doctorID = GetDoctorID(dr["DOCTOR_ID"].ToString(), clinicID);

                string query = "UPDATE fd_rel_doctor_appointment_time SET ACTIVE_STATUS="
                    + dr["ACTIVE_STATUS"]
                    + ",UPDATE_USER="+ update_user + ",UPDATE_DATE="+ DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")+ " WHERE DOCTOR_ID="
                    + doctorID
                    + " AND APPOINTMENT_DATE= '"
                        + dr["APPOINTMENT_DATE"]
                        + "' AND APPOINTMENT_TIME= '"
                        + dr["APPOINTMENT_TIME"]
                        + "'";


                using (MySqlConnection connection = new MySqlConnection(PrepareConnectionString()))
                {
                    connection.Open();
                    using (MySqlCommand cmd = new MySqlCommand(query, connection))
                    {
                        cmd.ExecuteNonQuery();
                    }
                }


            }
            catch (Exception e)
            {
                log.Write("MYSQL:UpdateAppointment(): failed\n" + e.Message);
            }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="dr"></param>
        /// <param name="clinicID"></param>
        /// <param name="create_user"></param>
        public void InsertAppointment(DataRow dr, string clinicID, string create_user)
        {
            try
            {
                string doctorID = GetDoctorID(dr["DOCTOR_ID"].ToString(), clinicID);
                string query = "INSERT INTO fd_rel_doctor_appointment_time (DOCTOR_ID, APPOINTMENT_DATE, APPOINTMENT_TIME, ACTIVE_STATUS,CREATE_USER,CREATE_DATE) VALUES('"
                    + doctorID
                    + "','"
                    + dr["APPOINTMENT_DATE"].ToString()
                    + "','"
                    + dr["APPOINTMENT_TIME"].ToString()
                    + "','"
                    + dr["ACTIVE_STATUS"].ToString()
                    + "','"+ create_user + "','"+ DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")+ "')";

                using (MySqlConnection connection = new MySqlConnection(PrepareConnectionString()))
                {
                    connection.Open();
                    using (MySqlCommand cmd = new MySqlCommand(query, connection))
                    {
                        cmd.ExecuteNonQuery();
                    }
                }


            }
            catch (Exception e)
            {
                log.Write("MYSQL:InsertAppointment(): failed\n" + e.Message);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="clinicID"></param>
        /// <returns></returns>
        public DataTable GetAppointmentRequests(string clinicID)
        {
            try
            {
                string query = "SELECT DOCTOR_APPOINTMENT_TIME_ID,DOCTOR_ID_IMPORT,APPOINTMENT_DATE,APPOINTMENT_TIME,CUSTOMER_USER_MAIL,CUSTOMER_FIRSTNAME,CUSTOMER_LASTNAME,TITLE_ID,GENDER_ID,CUSTOMER_BIRTHDAY,CUSTOMER_ADDR,CUSTOMER_POSTCODE,CUSTOMER_SUBURB,STATE_ID,CUSTOMER_PHONE_NO,MEDICAL_CARD_NO FROM fd_rel_doctor_appointment_time a LEFT JOIN fd_doctor b ON a.DOCTOR_ID = b.DOCTOR_ID left join fd_rel_clinic_doctor c on a.DOCTOR_ID = c.DOCTOR_ID LEFT JOIN fd_customer_user d ON a.REQUESTING_USER_ID = d.CUSTOMER_USER_ID  WHERE REQUESTING_FLAG=1 and c.CLINIC_USER_ID="
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

                using (MySqlConnection connection = new MySqlConnection(PrepareConnectionString()))
                {
                    connection.Open();
                    using (MySqlCommand cmd = new MySqlCommand(query, connection))
                    {
                        using (MySqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader != null)
                            {
                                while (reader.Read())
                                {
                                    DataRow _r = re.NewRow();
                                    _r["DOCTOR_APPOINTMENT_TIME_ID"] = reader["DOCTOR_APPOINTMENT_TIME_ID"].ToString();
                                    _r["DOCTOR_ID_IMPORT"] = reader["DOCTOR_ID_IMPORT"].ToString();
                                    _r["APPOINTMENT_DATE"] = reader["APPOINTMENT_DATE"].ToString();
                                    _r["APPOINTMENT_TIME"] = reader["APPOINTMENT_TIME"].ToString();
                                    _r["CUSTOMER_USER_MAIL"] = reader["CUSTOMER_USER_MAIL"].ToString();
                                    _r["CUSTOMER_FIRSTNAME"] = reader["CUSTOMER_FIRSTNAME"].ToString();
                                    _r["CUSTOMER_LASTNAME"] = reader["CUSTOMER_LASTNAME"].ToString();
                                    _r["TITLE_ID"] = reader["TITLE_ID"].ToString();
                                    _r["GENDER_ID"] = reader["GENDER_ID"].ToString();
                                    _r["CUSTOMER_BIRTHDAY"] = reader["CUSTOMER_BIRTHDAY"].ToString();
                                    _r["CUSTOMER_ADDR"] = reader["CUSTOMER_ADDR"].ToString();
                                    _r["CUSTOMER_POSTCODE"] = reader["CUSTOMER_POSTCODE"].ToString();
                                    _r["CUSTOMER_SUBURB"] = reader["CUSTOMER_SUBURB"].ToString();
                                    _r["STATE_ID"] = reader["STATE_ID"].ToString();
                                    _r["CUSTOMER_PHONE_NO"] = reader["CUSTOMER_PHONE_NO"].ToString();
                                    _r["MEDICAL_CARD_NO"] = reader["MEDICAL_CARD_NO"].ToString();
                                    re.Rows.Add(_r);
                                }
                            }
                        }
                    }
                }


                return re;
            }
            catch (Exception e)
            {
                log.Write("MYSQL:GetAppointmentRequests(): failed\n" + e.Message);
                return null;
            }

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="clinicID"></param>
        /// <param name="update_user"></param>
        public void ResetAllRequestFlag(string clinicID,string update_user)
        {
            try
            {
                string query = "SET SQL_SAFE_UPDATES = 0; UPDATE fd_rel_doctor_appointment_time a left JOIN `fd_rel_clinic_doctor` b ON a.`DOCTOR_ID` = b.`DOCTOR_ID` SET a.REQUESTING_FLAG = 0,a.REQUESTING_USER_ID = 0,a.UPDATE_USER="
                    + update_user
                    + ",a.UPDATE_DATE="
                    + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") 
                    + " WHERE b.`CLINIC_USER_ID` = " 
                    + clinicID;

                using (MySqlConnection connection = new MySqlConnection(PrepareConnectionString()))
                {
                    connection.Open();
                    using (MySqlCommand cmd = new MySqlCommand(query, connection))
                    {
                        cmd.ExecuteNonQuery();
                    }
                }

            }
            catch (Exception e)
            {
                log.Write("MYSQL:ResetAllRequestFlag(): failed\n" + e.Message);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <param name="update_user"></param>
        public void SetSuccessfulTo2(string id,string update_user)//2 means ocupied, failed
        {
            try
            {
                string query = "UPDATE fd_rel_doctor_appointment_time SET SUCCESSFUL_FLAG=2,UPDATE_USER="
                    + update_user
                    + ",UPDATE_DATE="
                    + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                    +" where DOCTOR_APPOINTMENT_TIME_ID=" + id;
                using (MySqlConnection connection = new MySqlConnection(PrepareConnectionString()))
                {
                    connection.Open();
                    using (MySqlCommand cmd = new MySqlCommand(query, connection))
                    {
                        cmd.ExecuteNonQuery();
                    }
                }

            }
            catch (Exception e)
            {
                log.Write("MYSQL:SetSuccessfulTo2(): failed\n" + e.Message);
            }

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <param name="update_user"></param>
        public void SetSuccessfulTo1(string id, string update_user)//1 means successful
        {
            try
            {
                string query = "UPDATE fd_rel_doctor_appointment_time SET SUCCESSFUL_FLAG=1,UPDATE_USER="
                    + update_user
                    + ",UPDATE_DATE="
                    + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                    + " where DOCTOR_APPOINTMENT_TIME_ID=" + id;
                using (MySqlConnection connection = new MySqlConnection(PrepareConnectionString()))
                {
                    connection.Open();
                    using (MySqlCommand cmd = new MySqlCommand(query, connection))
                    {
                        cmd.ExecuteNonQuery();
                    }
                }

            }
            catch (Exception e)
            {
                log.Write("MYSQL:SetSuccessfulTo1(): failed\n" + e.Message);
            }
        }

    }
}