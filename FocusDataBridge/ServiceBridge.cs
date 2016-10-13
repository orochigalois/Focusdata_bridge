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

namespace FocusDataBridge
{
    public partial class ServiceBridge : ServiceBase
    {
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
            string last="", first="";
            DBConnect dbConnect;
            SqlDataReader rdr = null;
            System.IO.File.Create(AppDomain.CurrentDomain.BaseDirectory + "OnStart.txt");
            SqlConnection con = new SqlConnection("Data Source=DBServer;Password=Miaoyaliu1213;Persist Security Info=True;User ID=sa;Initial Catalog=BPSPatients;Data Source=CND98GRZ52\\SQLEXPRESS");
            try
            {
                con.Open();
      

                SqlCommand cmd = new SqlCommand(
                "BP_GetAllUsers", con);
                cmd.CommandType = CommandType.StoredProcedure;

                try
                {
                    rdr=cmd.ExecuteReader();
                    while (rdr.Read())
                    {
                        // get the results of each column
                        last = (string)rdr["SURNAME"];
                        first = (string)rdr["FIRSTNAME"];
                        string email = (string)rdr["EMAIL"];

                        // print out the results


                        using (StreamWriter writer = new StreamWriter("c:\\OnStart.txt", true))
                        {
                            writer.WriteLine(first + ";" + last + ";" + email + "\n");
                            writer.Close();
                        }

                    }
                }
                catch(Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }
            catch(Exception e)
            {
                Console.WriteLine(e.Message);
            }


            dbConnect = new DBConnect();
            dbConnect.Insert(first+last);
        }

        protected override void OnStop()
        {
            System.IO.File.Create(AppDomain.CurrentDomain.BaseDirectory + "OnStopAlex.txt");
        }
    }
}
