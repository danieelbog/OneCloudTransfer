using System.Collections.Generic;

namespace Ecom.OneCloud.Models.ViewModels
{
    public class DownloadFileViewModel
    {
        public List<DownloadItem> DownloadItems { get; set; }
        public string PartitionKey { get; set; }
        public bool LinkIsExpired { get; set; }

        protected DownloadFileViewModel()
        {
            DownloadItems = new List<DownloadItem>();
        }

        public DownloadFileViewModel(string partitionKey) 
            :this()
        {
            PartitionKey = partitionKey;
        }

        public DownloadFileViewModel(List<DownloadItem> downloadItems)
        {
            DownloadItems = downloadItems;
        }
    }
}
