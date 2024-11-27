using MimeKit;
using MailKit.Net.Smtp;
using Microsoft.Extensions.Configuration;

namespace Application.Services
{
    public class EmailService
    {
        private readonly IConfiguration _configuration;
        public EmailService(IConfiguration configuration)
        {
            this._configuration = configuration;
        }

        public async Task SendEmailOtp(string recipientEmail, string code, string receiverName)
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(_configuration["EmailSettings:SenderName"], _configuration["EmailSettings:SenderEmail"]));
            message.To.Add(new MailboxAddress(receiverName, recipientEmail));
            message.Subject = $"Hello {receiverName}";

            var bodyBuilder = new BodyBuilder
            {
                HtmlBody = $"your code is {code}"
            };

            message.Body = bodyBuilder.ToMessageBody();

            using (var client = new SmtpClient())
            {
                client.Connect(_configuration["EmailSettings:SmtpServer"], int.Parse(_configuration["EmailSettings:SmtpPort"]), false);
                client.Authenticate(_configuration["EmailSettings:Username"], _configuration["EmailSettings:Password"]);

                await client.SendAsync(message);
                client.Disconnect(true);
            }
        }
    }
}
