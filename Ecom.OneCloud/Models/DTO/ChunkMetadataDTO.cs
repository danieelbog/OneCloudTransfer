namespace Ecom.OneCloud.Models.DTO
{
    public class ChunkMetadataDTO
    {
        public int Index { get; set; }
        public int TotalCount { get; set; }
        public int FileSize { get; set; }
        public string FileName { get; set; }
        public string FileType { get; set; }
        public string FileGuid { get; set; }
    }
}
