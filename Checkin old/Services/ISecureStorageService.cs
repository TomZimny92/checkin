using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Checkin.Services
{
    public interface ISecureStorageService
    {
        Task Save(string key, string value);
        Task<string> Get(string key);
    }
}
