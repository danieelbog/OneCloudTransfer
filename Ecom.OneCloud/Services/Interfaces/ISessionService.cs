using Ecom.OneCloud.Models.DTO;

namespace Ecom.OneCloud.Services.Interfaces
{
    public interface ISessionService
    {
        public SessionDTO GetOrSetPartitionKey(string sessionToken);
        public bool VerifyEmail(string sessionToken, string emailFrom, int verificationNumber);
        public SessionDTO SetEmail(string sessionToken, string emailFrom);
        public SessionDTO GetOrCreateMemoryCache(string sessionToken);
        void ResetMememoryCacheEmailFrom(string sessionToken);
    }
}
