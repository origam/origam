using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mail;
using Microsoft.SqlServer.Server;
using Origam.DA;
using Origam.Workbench.Services.CoreServices;

namespace Origam.Security.Common
{
    public class MailMan
    {
        private readonly string mailTemplateDirectoryPath;
        private readonly string mailQueueName;

        public MailMan(string mailTemplateDirectoryPath, string mailQueueName)
        {
            this.mailTemplateDirectoryPath = mailTemplateDirectoryPath;
            this.mailQueueName = mailQueueName;
        }

        public void SendMailByAWorkflow(MailMessage mail)
        {
            // send mail - by a workflow located at root package			
            QueryParameterCollection pms = new QueryParameterCollection();
            pms.Add(new QueryParameter("subject", mail.Subject));
            pms.Add(new QueryParameter("body", mail.Body));
            pms.Add(new QueryParameter("recipientEmail", mail.To.First().Address));
            pms.Add(new QueryParameter("senderEmail", mail.From.Address));
            if (!string.IsNullOrWhiteSpace(mail.From.DisplayName))
            {
                pms.Add(new QueryParameter("senderName", mail.From.DisplayName));
            }
            if (!string.IsNullOrWhiteSpace(mailQueueName))
            {
                pms.Add(new QueryParameter("MailWorkQueueName", mailQueueName));
            }
            WorkflowService.ExecuteWorkflow(new Guid("6e6d4e02-812a-4c95-afd1-eb2428802e2b"), pms, null);
        }

        internal MailMessage GenerateMail(string userEmail, string fromAddress,
            string templateFilename, string templateFromResources,
            string subjectFromConfig,
            string userLangIETF, List<KeyValuePair<string, string>> replacements)
        {
            MailMessage passwordRecoveryMail = new MailMessage(fromAddress,
                userEmail);
            string templateContent =
                (string.IsNullOrEmpty(templateFilename)) 
                    ? templateFromResources
                    : GetLocalizedMailTemplateText(templateFilename,
                        userLangIETF);

            string[] subjectAndBody = processMailTemplate(templateContent,
                replacements);
            passwordRecoveryMail.Subject = subjectAndBody[0];
            passwordRecoveryMail.Body = subjectAndBody[1];

            if (string.IsNullOrWhiteSpace(passwordRecoveryMail.Subject))
            {
                passwordRecoveryMail.Subject = subjectFromConfig;
            }
            return passwordRecoveryMail;
        }
          
        private string GetLocalizedMailTemplateText(
            string templateFilename,
            string languageIETF = "")
        {
            string filePath = Path.Combine(
                mailTemplateDirectoryPath, //AppDomain.CurrentDomain.SetupInformation.ApplicationBase,
                templateFilename);
            return File.ReadAllText(
                FindBestLocalizedFile(filePath, languageIETF)).Trim();     
        }
          
          
        public static string FindBestLocalizedFile(string filePath,
            string languageIETF)
        {
            if (String.IsNullOrEmpty(languageIETF))
            {
                // language not sent, use current thread one
                languageIETF = System.Threading.Thread.CurrentThread.
                    CurrentUICulture.IetfLanguageTag;
            }

            // find the last '.'
            int lastDotIndex = filePath.LastIndexOf('.');
            // create a localized file candidate ( password_reset.de-DE.txt )
            /*
                password_reset.txt -> password_reset.de-DE.txt
                password_reset -> password_reset.de-DE
             */
            string candidate;
            if (lastDotIndex == -1)
            {
                // dot not found
                candidate = String.Format("{0}.{1}", filePath, languageIETF);
            }
            else
            {
                candidate = String.Format("{0}.{1}{2}", filePath.Substring(0,
                        lastDotIndex), languageIETF,
                    filePath.Substring(lastDotIndex));
            }
            if (File.Exists(candidate)) return candidate;
            // try better
            /*
                password_reset.txt -> password_reset.de.txt
                password_reset -> password_reset.de
             */
            string[] splittedIETF = languageIETF.Split('-');
            if (splittedIETF.Length == 2)
            {
                if (lastDotIndex == -1)
                {
                    candidate = String.Format("{0}.{1}", filePath,
                        splittedIETF[0]);
                }
                else
                {
                    candidate = String.Format("{0}.{1}{2}", filePath.Substring(
                            0, lastDotIndex), splittedIETF[0],
                        filePath.Substring(lastDotIndex));
                }
                if (File.Exists(candidate)) return candidate;
            }
            // fallback
            return filePath;            
        }
           
        private static string[] processMailTemplate(string templateContent,
            List<KeyValuePair<string, string>> replacements)
        {
            string subject = null;
            if (templateContent.ToLower().StartsWith("subject:"))
            {
                subject = templateContent.Substring(8,
                    templateContent.IndexOf('\n') - 8).Trim();
                foreach (KeyValuePair<string, string> replacement
                    in replacements)
                {
                    subject = subject.Replace(replacement.Key, replacement.Value);
                }
                templateContent = templateContent.Substring(
                    templateContent.IndexOf('\n')).TrimStart();
            }
            foreach (KeyValuePair<string, string> replacement in replacements)
            {
                templateContent = templateContent.Replace(replacement.Key,
                    replacement.Value);
            }
            return new string[] { subject, templateContent };
        }
    }
}