using System.Net;
using System.Net.Mail;
using Webserver.Services;

namespace E_Shop.Services
{
    public class EmailService
    {
        private readonly SmtpClient smtpClient;

        public EmailService(IConfigurationProvider configurationProvider)
        {
            var host = configurationProvider.GetSetting("SmtpHost");
            var username = configurationProvider.GetSetting("SmtpUsername");
            var password = configurationProvider.GetSetting("SmtpPassword");

            smtpClient = new SmtpClient(host)
            {
                Port = 587,
                Credentials = new NetworkCredential(username, password),
                EnableSsl = true
            };
        }

        public void Send(MailMessage message)
        {
            try
            {
                smtpClient.Send(message);
            }
            catch { }
        }
    }
}
