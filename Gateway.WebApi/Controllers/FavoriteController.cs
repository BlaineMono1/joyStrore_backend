using Gateway.WebApi.Attributes;
using Microsoft.AspNetCore.Mvc;
using Service.Application.Service.FavoriteQuery;
using Service.Application.Service.FavoriteQuery.Dto;

namespace Gateway.WebApi.Controllers
{
    [SetRoute("api/[controller]/[action]")]
    [ApiController]
    public class FavoriteController : ControllerBase
    {
        private readonly FavoriteQuery _favoriteQuery;
        private readonly ILogger<FavoriteQuery> _logger;

        public FavoriteController(FavoriteQuery favoriteQuery, ILogger<FavoriteQuery> logger)
        {
            _favoriteQuery = favoriteQuery;
            _logger = logger;
        }

        /// <summary>
        /// Вывод избранного пользователя
        /// </summary>
        /// <returns></returns>
        /// 

        [HttpGet]
        public async Task<ActionResult<List<FavoriteDto>>> GetUserFavorite()
        {
            try
            {
  
                var fav = await _favoriteQuery.UserFavorite();
                return Ok(fav);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Error occurred while fetching favorite items");
            }

        }

        /// <summary>
        /// Добавление предмета в избранное пользователя
        /// </summary>
        /// <returns></returns>
        /// 

        [HttpPut]

        public async Task<ActionResult> AddItemInFavorite(Guid productId)
        {
            try
            {
                await _favoriteQuery.UpdateUserFavorites(productId);
                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Error occurred while adding item in user Favorites");
            }
        }

        /// <summary>
        /// Удаление предмета из избранного пользователя
        /// </summary>
        /// <returns></returns>
        /// 

        [HttpDelete]

        public async Task<ActionResult> DeleteItemInFavorite(Guid productId)
        {
            try
            {
                await _favoriteQuery.DeleteFromFavorites(productId);
                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Error occurred while Deliting item in user Favorites");
            }
        }
    }
}
