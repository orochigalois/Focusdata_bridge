using LogInterfaces;

namespace FocusDataBridge
{
    public class TestCallback : ICallbackService
    {
        public void SendCallbackMessage(string message)
        {
            //MessageBox.Show(message);
        }
    }
}
