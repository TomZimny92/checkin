using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Checkin.Services
{
    public class SecureStorageService : ISecureStorageService
    {
        public SecureStorageService() { }

        public async Task<string> Get(string key)
        {
            return await SecureStorage.Default.GetAsync(key);
        }

        public async Task Save(string key, string value)
        {
            await SecureStorage.Default.SetAsync(key, value);
        }
    }
}
