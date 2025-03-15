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
        public async Task<ActionResult<List<FavoriteDto>>> GetUserFavorite(Guid UserId)
        {
            try
            {
                _logger.LogInformation("Fetching user favorite items whith tg ID {id}", UserId);
                var fav = await _favoriteQuery.UserFavorite(UserId);
                return Ok(fav);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while Fetching favorite items with tg ID : {id}", UserId);
                return StatusCode(500, "Error occurred while fetching favorite items");
            }

        }

        /// <summary>
        /// Добавление предмета в избранное пользователя
        /// </summary>
        /// <returns></returns>
        /// 

        [HttpPut]

        public async Task<ActionResult> AddItemInFavorite(Guid userId, Guid productId)
        {
            try
            {
                _logger.LogInformation("Adding item with GUID {id} to user Favorite with tg id {tgid}", productId, userId);
                await _favoriteQuery.UpdateUserFavorites(userId, productId);
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while adding item in user Favorites with tg ID : {id}, item GUID {id}", userId, productId);
                return StatusCode(500, "Error occurred while adding item in user Favorites");
            }
        }

        /// <summary>
        /// Удаление предмета из избранного пользователя
        /// </summary>
        /// <returns></returns>
        /// 

        [HttpDelete]

        public async Task<ActionResult> DeleteItemInFavorite(Guid userId, Guid productId)
        {
            try
            {
                _logger.LogInformation("Deleting item with GUID {id} to user Favorite with tg id {tgid}", productId, userId);
                await _favoriteQuery.DeleteFromFavorites(userId, productId);
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while Deliting item in user Favorites with tg ID : {id}, item GUID {id}", userId, productId);
                return StatusCode(500, "Error occurred while Deliting item in user Favorites");
            }
        }
    }
}
