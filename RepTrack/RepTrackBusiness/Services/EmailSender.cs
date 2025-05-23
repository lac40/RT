using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MimeKit;
using MailKit.Net.Smtp;
using MailKit.Security;
using System;
using System.Threading.Tasks;

namespace RepTrackBusiness.Services
{
    /// <summary>
    /// Email sender implementation for ASP.NET Identity
    /// This class implements the IEmailSender interface that Identity uses for all email operations
    /// </summary>
    public class EmailSender : IEmailSender
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<EmailSender> _logger;

        // SMTP configuration values loaded from appsettings.json
        private readonly string _smtpServer;
        private readonly int _smtpPort;
        private readonly string _smtpUsername;
        private readonly string _smtpPassword;
        private readonly string _senderEmail;
        private readonly string _senderName;
        private readonly bool _enableSsl;

        public EmailSender(IConfiguration configuration, ILogger<EmailSender> logger)
        {
            _configuration = configuration;
            _logger = logger;

            // Load email settings from configuration
            // These values come from the "EmailSettings" section in appsettings.json
            _smtpServer = _configuration["EmailSettings:SmtpServer"] ?? "smtp.gmail.com";
            _smtpPort = int.Parse(_configuration["EmailSettings:SmtpPort"] ?? "587");
            _smtpUsername = _configuration["EmailSettings:SmtpUsername"] ?? "";
            _smtpPassword = _configuration["EmailSettings:SmtpPassword"] ?? "";
            _senderEmail = _configuration["EmailSettings:SenderEmail"] ?? _smtpUsername;
            _senderName = _configuration["EmailSettings:SenderName"] ?? "RepTrack";
            _enableSsl = bool.Parse(_configuration["EmailSettings:EnableSsl"] ?? "true");

            // Log a warning if email settings are not properly configured
            if (string.IsNullOrEmpty(_smtpUsername) || string.IsNullOrEmpty(_smtpPassword))
            {
                _logger.LogWarning("Email settings are not properly configured. Please update EmailSettings in appsettings.json");
            }
        }

        /// <summary>
        /// Sends an email asynchronously using the configured SMTP settings
        /// This method is called by ASP.NET Identity for all email operations
        /// </summary>
        /// <param name="email">The recipient's email address</param>
        /// <param name="subject">The email subject line</param>
        /// <param name="htmlMessage">The email body in HTML format</param>
        public async Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            try
            {
                // Create a new email message using MimeKit
                var message = new MimeMessage();

                // Set the sender - this appears in the "From" field
                message.From.Add(new MailboxAddress(_senderName, _senderEmail));

                // Set the recipient - this is who receives the email
                message.To.Add(new MailboxAddress(email, email));

                // Set the subject line
                message.Subject = subject;

                // Create the message body with both HTML and text versions
                var bodyBuilder = new BodyBuilder
                {
                    // The HTML version is what most email clients will display
                    HtmlBody = WrapInTemplate(subject, htmlMessage),

                    // The text version is a fallback for email clients that don't support HTML
                    TextBody = StripHtml(htmlMessage)
                };

                message.Body = bodyBuilder.ToMessageBody();

                // Send the email using MailKit's SMTP client
                using var client = new SmtpClient();

                // Configure the client to accept all SSL certificates (for development)
                // In production, you might want to validate certificates properly
                client.ServerCertificateValidationCallback = (s, c, h, e) => true;

                // Connect to the SMTP server
                // For Gmail, this uses STARTTLS on port 587
                await client.ConnectAsync(_smtpServer, _smtpPort, _enableSsl ? SecureSocketOptions.StartTls : SecureSocketOptions.None);

                // Authenticate with the SMTP server
                // For Gmail, this requires an App Password, not your regular password
                await client.AuthenticateAsync(_smtpUsername, _smtpPassword);

                // Send the email
                await client.SendAsync(message);

                // Disconnect from the server
                await client.DisconnectAsync(true);

                _logger.LogInformation($"Email sent successfully to {email} with subject: {subject}");
            }
            catch (Exception ex)
            {
                // Log the error but don't throw it further
                // This prevents email failures from breaking the user experience
                _logger.LogError(ex, $"Failed to send email to {email}. Subject: {subject}");

                // In development, you might want to throw this exception to see errors
                // In production, it's often better to fail silently for emails
                if (_configuration["Environment"] == "Development")
                {
                    throw;
                }
            }
        }

        /// <summary>
        /// Wraps the email content in a professional HTML template
        /// This gives all Identity emails a consistent, branded appearance
        /// </summary>
        private string WrapInTemplate(string subject, string content)
        {
            return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8' />
    <meta name='viewport' content='width=device-width, initial-scale=1.0' />
    <title>{subject}</title>
    <style>
        /* Email-safe CSS styles */
        body {{
            font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, 'Helvetica Neue', Arial, sans-serif;
            line-height: 1.6;
            color: #333;
            max-width: 600px;
            margin: 0 auto;
            padding: 0;
            background-color: #f4f4f4;
        }}
        .container {{
            background-color: #ffffff;
            margin: 20px auto;
            padding: 0;
            border-radius: 5px;
            box-shadow: 0 2px 5px rgba(0,0,0,0.1);
        }}
        .header {{
            background-color: #1b6ec2;
            color: white;
            padding: 20px;
            text-align: center;
            border-radius: 5px 5px 0 0;
        }}
        .header h1 {{
            margin: 0;
            font-size: 24px;
        }}
        .content {{
            padding: 30px;
        }}
        .footer {{
            background-color: #f8f9fa;
            padding: 20px;
            text-align: center;
            font-size: 14px;
            color: #6c757d;
            border-radius: 0 0 5px 5px;
        }}
        a {{
            color: #1b6ec2;
            text-decoration: none;
        }}
        .button {{
            display: inline-block;
            padding: 10px 20px;
            background-color: #1b6ec2;
            color: white !important;
            text-decoration: none;
            border-radius: 5px;
            margin: 10px 0;
        }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>RepTrack</h1>
            <p style='margin: 5px 0 0 0; opacity: 0.9;'>Track Your Progress. Achieve Your Goals.</p>
        </div>
        <div class='content'>
            {content}
        </div>
        <div class='footer'>
            <p>&copy; 2025 RepTrack. All rights reserved.</p>
            <p>This is an automated message. Please do not reply to this email.</p>
        </div>
    </div>
</body>
</html>";
        }

        /// <summary>
        /// Removes HTML tags from content to create a plain text version
        /// This is used as a fallback for email clients that don't support HTML
        /// </summary>
        private string StripHtml(string html)
        {
            // Simple regex to remove HTML tags
            var text = System.Text.RegularExpressions.Regex.Replace(html, "<[^>]*(>|$)", string.Empty);

            // Replace multiple spaces with single space
            text = System.Text.RegularExpressions.Regex.Replace(text, @"\s+", " ");

            // Decode HTML entities
            text = System.Net.WebUtility.HtmlDecode(text);

            return text.Trim();
        }
    }
}