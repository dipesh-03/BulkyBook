using MailKit.Net.Smtp;
using Microsoft.AspNetCore.Identity.UI.Services;
using MimeKit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BulkyBook.Utility
{
    public class EmailSender : IEmailSender
    {
        public Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
                       var emailToSend = new MimeMessage();
            emailToSend.From.Add(MailboxAddress.Parse("dipeshkumar.ce@gmail.com"));
            emailToSend.To.Add(MailboxAddress.Parse(email));
            emailToSend.Subject = subject;
            emailToSend.Body = new TextPart(MimeKit.Text.TextFormat.Html) { Text = htmlMessage };

            using(var emailCliennt = new SmtpClient())
            {
                emailCliennt.Connect("smtp.gmail.com", 587, MailKit.Security.SecureSocketOptions.StartTls);
                emailCliennt.Authenticate("dipeshkumar.ce@gmail.com", "ixnoueqvuemqgdkz");
                emailCliennt.Send(emailToSend);
                emailCliennt.Disconnect(true);
            }

            return Task.CompletedTask;
        }
    }
}
