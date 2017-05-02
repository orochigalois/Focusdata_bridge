using System.ServiceModel;
using System.Windows.Forms;
using LogInterfaces;

namespace SystemTray
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerSession)]
    public class TestService : ITestService
    {
        public void Connect()
        {
            Callback = OperationContext.Current.GetCallbackChannel<ICallbackService>();
        }

        public static ICallbackService Callback { get; set; }


        public void SendMessage(string message)
        {
            FocusdataSystemTray.logForm.SetLog(message);
        }
    }
}
