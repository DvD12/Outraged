using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Mail;
using System.Threading.Tasks;
using UnityEngine;

namespace Outraged
{
    public static class DataEmail
    {
        public static string User;
        public static string Pass;

        public static void LoadJson()
        {
            var data = Resources.Load("Data/Email");
            if (data != null)
            {
                var result = new { User = String.Empty, Pass = String.Empty };
                var obj = JsonConvert.DeserializeAnonymousType(data.ToString(), result, DataHeaders.GetFormatting());
                User = obj.User;
                Pass = obj.Pass;
            }
        }

        public enum SendMailOutcome
        {
            Generic,
            OK
        }
        public static async Task<SendMailOutcome> SendMailAsync(string subject, string body, params Attachment[] attachments) => await System.Threading.Tasks.Task.Run(() => SendMail(subject, body, attachments));
        public static SendMailOutcome SendMail(string subject, string body, params Attachment[] attachments)
        {
            if (!User.IsValid() || !Pass.IsValid()) { return SendMailOutcome.Generic; }
            MailMessage mail = new MailMessage();
            SmtpClient SmtpServer = new SmtpClient("smtp.gmail.com");

            mail.From = new MailAddress(User + "@gmail.com");
            mail.To.Add(User + "@gmail.com");
            mail.Subject = subject;
            mail.Body = body;
            foreach (var a in attachments)
            {
                mail.Attachments.Add(a);
            }

            SmtpServer.Port = 587;
            SmtpServer.Credentials = new System.Net.NetworkCredential(User, Pass);
            SmtpServer.EnableSsl = true;

            try
            {
                SmtpServer.Send(mail);
                return SendMailOutcome.OK;
            }
            catch (Exception e)
            {
                return SendMailOutcome.Generic;
            }
        }
    }
}
