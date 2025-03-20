using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Service.Application.Iterfaces;

namespace Services.GetRegionFromCookie
{
    public class DataFromCookie : IDataFromCookie
    {
        private readonly ILogger<DataFromCookie> _logger;
        private readonly IHttpContextAccessor _contextAccessor;
        public DataFromCookie(ILogger<DataFromCookie> logger, IHttpContextAccessor contextAccessor)
        {
            _logger = logger;
            _contextAccessor = contextAccessor;
        }

        public string GetUserRegion()
        {
            var httpContext = _contextAccessor.HttpContext;
            try
            {
                return httpContext.Request.Cookies["region"];
            }
            catch(Exception ex)
            {
                _logger.LogError(ex.Message);
                throw;
            }

            //if (httpContext?.Request.Cookies.TryGetValue("region", out string region) == true)
            //{
            //    _logger.LogInformation("Cookie 'region' found with value: {Region}", region);
            //    return region;
            //}
            //else
            //{
            //    _logger.LogWarning("Cookie 'region' not found. Returning default value.");
            //    return "default"; // Значение по умолчанию, если кука не найдена
            //}
        }

        public string GetUserTgID()
        {
            var httpContext = _contextAccessor.HttpContext;
            try
            {
                return httpContext.Request.Cookies["tgid"];
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                throw;
            }
            //if (httpContext?.Request.Cookies.TryGetValue("tgid", out string tgid) == true)
            //{
            //    _logger.LogInformation("Cookie 'tgid' found with value: {TgId}", tgid);
            //    return tgid;
            //}
            //else
            //{
            //    _logger.LogWarning("Cookie 'tgid' not found. Returning default value.");
            //    return "default"; // Значение по умолчанию, если кука не найдена
            //}
        }
    }
}
