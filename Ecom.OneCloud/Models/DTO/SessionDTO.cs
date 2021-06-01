namespace Ecom.OneCloud.Models.DTO
{
    public class SessionDTO
    {
        public string PartitionKey { get; set; }
        public string EmailFrom { get; set; }
        public int? VerificationNumber { get; set; }
    }
}
