using Microsoft.AspNetCore.Http;
using Service.Application.Iterfaces;

namespace Services.GetRegionFromCookie
{
    public class RegionFromCookie : IRegionFromCookie
    {
        public string GetUserRegion(IHttpContextAccessor _httpContextAccessor) 
        {
            var httpContext = _httpContextAccessor.HttpContext;

            if (httpContext?.Request.Cookies.TryGetValue("region", out string region) == true)
            {
                return region;
            }

            return "default"; // Значение по умолчанию, если кука не найдена
        }
    }
}
