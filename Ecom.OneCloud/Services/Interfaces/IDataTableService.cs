using Ecom.OneCloud.Models.DTO;
using System.Threading.Tasks;

namespace Ecom.OneCloud.Services.Interfaces
{
    public interface IDataTableService
    {
        Task<ResponseDTO> AddNewTableAsync(ChunkMetadataDTO metaData, string partitionKey, string sasToken, SendEmailInfoDTO sendEmailInfo);
        Task<ResponseDTO> AddNewTableAsync(ChunkMetadataDTO metaData, string partitionKey, string sasToken);
        TableEntitiesResponseDTO GetTableEntities(string partitionKey);
    }
}
