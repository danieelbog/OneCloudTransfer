using Azure.Data.Tables;
using Ecom.OneCloud.Enums;
using Ecom.OneCloud.Models;
using Ecom.OneCloud.Models.DTO;
using Ecom.OneCloud.Services.Interfaces;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ecom.OneCloud.Services
{
    public class DataTableActionsService : IDataTableActionsService
    {
        #region Constructor and DIP
        private readonly IConfiguration _configuration;
        public DataTableActionsService(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        #endregion

        #region Public Methods
        public TableEntitiesResponseDTO GetTableEntities(string partitionKey)
        {
            try
            {
                var tableClient = GetTableClient();

                var queryResultsFilter = tableClient.Query<TableEntity>(filter: $"PartitionKey eq '{partitionKey}'");
                var tableEntities = new List<TableEntity>();

                foreach (TableEntity tableEnetity in queryResultsFilter)
                {
                    tableEntities.Add(tableEnetity);
                }

                return new TableEntitiesResponseDTO
                {
                    Success = true,
                    TableEntities = tableEntities
                };
            }
            catch (Exception ex)
            {
                return new TableEntitiesResponseDTO
                {
                    Success = false,
                    Message = ex.Message
                };
            }
        }

        public async Task<ResponseDTO> AddNewTableAsync(string fileId, ActionType actionType)
        {
            try
            {
                var tableClient = GetTableClient();

                var tableEntity = CreateTableEntity(fileId, actionType);

                var response = await tableClient.AddEntityAsync(tableEntity);

                return new ResponseDTO
                {
                    Success = true
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

        public async Task<ResponseDTO> AddNewTableAsync(List<string> fileIds, ActionType actionType)
        {
            try
            {
                var tableClient = GetTableClient();

                foreach (var fileId in fileIds)
                {
                    var tableEntity = CreateTableEntity(fileId, actionType);
                    var response = await tableClient.AddEntityAsync(tableEntity);
                }

                return new ResponseDTO
                {
                    Success = true
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
        #endregion

        #region Private Methods
        private TableClient GetTableClient()
        {
            var tableServiceClient = GetTableServiceClient();
            var tableClient = GetTableClient(tableServiceClient);
            return tableClient;
        }

        private TableEntity CreateTableEntity(string fileId, ActionType actionType)
        {
            var entity = new TableEntity(fileId, Guid.NewGuid().ToString())
            {
                { ActionsTableFields.Action, actionType.ToString() },
                { ActionsTableFields.ActionDate, DateTime.UtcNow }
            };

            return entity;
        }

        private TableClient GetTableClient(TableServiceClient serviceClient)
        {
            var tableClient = serviceClient.GetTableClient(_configuration["AzureActionsTableName"]);
            return tableClient;
        }

        private TableServiceClient GetTableServiceClient()
        {
            var serviceClient = new TableServiceClient(_configuration["AzureConnectionString"]);
            return serviceClient;
        }
        #endregion
    }
}
