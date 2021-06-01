namespace Ecom.OneCloud.Models.ViewModels
{
    public class DownloadItem
    {
        public string FileName { get; set; }
        public string SasUrl { get; set; }
        public int FileSize { get; set; }
        public string FileId { get; set; }

        protected DownloadItem()
        {

        }

        public DownloadItem(string fileName, string sasUrl, int fileSize, string fileId)
        {
            FileName = fileName;
            SasUrl = sasUrl;
            FileSize = fileSize;
            FileId = fileId;
        }
    }
}
