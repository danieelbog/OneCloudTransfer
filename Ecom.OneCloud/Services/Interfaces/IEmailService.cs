using Ecom.OneCloud.Models.DTO;
using FluentEmail.Core.Models;
using System.Threading.Tasks;

namespace Ecom.OneCloud.Services.Interfaces
{
    public interface IEmailService
    {
        Task<SendResponse> SendEmailVerificationCode(string emailTo, string subject, string verificationNumber);
        Task<ResponseDTO> SendDownloadEmails(SendEmailInfoDTO sendEmailInfo, string subject);
    }
}
