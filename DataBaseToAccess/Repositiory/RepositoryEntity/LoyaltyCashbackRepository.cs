using Business.Data.Iterfaces;
using Business.Data.Iterfaces.Store;
using Business.Data.Models;
using System.Text.Json;

namespace DataBaseToAccess.Repositiory.RepositoryEntity
{
    public class LoyaltyCashbackRepository<T> : Repository<LoyaltyCashback>, ILoyaltyCashbackRepository<T> where T : class, IBaseEntity
    {
        private readonly IRedisRepository _redis;
        public LoyaltyCashbackRepository(BaseDbContext contex, IRedisRepository redis) : base(contex) 
        {
            _redis = redis;
        }

                
        public async new Task Update(LoyaltyCashback entity)
        {
            await base.Update(entity);
          
            string cacheKey = "cashback";
            string jsonData = JsonSerializer.Serialize(entity.Percent);

            await _redis.SetAsync(cacheKey, jsonData, null);
        }

       
    }
}
