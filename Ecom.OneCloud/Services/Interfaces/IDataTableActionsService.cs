using Ecom.OneCloud.Enums;
using Ecom.OneCloud.Models.DTO;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ecom.OneCloud.Services.Interfaces
{
    public interface IDataTableActionsService
    {
        TableEntitiesResponseDTO GetTableEntities(string partitionKey);
        Task<ResponseDTO> AddNewTableAsync(string fileId, ActionType actionType);
        Task<ResponseDTO> AddNewTableAsync(List<string> fileIds, ActionType actionType);

    }
}
