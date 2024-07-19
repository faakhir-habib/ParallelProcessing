using MailKit.Net.Smtp;
using MimeKit;

namespace ParallelProcessing.Server
{
    public interface IEmailService
    {
        Task SendAlertAsync(string message);
    }

    public class EmailService : IEmailService
    {
        public async Task SendAlertAsync(string message)
        {
            var email = new MimeMessage();
            email.From.Add(new MailboxAddress("App Name", "your-email@example.com"));
            email.To.Add(new MailboxAddress("Admin", "admin@example.com"));
            email.Subject = "Job Alert";
            email.Body = new TextPart("plain") { Text = message };

            using var smtp = new SmtpClient();
            await smtp.ConnectAsync("smtp.example.com", 587, false);
            await smtp.AuthenticateAsync("your-email@example.com", "your-email-password");
            await smtp.SendAsync(email);
            await smtp.DisconnectAsync(true);
        }
    }
}
