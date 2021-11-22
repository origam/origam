using Origam.Twilio;


namespace Origam.Sms
{
    public class SmsServiceFactory
    {
        public static ISmsService GetService()
        {
            return new TwilioService();
        }
    }
}