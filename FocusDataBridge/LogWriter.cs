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
#if DEBUG
#else
        static ITestService WCFservice;
#endif
        public LogWriter()
        {
#if DEBUG
#else
            var callback = new TestCallback();
            var context = new InstanceContext(callback);
            var pipeFactory =
                 new DuplexChannelFactory<ITestService>(context,
                 new NetNamedPipeBinding(),
                 new EndpointAddress("net.pipe://localhost/Test"));

            WCFservice = pipeFactory.CreateChannel();

            WCFservice.Connect();
#endif
        }

        public static void LogWrite(string logMessage)
        {

            

            string m_exePath = string.Empty;
            m_exePath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            try
            {
                using (StreamWriter txtWriter = File.AppendText(m_exePath + "\\" + "log.txt"))
                {

                    try
                    {
                        txtWriter.Write("\r\nLog Entry : ");
                        txtWriter.WriteLine("{0} {1}", DateTime.Now.ToLongTimeString(),
                            DateTime.Now.ToLongDateString());
                        txtWriter.WriteLine("  :");
                        txtWriter.WriteLine("  :{0}", logMessage);
                        txtWriter.WriteLine("-------------------------------");

                        StringBuilder builder = new StringBuilder();
                        builder.Append("\r\nLog Entry : ");
                        builder.Append(DateTime.Now.ToLongTimeString());
                        builder.Append(" ");
                        builder.Append(DateTime.Now.ToLongDateString());
                        builder.AppendLine("  :");
                        builder.AppendLine(logMessage);
                        builder.AppendLine("-------------------------------");



#if DEBUG
                        Console.WriteLine(builder.ToString());
#else
                        WCFservice.SendMessage(builder.ToString());
#endif
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }


                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
}
