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
        private CancellationTokenSource cts = new CancellationTokenSource();
        private Task mainTask = null;


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

            mainTask = new Task(Poll, cts.Token, TaskCreationOptions.LongRunning);
            mainTask.Start();

            
        }

        private void Poll()
        {
            CancellationToken cancellation = cts.Token;

            TimeSpan interval = TimeSpan.FromSeconds(5);


            while (!cancellation.WaitHandle.WaitOne(interval))
            {
                try
                {

                    string last = "", first = "";
                    MysqlConnect mysqlConnect;
                    SqlDataReader rdr = null;
        

                    string DATABASE_HOST = ConfigurationManager.AppSettings["DATABASE_HOST"];
                    string DATABASE_NAME = ConfigurationManager.AppSettings["DATABASE_NAME"];
                    string DATABASE_USER = ConfigurationManager.AppSettings["DATABASE_USER"];
                    string DATABASE_PASS = ConfigurationManager.AppSettings["DATABASE_PASS"];

                    SqlConnection con = new SqlConnection("Data Source=DBServer;Password="+ DATABASE_PASS + ";Persist Security Info=True;User ID="+ DATABASE_USER + ";Initial Catalog="+ DATABASE_NAME + ";Data Source="+ DATABASE_HOST);
                    try
                    {
                        con.Open();
                        LogWriter.LogWrite("Connect to local database successfully");

                        SqlCommand cmd = new SqlCommand(
                        "BP_GetAllUsers", con);
                        cmd.CommandType = CommandType.StoredProcedure;

                        try
                        {
                            rdr = cmd.ExecuteReader();
                            while (rdr.Read())
                            {
                    
                                last = (string)rdr["SURNAME"];
                                first = (string)rdr["FIRSTNAME"];
                            }
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e.Message);
                            LogWriter.LogWrite("Read local table failed.\n" + e.Message);
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.Message);
                        LogWriter.LogWrite("Local database connecting failed.\n"+ e.Message);
                    }


                    mysqlConnect = new MysqlConnect();
                    mysqlConnect.InsertDoctor(first + last);




                    if (cancellation.IsCancellationRequested)
                    {
                        break;
                    }
                    interval = TimeSpan.FromSeconds(5);
                }
                catch (Exception ex)
                {
                    // Log the exception.

                    eventLog1.WriteEntry("Bridge encountered an error '" +
                    ex.Message + "'", EventLogEntryType.Error);

                    interval = TimeSpan.FromSeconds(5);
                }
            }
        }

        protected override void OnStop()
        {
            //System.IO.File.Create(AppDomain.CurrentDomain.BaseDirectory + "OnStopAlex.txt");

            cts.Cancel();
            mainTask.Wait();
        }


    }
}
