using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Service.Application.Iterfaces;

namespace Services.GetRegionFromCookie
{
    public class RegionFromCookie : IRegionFromCookie
    {
        private readonly ILogger<RegionFromCookie> _logger;

        public RegionFromCookie(ILogger<RegionFromCookie> logger)
        {
            _logger = logger;
        }

        public string GetUserRegion(IHttpContextAccessor _httpContextAccessor)
        {
            var httpContext = _httpContextAccessor.HttpContext;

            if (httpContext?.Request.Cookies.TryGetValue("region", out string region) == true)
            {
                _logger.LogInformation("Cookie 'region' found with value: {Region}", region);
                return region;
            }
            else
            {
                _logger.LogWarning("Cookie 'region' not found. Returning default value.");
                return "default"; // Значение по умолчанию, если кука не найдена
            }
        }

        public string GetUserTgID(IHttpContextAccessor _httpContextAccessor)
        {
            var httpContext = _httpContextAccessor.HttpContext;

            if (httpContext?.Request.Cookies.TryGetValue("tgid", out string tgid) == true)
            {
                _logger.LogInformation("Cookie 'tgid' found with value: {TgId}", tgid);
                return tgid;
            }
            else
            {
                _logger.LogWarning("Cookie 'tgid' not found. Returning default value.");
                return "default"; // Значение по умолчанию, если кука не найдена
            }
        }
    }
}
