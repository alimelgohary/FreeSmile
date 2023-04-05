using System.Text;
using System.Net.Mail;
using System.Net;
using Google.Apis.Gmail.v1;
using Google.Apis.Services;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Util.Store;

namespace FreeSmile.Services
{
    public class EmailService
    {
        private static string? FREESMILE_GMAIL_PASSWORD { get; } = Helper.GetEnvVariable("_FreeSmileGmailPass", false);
        private static string? FREESMILE_GMAIL { get; } = Helper.GetEnvVariable("_FreeSmileGmail", false);
        private static readonly string SmtpServer = "smtp.gmail.com";
        private static readonly int SmtpPort = 587;

        public static void SendEmail(string mailTo, string subject, string htmlFileName, string lang, bool gmailApi, params object[] formatParameters)
        {
            string body = string.Format(File.ReadAllText(@$"EmailTemplates\{lang}\{htmlFileName}"), formatParameters);
            if (gmailApi)
            {
                SendEmailGmail(mailTo, subject, body);
                return;
            }

            MailMessage message = new()
            {
                From = new MailAddress(FREESMILE_GMAIL, "Free Smile"),
                To = { new MailAddress(mailTo) },
                Subject = subject,
                Body = body,
                IsBodyHtml = true
            };

            SmtpClient client = new(SmtpServer, SmtpPort)
            {
                EnableSsl = true,
                Credentials = new NetworkCredential(FREESMILE_GMAIL, FREESMILE_GMAIL_PASSWORD)
            };
            client.Send(message);
        }

        private static void SendEmailGmail(string mailTo, string subject, string body)
        {
            string[] Scopes = { GmailService.Scope.GmailSend };
            UserCredential credential;
            using (var stream = new FileStream("tokens/client_secret.json", FileMode.Open, FileAccess.Read))
            {
                string credPath = "tokens";
                credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
                             GoogleClientSecrets.FromStream(stream).Secrets,
                              Scopes,
                              "user",
                              CancellationToken.None,
                              new FileDataStore(credPath, true)).Result;
                Console.WriteLine("Credential file saved to: " + credPath);
            }
            var service = new GmailService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = "free smile",
            });

            string message = $"From: \"Free Smile\" <freesmilemans@gmail.com>\r\n" +
                             $"Content-Type: text/html;charset=utf-8\r\n" +
                             $"To: {mailTo}\r\n" +
                             $"Subject: =?utf-8?B?{Base64UrlEncode(subject)}?=\r\n\r\n" +  // Set subject encoding
                             $"{body}";

            var newMsg = new Google.Apis.Gmail.v1.Data.Message();
            newMsg.Raw = Base64UrlEncode(message);
            var response = service.Users.Messages.Send(newMsg, "me").Execute();

            if (response == null)
                throw new Exception("Gmail Api response is null");

        }
        private static string Base64UrlEncode(string input)
        {
            var inputBytes = Encoding.UTF8.GetBytes(input);
            return Convert.ToBase64String(inputBytes)
              .Replace('+', '-')
              .Replace('/', '_')
              .Replace("=", "");
        }
        public static string ObscureEmail(string email) // johndoe2023@gmail.com
        {
            var i = email.IndexOf('@');
            // j*****3@gmail.com
            return new StringBuilder().Append(email.First()).Append("*****").Append(email[i - 1]).Append(email.Substring(i)).ToString();
        }
    }
}