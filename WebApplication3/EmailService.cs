/*using MailKit.Net.Smtp;
using MimeKit;
using Microsoft.Identity.Client;
using System;
using System.Threading.Tasks;
using MailKit.Security;
using System.Security.Claims;
using Azure.Identity;
using Microsoft.Graph.Models;
using Microsoft.Graph;

public class EmailService
{
    private readonly string fromEmail; // 송신자 이메일
    private readonly string _tenantId;
    private readonly string _clientId;
    private readonly string _clientSecret;
    private readonly string[] _scopes = new string[] { "https://outlook.office365.com/.default" }; // 권한 범위

    public EmailService(IConfiguration configuration, ClaimsPrincipal user)
    {
        _tenantId = configuration["AzureAd:TenantId"];
        _clientId = configuration["AzureAd:ClientId"];
        _clientSecret = configuration["AzureAd:ClientSecret"];
        fromEmail = user.FindFirstValue(ClaimTypes.Email);
    }

    public async Task<bool> SendEmailAsync(string toEmail, string subject, string content)
    {
        try
        {
            ClientSecretCredential credential = new(_tenantId, _clientId, _clientSecret);
            GraphServiceClient graphClient = new(credential);

            var body = new Microsoft.Graph.Users.Item.SendMail.SendMailPostRequestBody
            {
                Message = new Message
                {
                    Subject = subject,
                    Body = new ItemBody
                    {
                        ContentType = BodyType.Html,
                        Content = content
                    },
                    ToRecipients = new List<Recipient>()
                    {
                        new Recipient
                        {
                            EmailAddress = new EmailAddress
                            {
                                Address = toEmail
                            }
                        }
                    }
                }
            };
            await graphClient.Users[fromEmail].SendMail.PostAsync(body);
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred: {ex.Message}");
            return false;
        }
    }
}
*/