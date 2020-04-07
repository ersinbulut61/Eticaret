using Eticaret.Enums;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.ServiceModel.Channels;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace Eticaret.Services.Senders
{
    public class EmailService : IMessageService
    {
        private string _userId = HttpContext.Current.User.Identity.GetUserId();
        public string[] Cc { get; set; } //Bilgi Alanı
        public string[] Bcc { get; set; } //Gizli Bilgi Alanı
        public string FilePath { get; set; } //Dosya gönderilecekse
        public MessageStates MessageState { get; private set; }
        public string SenderMail { get; set; } //Kim Gönderecek
        public string Password { get; set; } //Onun Şifresi
        public string Smtp { get; set; } //Giden Posta Sunucusu
        public int SmtpPort { get; set; }

        public MessageState messageState => throw new NotImplementedException();

        public EmailService()
        {
            this.SenderMail = "eticaretprojem.123@gmail.com";
            this.Password = "eticaret.123";
            this.Smtp = "smtp.gmail.com";
            this.SmtpPort = 587;
        }
        public EmailService(string senderMail, string password, string smtp, int smtpport)
        {
            this.SenderMail = senderMail;
            this.Password = password;
            this.Smtp = smtp;
            this.SmtpPort = smtpport;
        }

        public async Task SendAsync(IdentityMessage message, params string[] concacts)
        {
            var userID = _userId ?? "system";
            try
            {
                var mail = new MailMessage { From = new MailAddress(this.SenderMail) };
                if (!string.IsNullOrEmpty(FilePath))
                {
                    mail.Attachments.Add(new Attachment(FilePath));
                }
                foreach (var c in concacts)
                {
                    mail.To.Add(c);
                }
                if (Cc != null && Cc.Length > 0)
                {
                    foreach (var cc in Cc)
                    {
                        mail.CC.Add(new MailAddress(cc));
                    }
                }
                if (Bcc != null && Bcc.Length > 0)
                {
                    foreach (var bcc in Bcc)
                    {
                        mail.Bcc.Add(new MailAddress(bcc));
                    }
                }
                mail.Subject = message.Subject; //Mailin konusu
                mail.Body = message.Body; //Mailin içeriği

                mail.IsBodyHtml = true;//mail gönderirken html taglarının render edilmesini sağlar
                //Türkçe karakter sorununu önler
                mail.BodyEncoding = Encoding.UTF8;
                mail.SubjectEncoding = Encoding.UTF8;
                mail.HeadersEncoding = Encoding.UTF8;

                var smptClient = new SmtpClient(this.Smtp, this.SmtpPort)
                {
                    Credentials = new NetworkCredential(this.SenderMail, this.Password),
                    EnableSsl = true //güvenli bağlantı
                };
                await smptClient.SendMailAsync(mail);
                MessageState = MessageStates.Delivered;
            }
            catch (Exception)
            {
                MessageState = MessageStates.NotDelivered;

            }
        }

        public void Send(IdentityMessage message, params string[] concacts)
        {
            Task.Run(async () =>
            {
                await this.SendAsync(message, concacts);
            });
        }
    }
}