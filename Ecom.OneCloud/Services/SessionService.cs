using Ecom.OneCloud.Models.DTO;
using Ecom.OneCloud.Services.Interfaces;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Linq;

namespace Ecom.OneCloud.Services
{
    public class SessionService : ISessionService
    {
        private readonly IMemoryCache _memoryCache;

        public SessionService(IMemoryCache memoryCache)
        {
            _memoryCache = memoryCache;
        }        

        public SessionDTO GetOrSetPartitionKey(string sessionToken)
        {
            var memoryCache = GetOrCreateMemoryCache(sessionToken);
            if (memoryCache.PartitionKey != null)
                return memoryCache;

            var partitionKey = Guid.NewGuid();
            memoryCache.PartitionKey = partitionKey.ToString();

            return _memoryCache.Set(sessionToken, memoryCache);
        }

        public void ResetMememoryCacheEmailFrom(string sessionToken)
        {
            var memoryCache = GetOrCreateMemoryCache(sessionToken);
            if (memoryCache != null && memoryCache.EmailFrom != null)
            {
                memoryCache.EmailFrom = null;
                _memoryCache.Set(sessionToken, memoryCache);
            }
        }

        public bool VerifyEmail(string sessionToken, string emailFrom, int verificationNumber)
        {
            var memoryCache = GetOrCreateMemoryCache(sessionToken);
            if (memoryCache.EmailFrom != emailFrom || memoryCache.VerificationNumber != verificationNumber)
                return false;

            return true;
        }

        public SessionDTO SetEmail(string sessionToken, string emailFrom)
        {
            var memoryCache = GetOrCreateMemoryCache(sessionToken);
            if (memoryCache == null)
                return null;

            memoryCache.EmailFrom = emailFrom;
            memoryCache.VerificationNumber = GenerateVerificationNumber();

            return _memoryCache.Set(sessionToken, memoryCache);
        }

        public SessionDTO GetOrCreateMemoryCache(string sessionToken)
        {
            return _memoryCache.GetOrCreate(sessionToken, entry =>
            {
                entry.SlidingExpiration = TimeSpan.FromDays(1);
                return new SessionDTO(); ;
            });
        }

        private int GenerateVerificationNumber()
        {
            Random generator = new Random();
            var verificationNumber = generator.Next(0, 1000000).ToString("D6");
            if (verificationNumber.Distinct().Count() == 1 && verificationNumber.Length != 6)
            {
                verificationNumber = GenerateVerificationNumber().ToString();
            }
            return int.Parse(verificationNumber);
        }
    }
}
