using Ecom.OneCloud.Models.DTO;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace Ecom.OneCloud.Services.Interfaces
{
    public interface IFileUploaderService
    {
        Task<ResponseDTO> AddChunk(ChunkMetadataDTO metaData, IFormFile myFile);
        Task<CommitChunksResponseDTO> CommitChunks(ChunkMetadataDTO metaData);
    }
}
