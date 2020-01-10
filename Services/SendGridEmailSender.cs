using GreenHealth.Email;
using SendGrid;
using SendGrid.Helpers.Mail;
using System;
using System.Threading.Tasks;

namespace GreenHealth.Services
{

    public class SendGridEmailSender : IEmailSender
      
    {
        private readonly Microsoft.Extensions.Configuration.IConfiguration _appSettings;
        public SendGridEmailSender(Microsoft.Extensions.Configuration.IConfiguration appSettings)
        
    {
        _appSettings = appSettings;
    }

        public async Task<SendEmailResponse> SendEmailAsync(string userEmail, string emailSubject, string message )
        {
            try
            {

                var apiKey = _appSettings["AppSettings:SendGridKey"];
                                                                     
                var client = new SendGridClient(apiKey);
                var from = new EmailAddress("tanvidhupkar0728@gmail.com", "greenhealth.azurewebsites.net");
                var subject = emailSubject;
                var to = new EmailAddress(userEmail, "Test");
                var plainTextContent = message;
                var htmlContent = message;
                var msg = MailHelper.CreateSingleEmail(from, to, subject, plainTextContent, htmlContent);
                var response = await client.SendEmailAsync(msg);

                return new SendEmailResponse();
            }
            catch (Exception e)
            {

                throw e;
            }
        }
    }

   
}

