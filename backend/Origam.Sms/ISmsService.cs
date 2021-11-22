namespace Origam.Sms
{
    public interface ISmsService
    {
        public void SendSms(string from, string to, string text);
    }
}