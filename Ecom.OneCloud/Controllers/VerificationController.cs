using Ecom.OneCloud.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace Ecom.OneCloud.Controllers
{
    public class VerificationController : BaseController
    {
        private readonly ISessionService _sessionService;
        private readonly IEmailService _emailService;

        public VerificationController(ILoggingService loggingService, ISessionService sessionService, IEmailService emailService)
            :base(loggingService)
        {
            _sessionService = sessionService;
            _emailService = emailService;
        }

        [HttpPost("Verification/GetEmailVerificationNumber/{sessionToken}/{emailFrom}")]
        public async Task<IActionResult> GetEmailVerificationNumber(string sessionToken, string emailFrom)
        {
            try
            {
                if (string.IsNullOrEmpty(sessionToken) || string.IsNullOrEmpty(emailFrom))
                    return ModelError("Session Token or Email from was Empty");

                var sessions = _sessionService.SetEmail(sessionToken, emailFrom);          

                var sendEmailResponse = await _emailService.SendEmailVerificationCode(emailFrom, "Validate your Email", sessions.VerificationNumber.ToString());
                if (!sendEmailResponse.Successful)
                    return ModelError(sendEmailResponse.ErrorMessages.ToString());

                return Ok();
            }
            catch (Exception ex)
            {
                return ModelError(ex.Message);
            }
        }

        [HttpPost("Verification/VerifyEmail/{sessionToken}/{emailFrom}/{verificationNumber}")]
        public IActionResult VerifyEmail(string sessionToken, string emailFrom, int verificationNumber)
        {
            try
            {
                if (string.IsNullOrEmpty(sessionToken) || string.IsNullOrEmpty(emailFrom))
                    return ModelError("Session Token or Email from was Empty");

                var emailVerified = _sessionService.VerifyEmail(sessionToken, emailFrom, verificationNumber);
                if (!emailVerified)
                    return ModelError("Email is not verified");

                return Ok();
            }
            catch (Exception ex)
            {
                return ModelError(ex.Message);
            }
        }
    }
}
