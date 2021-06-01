using Azure.Data.Tables;
using Ecom.OneCloud.Models.DTO;
using Ecom.OneCloud.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Ecom.OneCloud.Models
{
    public static class Factory
    {
        public static SendEmailInfoDTO GetSendEmailInfoDTO(List<TableEntity> tableEntities, string downloadLink)
        {
            tableEntities.FirstOrDefault().TryGetValue(DataTableFields.EmailFrom, out var emailFrom);
            tableEntities.FirstOrDefault().TryGetValue(DataTableFields.EmailTo, out var emailTo);
            tableEntities.FirstOrDefault().TryGetValue(DataTableFields.Message, out var message);

            var emailDestions = new SendEmailInfoDTO
            {
                EmailFrom = emailTo == null ? throw new Exception("From email was empty") : emailFrom.ToString(),
                EmailTo = emailTo == null ? throw new Exception("To email was empty") : emailTo.ToString(),
                Message = message == null ? string.Empty : message.ToString(),
                DownloadLink = downloadLink
            };

            return emailDestions;
        }

        public static DownloadFileViewModel GetFileViewModel(string partitionKey, List<TableEntity> tableEntities)
        {
            var viewModel = new DownloadFileViewModel(partitionKey);

            foreach (var table in tableEntities)
            {
                table.TryGetValue(DataTableFields.ExpirationDate, out var expirationDate);
                if (DateTime.Parse(expirationDate.ToString()) < DateTime.Today)
                {
                    viewModel.LinkIsExpired = true;
                    return viewModel;
                }

                table.TryGetValue(DataTableFields.FileName, out var fileName);
                table.TryGetValue(DataTableFields.SasUrl, out var sasUrl);
                table.TryGetValue(DataTableFields.FileSize, out var fileSize);
                table.TryGetValue(DataTableFields.RowKey, out var fileId);

                var downloadItem = new DownloadItem(fileName.ToString(), sasUrl.ToString(), int.Parse(fileSize.ToString()), fileId.ToString());

                viewModel.DownloadItems.Add(downloadItem);
            }

            return viewModel;
        }
    }
}
