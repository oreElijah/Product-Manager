using System.Net;
using System.Text;
using System.Text.Json;
using api.Data;
using api.Exceptions;
using api.Interfaces;
using api.models;
using System.Web;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace api.Services
{
    public class EmailService : IEmailService
    {
        private const string BrevoUrl = "https://api.brevo.com/v3/smtp/email";

        private readonly ApplicationDBContext _context;
        private readonly IConfiguration _configuration;
        private readonly IWebHostEnvironment _environment;
        private readonly UserManager<AppUser> _userManager;
        private readonly ILogger<EmailService> _logger;

        public EmailService(
            ApplicationDBContext context,
            IConfiguration configuration,
            IWebHostEnvironment environment,
            UserManager<AppUser> userManager,
            ILogger<EmailService> logger)
        {
            _context = context;
            _configuration = configuration;
            _environment = environment;
            _userManager = userManager;
            _logger = logger;
        }

        public async Task SendVerifyUserEmail(string firstName, string email, string verify_token)
        {
            var apiKey = _configuration["BREVO_API_KEY"];
            var fromEmail = _configuration["Email:From"];
            var fromName = _configuration["Email:FromName"] ?? "Project Management API";
            var baseUrl = _configuration["App:BaseUrl"] ?? "https://localhost:5135";

            if (string.IsNullOrWhiteSpace(apiKey) || string.IsNullOrWhiteSpace(fromEmail))
            {
                _logger.LogError("Email settings are missing. Configure BREVO_API_KEY and Email:From.");
                return;
            }

            var templatePath = Path.Combine(_environment.ContentRootPath, "Template", "base.html");
            var html = await File.ReadAllTextAsync(templatePath);
            html = html.Replace("{{name}}", firstName);
            
            var verifyLink = $"{baseUrl}/api/account/v1/verify_email?" +
                    $"email={HttpUtility.UrlEncode(email)}&" +
                    $"token={HttpUtility.UrlEncode(verify_token)}";

            html = html.Replace("{{verification_link}}", verifyLink);

            var payload = new
            {
                sender = new { name = fromName, email = fromEmail },
                to = new[] { new { email, name = firstName } },
                subject = "Verify Your Email",
                htmlContent = html
            };

            try
            {
                using var http = new HttpClient();
                using var request = new HttpRequestMessage(HttpMethod.Post, BrevoUrl)
                {
                    Content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json")
                };

                request.Headers.Add("accept", "application/json");
                request.Headers.Add("api-key", apiKey);

                using var response = await http.SendAsync(request);
                if (!response.IsSuccessStatusCode)
                {
                    var body = await response.Content.ReadAsStringAsync();
                    _logger.LogError($"Failed to send email (Brevo {(int)response.StatusCode}): {body}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send email due to an exception.");
            }
        }

        public async Task VerifyEmail(string email, string token)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null)
            {
                throw new NotFoundException("User with email not found");
            }

            var result = await _userManager.ConfirmEmailAsync(user, token);
            if (!result.Succeeded)
            {
                throw new UnauthorizedAccessException("Invalid or expired verification token.");
            }
        }
    }
}