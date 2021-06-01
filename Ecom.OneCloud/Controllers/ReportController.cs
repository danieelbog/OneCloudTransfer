using Ecom.OneCloud.Enums;
using Ecom.OneCloud.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ecom.OneCloud.Controllers
{
    public class ReportController : BaseController
    {
        private readonly IDataTableActionsService _dataTableActionsService;

        public ReportController(ILoggingService loggingService, IDataTableActionsService dataTableActionsService)
            : base(loggingService)
        {
            _dataTableActionsService = dataTableActionsService;
        }

        [HttpPost("Report/AddReports")]
        public async Task<IActionResult> AddReports([FromBody]List<string> fileIds)
        {
            try
            {
                if (!fileIds.Any())
                    return ModelError("No files found to download");

                var actionTablePushResponse = await _dataTableActionsService.AddNewTableAsync(fileIds, ActionType.download);
                if (!actionTablePushResponse.Success)
                    return ModelError(actionTablePushResponse.Message);

                return Ok();
            }
            catch (Exception ex)
            {
                return ModelError(ex.Message);
            }
        }


        [HttpPost("Report/AddReport/{fileId}")]
        public async Task<IActionResult> AddReport(string fileId)
        {
            try
            {
                if(string.IsNullOrEmpty(fileId))
                    return ModelError("No files found to download");

                var actionTablePushResponse = await _dataTableActionsService.AddNewTableAsync(fileId, ActionType.download);
                if (!actionTablePushResponse.Success)
                    return ModelError(actionTablePushResponse.Message);

                return Ok();
            }
            catch (Exception ex)
            {
                return ModelError(ex.Message);
            }
        } 
    }
}
