using Ecom.OneCloud.Enums;
using Ecom.OneCloud.Models;
using Ecom.OneCloud.Models.DTO;
using Ecom.OneCloud.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;

namespace Ecom.OneCloud.Controllers
{
    public class OneCloudController : BaseController
    {
        #region Constructor and DIP

        private readonly IConfiguration _configuration;
        private readonly IFileUploaderService _fileUploaderService;
        private readonly IDataTableService _dataTableService;
        private readonly IDataTableActionsService _dataTableActionsService;
        private readonly ISessionService _sessionService;


        private readonly IEmailService _emailService;

        public OneCloudController(IConfiguration configuration,
                                  IFileUploaderService fileUploaderService,
                                  IDataTableService dataTableService,
                                  IDataTableActionsService dataTableActionsService,
                                  ISessionService sessionService,
                                  ILoggingService loggingService,
                                  IEmailService emailService)
            : base(loggingService)
        {
            _configuration = configuration;
            _fileUploaderService = fileUploaderService;
            _dataTableService = dataTableService;
            _dataTableActionsService = dataTableActionsService;
            _sessionService = sessionService;
            _emailService = emailService;
        }

        #endregion

        #region Public Get
        [HttpGet]
        public IActionResult UploadFile()
        {
            return View();
        }

        [HttpGet("OneCloud/GetDownloadLink/{sessionToken}")]
        public async Task<IActionResult> GetDownloadLink(string sessionToken)
        {
            try
            {
                var session = _sessionService.GetOrSetPartitionKey(sessionToken);
                if (string.IsNullOrEmpty(session.PartitionKey))
                    return ModelError("Partition Key is not valid");

                var downloadUrl = $"{_configuration["SiteBaseUrl"]}/OneCloud/DownloadFile/{session.PartitionKey}";

                var response = new
                {
                    DownloadUrl = downloadUrl,
                    UploadType = string.IsNullOrEmpty(session.EmailFrom) ? "GetLink" : "SendEmail"
                };

                if (string.IsNullOrEmpty(session.EmailFrom))
                    return Ok(response);

                var tableEntities = _dataTableService.GetTableEntities(session.PartitionKey);

                var emailSendInfo = Factory.GetSendEmailInfoDTO(tableEntities.TableEntities, downloadUrl);

                var sendEmailResponse = await _emailService.SendDownloadEmails(emailSendInfo, "Download Files");
                if (!sendEmailResponse.Success)
                    return ModelError(sendEmailResponse.Message.ToString());

                return Ok(response);
            }
            catch (Exception ex)
            {
                return ModelError(ex.Message);
            }
        }

        [HttpGet("OneCloud/DownloadFile/{partitionKey}")]
        public IActionResult DownloadFile(string partitionKey)
        {
            try
            {
                var getTablesResponse = _dataTableService.GetTableEntities(partitionKey);

                var viewModel = Factory.GetFileViewModel(partitionKey, getTablesResponse.TableEntities);

                return View(viewModel);
            }
            catch (Exception ex)
            {
                return ModelError(ex.Message);
            }
        }
        #endregion

        #region Public Post


        [HttpPost("OneCloud/UploadChunkAndSendEmail/{emailTo}/{emailFrom}/{sessionToken}/{message?}")]
        public async Task<IActionResult> UploadChunkAndSendEmail(IFormFile myFile,
                                                     string chunkMetadata,
                                                     string emailTo,
                                                     string emailFrom,
                                                     string sessionToken,
                                                     string message)
        {
            try
            {
                if (!ModelIsValid(chunkMetadata, emailTo, emailFrom, sessionToken))
                    return BadRequest();

                var metaData = GetMetadata(chunkMetadata);

                if (metaData.Index == 0)
                {
                    if (!ValidateSession(sessionToken, emailFrom))
                        return BadRequest();
                }

                var stageBlockResponse = await _fileUploaderService.AddChunk(metaData, myFile);
                if (!stageBlockResponse.Success)
                    return ModelError(stageBlockResponse.Message);

                if (metaData.Index != metaData.TotalCount - 1)
                    return Ok();

                var commitResponse = await _fileUploaderService.CommitChunks(metaData);
                if (!commitResponse.Success)
                    return ModelError(commitResponse.Message);

                var sendEmailInfo = new SendEmailInfoDTO
                {
                    EmailFrom = emailFrom,
                    EmailTo = emailTo,
                    Message = message
                };

                var session = _sessionService.GetOrSetPartitionKey(sessionToken).PartitionKey;

                var tablePushResponse = await _dataTableService.AddNewTableAsync(metaData, session, commitResponse.DownloadUrl, sendEmailInfo);
                if (!tablePushResponse.Success)
                    return ModelError(tablePushResponse.Message);

                var actionTablePushResponse = await _dataTableActionsService.AddNewTableAsync(metaData.FileGuid, ActionType.upload);
                if (!actionTablePushResponse.Success)
                    return ModelError(actionTablePushResponse.Message);

                return Ok();
            }
            catch (Exception ex)
            {
                return ModelError(ex.Message);
            }
        }



        [HttpPost("OneCloud/UploadChunkAndGetDownloadLink/{sessionToken}")]
        public async Task<IActionResult> UploadChunkAndGetDownloadLink(IFormFile myFile,
                                                                       string chunkMetadata,
                                                                       string sessionToken)
        {
            try
            {
                if (string.IsNullOrEmpty(chunkMetadata))
                    return BadRequest(ModelState);

                var metaData = GetMetadata(chunkMetadata);

                if (metaData.Index == 0)
                    _sessionService.ResetMememoryCacheEmailFrom(sessionToken);

                var stageBlockResponse = await _fileUploaderService.AddChunk(metaData, myFile);
                if (!stageBlockResponse.Success)
                    return ModelError(stageBlockResponse.Message);

                if (metaData.Index != metaData.TotalCount - 1)
                    return Ok();

                var commitResponse = await _fileUploaderService.CommitChunks(metaData);
                if (!commitResponse.Success)
                    return ModelError(commitResponse.Message);

                var session = _sessionService.GetOrSetPartitionKey(sessionToken);

                var dataTablePushResponse = await _dataTableService.AddNewTableAsync(metaData, session.PartitionKey, commitResponse.DownloadUrl);
                if (!dataTablePushResponse.Success)
                    return ModelError(dataTablePushResponse.Message);

                var actionTablePushResponse = await _dataTableActionsService.AddNewTableAsync(metaData.FileGuid, ActionType.upload);
                if (!actionTablePushResponse.Success)
                    return ModelError(actionTablePushResponse.Message);

                return Ok();
            }
            catch (Exception ex)
            {
                return ModelError(ex.Message);
            }
        }
        #endregion

        #region Private Region         
        private ChunkMetadataDTO GetMetadata(string chunkMetadata)
        {
            var metadata = JsonConvert.DeserializeObject<ChunkMetadataDTO>(chunkMetadata);

            return metadata;
        }

        private bool ModelIsValid(string chunkMetadata, string emailTo, string emailFrom, string sessionToken)
        {
            var guidIsValid = Guid.TryParse(sessionToken, out var output);

            if (!guidIsValid ||
                string.IsNullOrEmpty(chunkMetadata) ||
                string.IsNullOrWhiteSpace(emailTo) ||
                string.IsNullOrWhiteSpace(emailFrom))
                return false;

            return true;
        }

        private bool ValidateSession(string sessionToken, string emailFrom)
        {
            var session = _sessionService.GetOrCreateMemoryCache(sessionToken);
            if (session.EmailFrom != emailFrom)
                return false;

            return true;
        }
        #endregion
    }
}
