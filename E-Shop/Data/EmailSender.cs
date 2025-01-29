using System.Net;
using System.Net.Mail;

namespace E_Shop.Data
{
    internal class EmailSender
    {
        private static SmtpClient _smtpClient;

        public static void Initialize(string host, string username, string password)
        {
            _smtpClient = new SmtpClient(host)
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
                _smtpClient.Send(message);
            }
            catch {}
        }
    }
}
