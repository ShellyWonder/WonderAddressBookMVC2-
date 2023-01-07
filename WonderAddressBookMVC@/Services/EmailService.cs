using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Options;
using MimeKit;
using WonderAddressBookMVC_.Models;
using MailKit;
using MailKit.Net.Smtp;
using MailKit.Security;

namespace WonderAddressBookMVC_.Services
{
    public class EmailService : IEmailSender
    {
        //inject 
        private readonly MailSettings _mailSettings;
        #region Constructor
        public EmailService(IOptions<MailSettings> mailSettings)
        {
            _mailSettings = mailSettings.Value;
        }
        #endregion
        public async Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            //create sender:
            //If local is null look in the Environment
            var emailSender = _mailSettings.Email ?? Environment.GetEnvironmentVariable("Email");
            MimeMessage newEmail = new();

            newEmail.Sender = MailboxAddress.Parse(emailSender);
            //"To" emails--supports one and group emails
            //splits email string (list) by ";"
            foreach (var emailAddress in email.Split(";"))
            {
                newEmail.To.Add(MailboxAddress.Parse(emailAddress));
            }

            newEmail.Subject = subject;

            //Add msg to email:
            //creates body(text) of email
            BodyBuilder emailBody = new();
            emailBody.HtmlBody = htmlMessage;

            newEmail.Body = emailBody.ToMessageBody();

            //logging into/out of smtp client:
            using SmtpClient smtpClient = new();

            try
            {   //may need to change environment variables to accomodate Railway hosting
                //injected from user secrets
                var host = _mailSettings.Host ?? Environment.GetEnvironmentVariable("Host");
                //port must be int not string-- hence, int.parse is used
                var port = _mailSettings.Port !=0 ? _mailSettings.Port: int.Parse (Environment.GetEnvironmentVariable("Port")!);
                var password = _mailSettings.Password ?? Environment.GetEnvironmentVariable("Password");

                //sending email with SecureSocketOptions (encryption):
                await smtpClient.ConnectAsync(host, port, SecureSocketOptions.StartTls);
                await smtpClient.AuthenticateAsync(emailSender, password);

                await smtpClient.SendAsync(newEmail);
                await smtpClient.DisconnectAsync(true);

            }
            catch (Exception ex)
            {
                var error = ex.Message;
                throw;
            }
        }
    }
}
