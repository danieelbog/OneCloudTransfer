using Azure.Data.Tables;
using System.Collections.Generic;

namespace Ecom.OneCloud.Models.DTO
{
    public class ResponseDTO
    {
        public bool Success { get; set; }
        public string Message { get; set; }
    }

    public class CommitChunksResponseDTO : ResponseDTO
    {
        public string DownloadUrl { get; set; }
    }

    public class TableEntitiesResponseDTO : ResponseDTO
    {
        public List<TableEntity> TableEntities { get; set; }
    }
}
