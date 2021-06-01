using Ecom.OneCloud.Models.DTO;
using Ecom.OneCloud.Services.Interfaces;
using FluentEmail.Core;
using FluentEmail.Core.Models;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Ecom.OneCloud.Services
{
    public class EmailService : IEmailService
    {
        private readonly IFluentEmailFactory _fluentEmailFactory;
        private readonly IFluentEmail _fluentEmail;

        public EmailService(IFluentEmailFactory fluentEmailFactory, IFluentEmail fluentEmail)
        {
            _fluentEmailFactory = fluentEmailFactory;
            _fluentEmail = fluentEmail;
        }

        public async Task<SendResponse> SendEmailVerificationCode(string emailTo, string subject, string verificationNumber)
        {
            var path = $"{Directory.GetCurrentDirectory()}/Views/EmailTemplates/CodeVerification.cshtml";

            var response = await _fluentEmailFactory
           .Create()
           .To(emailTo)
           .Subject($"Your code is: {verificationNumber}")
           .UsingTemplateFromFile(path, verificationNumber)
           .SendAsync();

            return response;
        }

        public async Task<ResponseDTO> SendDownloadEmails(SendEmailInfoDTO sendEmailInfo, string subject)
        {
            try
            {
                var pathToReciever = $"{Directory.GetCurrentDirectory()}/Views/EmailTemplates/DownloadEmailToReciever.cshtml";

                var responseToReciever = await _fluentEmailFactory
               .Create()
               .To(sendEmailInfo.EmailFrom)
               .Subject($"{sendEmailInfo.EmailFrom} sent you files via OneCloud")
               .UsingTemplateFromFile(pathToReciever, sendEmailInfo)
               .SendAsync();

                var pathToSender = $"{Directory.GetCurrentDirectory()}/Views/EmailTemplates/DownloadEmailToSender.cshtml";

                var responseToSender = await _fluentEmailFactory
               .Create()
               .To(sendEmailInfo.EmailTo)
               .Subject($"Your files were sent successfully to {sendEmailInfo.EmailTo}")
               .UsingTemplateFromFile(pathToSender, sendEmailInfo)
               .SendAsync();               

                return new ResponseDTO
                {
                    Success = responseToReciever.Successful && responseToSender.Successful,
                };
            }
            catch (Exception ex)
            {
                return new ResponseDTO
                {
                    Success = false,
                    Message = ex.Message
                };
            }
        }
    }
}
