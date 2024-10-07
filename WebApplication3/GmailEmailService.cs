using Google.Apis.Auth.OAuth2;
using Google.Apis.Gmail.v1;
using Google.Apis.Gmail.v1.Data;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using MimeKit;
using System.Security.Claims;

public class GmailEmailService
{
    private readonly string fromEmail; // 송신자 이메일
    private readonly string _clientId;
    private readonly string _clientSecret;

    private static readonly string[] Scopes = { GmailService.Scope.GmailSend };
    private static readonly string ApplicationName = "Gmail API .NET Quickstart";
    private GmailService gmailService;

    public GmailEmailService(IConfiguration configuration, ClaimsPrincipal user)
    {
        _clientId = configuration["Google:ClientId"];
        _clientSecret = configuration["Google:ClientSecret"];
        fromEmail = user.FindFirstValue(ClaimTypes.Email);
        AuthenticateGoogleUser().Wait();
    }

    private async Task AuthenticateGoogleUser()
    {
        UserCredential credential;

        // 자격 정보가 코드에 직접 정의된 OAuth2 인증
        credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(
            new ClientSecrets
            {
                ClientId = _clientId,
                ClientSecret = _clientSecret
            },
            Scopes,
            "user",
            CancellationToken.None,
            new FileDataStore("token.json", true) // 토큰 저장 경로
        );

        // Gmail API 서비스 생성
        gmailService = new GmailService(new BaseClientService.Initializer()
        {
            HttpClientInitializer = credential,
            ApplicationName = ApplicationName
        });
    }

    public async Task<bool> SendEmailAsync(string toEmail, string subject, string bodyHtml)
    {
        try
        {
            var message = CreateEmail(toEmail, fromEmail, subject, bodyHtml);
            await SendMessage(gmailService, "me", message);
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred: {ex.Message}");
            return false;
        }
    }

    // 이메일 생성 (MimeMessage 사용)
    private Message CreateEmail(string to, string from, string subject, string bodyHtml)
    {
        var emailMessage = new MimeMessage();
        emailMessage.From.Add(new MailboxAddress("", from));
        emailMessage.To.Add(new MailboxAddress("", to));
        emailMessage.Subject = subject;

        var bodyBuilder = new BodyBuilder { HtmlBody = bodyHtml };
        emailMessage.Body = bodyBuilder.ToMessageBody();

        using (var memoryStream = new MemoryStream())
        {
            emailMessage.WriteTo(memoryStream);
            var encodedEmail = Convert.ToBase64String(memoryStream.ToArray())
                .Replace('+', '-')
                .Replace('/', '_')
                .Replace("=", "");

            return new Message { Raw = encodedEmail };
        }
    }

    // 이메일 전송
    private async Task SendMessage(GmailService service, string userId, Message email)
    {
        try
        {
            await service.Users.Messages.Send(email, userId).ExecuteAsync();
            Console.WriteLine("Email sent successfully.");
        }
        catch (Exception e)
        {
            Console.WriteLine("An error occurred: " + e.Message);
        }
    }
}
