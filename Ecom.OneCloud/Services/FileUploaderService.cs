using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Blobs.Specialized;
using Azure.Storage.Sas;
using Ecom.OneCloud.Models.DTO;
using Ecom.OneCloud.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Ecom.OneCloud.Services
{
    public class FileUploaderService : IFileUploaderService
    {
        #region Constructor and DIP
        private readonly IConfiguration _configuration;
        public FileUploaderService(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        #endregion

        #region Public Methods       

        public async Task<ResponseDTO> AddChunk(ChunkMetadataDTO metaData, IFormFile myFile)
        {
            try
            {
                var blockBlobClient = await GetBlockBlobClientAsync(metaData.FileGuid);

                var response = await blockBlobClient.StageBlockAsync(GetBlockId(metaData), myFile.OpenReadStream());

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

        public async Task<CommitChunksResponseDTO> CommitChunks(ChunkMetadataDTO metaData)
        {
            try
            {
                var blockBlobClient = await GetBlockBlobClientAsync(metaData.FileGuid);

                await UploadBlockAsync(metaData, blockBlobClient);

                var downloadUrl = GetSasUriForBlob(blockBlobClient).AbsoluteUri;

                return new CommitChunksResponseDTO
                { 
                    Success = true,
                    DownloadUrl = downloadUrl
                };
            }
            catch (Exception ex)
            {
                return new CommitChunksResponseDTO
                {
                    Success = false,
                    Message = ex.Message
                };
            }            
        }        
        #endregion

        #region Private Methoss
        private async Task<BlobContainerClient> GetOrCreateBlobContainerClientAsync()
        {
            var containerClient = new BlobContainerClient(_configuration["AzureConnectionString"], _configuration["AzureBlobContainer"]);
            await containerClient.CreateIfNotExistsAsync();

            return containerClient;
        }

        private async Task<BlockBlobClient> GetBlockBlobClientAsync(string fileGuid)
        {
            var containerClient = await GetOrCreateBlobContainerClientAsync();

            var blockBlobClient = containerClient.GetBlockBlobClient(fileGuid);

            return blockBlobClient;
        }

        private static List<string> GetBlockIds(ChunkMetadataDTO metaDataObject)
        {
            var blockIds = new List<string>();

            for (int i = 0; i < metaDataObject.TotalCount; i++)
            {
                blockIds.Add(Convert.ToBase64String(Encoding.UTF8.GetBytes($"{metaDataObject.FileGuid}{i.ToString("d6")}")));
            }

            return blockIds;
        }

        private string GetBlockId(ChunkMetadataDTO metaDataObject)
        {
            var blockId = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{metaDataObject.FileGuid}{metaDataObject.Index.ToString("d6")}"));

            return blockId;
        }

        private async Task UploadBlockAsync(ChunkMetadataDTO metaData, BlockBlobClient blockBlobClient)
        {
            await CommitBlockListAsync(blockBlobClient, metaData);

            await SetFileNameMetadataAsync(blockBlobClient, metaData);
        }

        private async Task CommitBlockListAsync(BlockBlobClient blockBlobClient, ChunkMetadataDTO metaDataObject)
        {
            var blockIds = GetBlockIds(metaDataObject);

            await blockBlobClient.CommitBlockListAsync(blockIds);
        }

        private async Task SetFileNameMetadataAsync(BlockBlobClient blockBlobClient, ChunkMetadataDTO metaDataObject)
        {
            BlobHttpHeaders headers = new BlobHttpHeaders
            {
                ContentDisposition = $"attachment; filename={metaDataObject.FileName}",
                ContentType = "application/octet-stream"
            };

            await blockBlobClient.SetHttpHeadersAsync(headers);
        }

        private Uri GetSasUriForBlob(BlockBlobClient blobClient, string storedPolicyName = null)
        {
            if (!blobClient.CanGenerateSasUri)
                return null;

            BlobSasBuilder sasBuilder = new BlobSasBuilder()
            {
                BlobContainerName = blobClient.GetParentBlobContainerClient().Name,
                BlobName = blobClient.Name,
                Resource = "b",

                //StartsOn = startsOn,
                //ExpiresOn = expiresOn,
                //Protocol = SasProtocol.Https,
            };

            if (storedPolicyName == null)
            {
                sasBuilder.ExpiresOn = DateTimeOffset.UtcNow.AddDays(7);
                sasBuilder.SetPermissions(BlobSasPermissions.Read |
                    BlobSasPermissions.Write);
            }
            else
            {
                sasBuilder.Identifier = storedPolicyName;
            }

            Uri sasUri = blobClient.GenerateSasUri(sasBuilder);
            return sasUri;
        }
        #endregion
    }
}
