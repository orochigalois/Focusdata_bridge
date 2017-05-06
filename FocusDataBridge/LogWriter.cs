using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;


using System.ServiceModel;
using LogInterfaces;

namespace FocusDataBridge
{
    public class LogWriter
    {
        private int connection;

        ITestService WCFservice;
        DuplexChannelFactory<ITestService> pipeFactory;
        public LogWriter()
        {
            try
            {
                connection = Constant.CLOSED;

                var callback = new TestCallback();
                var context = new InstanceContext(callback);
                pipeFactory =
                     new DuplexChannelFactory<ITestService>(context,
                     new NetNamedPipeBinding(),
                     new EndpointAddress("net.pipe://localhost/Test"));
    
                
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
 
        }
        public int getConnectionState()
        {
            return connection;
        }

        public bool OpenConnection()
        {
            try
            {
                WCFservice = pipeFactory.CreateChannel();
                WCFservice.SendMessage("LogWriter start working");
                connection = Constant.OPEN;
                Console.WriteLine("Connect to LogWriter successfully");
                return true;
            }
            catch (Exception e)
            {
                connection = Constant.CLOSED;
                Console.WriteLine("LogWriter:OpenConnection():Cannot connect to LogWriter.\n" + e.Message);
                return false;
            }
        }

        public void Write(string message)
        {
            string m_exePath = string.Empty;
            m_exePath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            try
            {
                using (StreamWriter txtWriter = File.AppendText(m_exePath + "\\" + "log.txt"))
                {

                    try
                    {
                        StringBuilder builder = new StringBuilder();
                        builder.Append("\r\nLog Entry : ");
                        builder.Append(DateTime.Now.ToLongTimeString());
                        builder.Append(" ");
                        builder.Append(DateTime.Now.ToLongDateString());
                        builder.AppendLine("  :");
                        builder.AppendLine(message);
                        builder.AppendLine("-------------------------------");

                        txtWriter.WriteLine(builder.ToString());

                        Console.WriteLine(builder.ToString());

                        if(connection  ==  Constant.OPEN)
                            WCFservice.SendMessage(builder.ToString());

                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                        connection = Constant.CLOSED;
                    }


                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                connection = Constant.CLOSED;
            }
        }
    }
}
