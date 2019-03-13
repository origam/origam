namespace Origam.ServerCore.Configuration
{
    public class AccountConfig
    {
        public string FromAddress { get; set; }
        public string NewUserRoleId { get; set; }
        public string ResetPasswordMailSubject { get; set; }
        public string ResetPasswordMailBodyFileName { get; set; }
        public string UserUnlockNotificationSubject { get; set; }
        public string UserUnlockNotificationBodyFileName { get; set; }
        public string userRegistrationMailSubject { get; set; }
        public string userRegistrationMailBodyFileName { get; set; }
        public string MailQueueName { get; set; }
    }
}