using Origam.Sms;

namespace Origam.Twilio
{
    public class TwilioService : ISmsService
    {
        private string _from;
        private string _to;
        private string _body;
        
        public void SendSms(string from, string to, string text)
        {
            this._from = from;
            this._to = to;
            this._body = text;
        }
    }
}