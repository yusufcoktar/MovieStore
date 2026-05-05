using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Configuration;

namespace DigitalMovieStore.Service.Email
{
    // Arayüzümüz (Interface)
    public interface IEmailService
    {
        Task SendEmailAsync(string toEmail, string subject, string body);
    }

    // Gerçek Sınıfımız
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _config;

        public EmailService(IConfiguration config)
        {
            _config = config;
        }

        public async Task SendEmailAsync(string toEmail, string subject, string body)
        {
            var emailSettings = _config.GetSection("EmailSettings");
            var senderEmail = emailSettings["SenderEmail"];
            var senderPassword = emailSettings["SenderPassword"];
            var senderName = emailSettings["SenderName"];

            // Postacıyı (SmtpClient) hazırlıyoruz
            // Null gelme ihtimaline karşı SmtpPort'un sonuna bir ünlem (!) ekliyoruz ki derleyici uyarı vermesin

            var client = new SmtpClient(emailSettings["SmtpServer"], int.Parse(emailSettings["SmtpPort"]!))
            {
                EnableSsl = true,
                UseDefaultCredentials = false, // 🔥 BU SATIRI EKLE (Mutlaka Credentials'dan ÖNCE olmalı)
                Credentials = new NetworkCredential(senderEmail, senderPassword)
            };
            // Mektubu hazırlıyoruz
            var mailMessage = new MailMessage
            {
                From = new MailAddress(senderEmail, senderName),
                Subject = subject,
                Body = body,
                IsBodyHtml = true // Kod gönderirken HTML ile şık tasarımlar yapabilmek için true yaptık
            };

            mailMessage.To.Add(toEmail);

            // Mektubu yolluyoruz
            await client.SendMailAsync(mailMessage);
        }
    }
}
