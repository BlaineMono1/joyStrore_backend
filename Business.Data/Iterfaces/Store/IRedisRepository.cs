using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Business.Data.Iterfaces.Store
{
    public interface IRedisRepository
    {
        Task SetAsync(string key, string value, TimeSpan? exp);
        Task<string?> GetAsync(string key);
        Task DeleteAsync(string key);
    }
}
