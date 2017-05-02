using System.ServiceModel;

namespace LogInterfaces
{
    public interface ICallbackService
    {
        [OperationContract(IsOneWay = true)]
        void SendCallbackMessage(string message);
    }
}
