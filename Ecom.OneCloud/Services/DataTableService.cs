using Azure.Data.Tables;
using Ecom.OneCloud.Models;
using Ecom.OneCloud.Models.DTO;
using Ecom.OneCloud.Services.Interfaces;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ecom.OneCloud.Services
{
    public class DataTableService : IDataTableService
    {
        #region Constructor and DIP

        private readonly IConfiguration _configuration;
        public DataTableService(IConfiguration configuration)
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

        public async Task<ResponseDTO> AddNewTableAsync(ChunkMetadataDTO metaData, string partitionKey, string sasToken, SendEmailInfoDTO sendEmailInfo)
        {
            try
            {
                var tableClient = GetTableClient();

                var tableEntity = CreateTableEntity(metaData, partitionKey, sasToken, sendEmailInfo);

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

        public async Task<ResponseDTO> AddNewTableAsync(ChunkMetadataDTO metaData, string partitionKey, string sasToken)
        {
            try
            {
                var tableClient = GetTableClient();

                var tableEntity = CreateTableEntity(metaData, partitionKey, sasToken);

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
        #endregion

        #region Private Methods
        private TableClient GetTableClient()
        {
            var tableServiceClient = GetTableServiceClient();
            var tableClient = GetTableClient(tableServiceClient);
            return tableClient;
        }

        private TableEntity CreateTableEntity(ChunkMetadataDTO metaData, string partitionKey, string sasToken, SendEmailInfoDTO sendEmailInfo = null)
        {
            var entity = new TableEntity(partitionKey, metaData.FileGuid)
            {
                { DataTableFields.FileName, metaData.FileName},
                { DataTableFields.EmailFrom, sendEmailInfo?.EmailFrom },
                { DataTableFields.EmailTo, sendEmailInfo?.EmailTo },
                { DataTableFields.Message, sendEmailInfo?.Message },
                { DataTableFields.SasUrl, sasToken },
                { DataTableFields.FileSize, metaData.FileSize },
                { DataTableFields.UploadDate, DateTime.UtcNow },
                { DataTableFields.ExpirationDate, DateTime.UtcNow.AddDays(7) }
            };

            return entity;
        }

        private TableClient GetTableClient(TableServiceClient serviceClient)
        {
            var tableClient = serviceClient.GetTableClient(_configuration["AzureDataTableName"]);
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
