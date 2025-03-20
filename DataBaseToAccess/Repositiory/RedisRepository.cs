using Business.Data.Iterfaces.Store;
using StackExchange.Redis;


namespace DataBaseToAccess.Repositiory
{
    public class RedisRepository : IRedisRepository
    {
        private readonly IDatabase _redisDb;

        public RedisRepository(IConnectionMultiplexer redis)
        {
            //_redisDb = redis.GetDatabase();
        }

        public async Task SetAsync(string key, string value, TimeSpan? exp)
        {
            await _redisDb.StringSetAsync(key, value, exp);
        }

        public async Task<string?> GetAsync(string key)
        {
            return await _redisDb.StringGetAsync(key);
        }

        public async Task DeleteAsync(string key)
        {
            await _redisDb.KeyDeleteAsync(key);
        }

    }
}
