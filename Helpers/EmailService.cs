using System;
using SendGrid;
using SendGrid.Helpers.Mail;
using System.Threading.Tasks;

public class EmailService
{
    private readonly IConfiguration _configuration;

    public EmailService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task SendEmailAsync(string toEmail, string subject, string body, byte[] attachment = null, string attachmentName = null)
    {
        var apiKey = _configuration["SendGrid:ApiKey"];
        var client = new SendGridClient(apiKey);
        var from = new EmailAddress("Sriharanmoorthy@gmail.com", "EBook");
        var to = new EmailAddress(toEmail);
        var plainTextContent = body;
        var msg = MailHelper.CreateSingleEmail(from, to, subject, plainTextContent, null);

        // Check if there is an attachment
        if (attachment != null && !string.IsNullOrEmpty(attachmentName))
        {
            // Convert the byte array to a Base64 string
            var base64Attachment = Convert.ToBase64String(attachment);

            // Add the attachment to the email message
            msg.AddAttachment(attachmentName, base64Attachment);
        }

        var response = await client.SendEmailAsync(msg);
    }

}
