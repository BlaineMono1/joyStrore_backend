
using Microsoft.AspNetCore.Http;

namespace Service.Application.Iterfaces
{
    public interface IRegionFromCookie
    {
        string GetUserRegion(IHttpContextAccessor _httpContextAccessor);
    }
}
