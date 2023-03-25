using FreeSmile;
using FreeSmile.Models;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Gmail.v1;
using Google.Apis.Gmail.v1.Data;
using Google.Apis.Services;
using Google.Apis.Util.Store;


public class EmailServices
{
    public static bool SendEmail(EmailTemplate data)
    {
        try
        {
            string[] Scopes = { GmailService.Scope.GmailSend };
            UserCredential credential;
            using (var stream = new FileStream("client_secret.json", FileMode.Open, FileAccess.Read))
            {
                string credPath = "token_Send.json";
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
            
            string message = $"To: {data.To}\r\nSubject: {data.Subject}\r\nContent-Type: text/html;charset=utf-8\r\n\r\n{data.Body}";
            var newMsg = new Google.Apis.Gmail.v1.Data.Message();
            newMsg.Raw = Base64UrlEncode(message.ToString());
            Google.Apis.Gmail.v1.Data.Message response = service.Users.Messages.Send(newMsg, "me").Execute();

            if (response != null)
                return true;
            else
                return false;
        }
        catch (Exception e)
        {
            // log error
            return false;
        }

    }
    private static string Base64UrlEncode(string input)
    {
        var inputBytes = System.Text.Encoding.UTF8.GetBytes(input);
        return Convert.ToBase64String(inputBytes)
          .Replace('+', '-')
          .Replace('/', '_')
          .Replace("=", "");
    }
}
