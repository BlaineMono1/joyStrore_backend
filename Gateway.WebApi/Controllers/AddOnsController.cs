using Gateway.WebApi.Attributes;
using Microsoft.AspNetCore.Mvc;
using Service.Application.Service.AddOnsQuery.Dto;
using Service.Application.Service.AddOnsQuery;
using static Service.Application.Exceptions.NotFoundExeption;

namespace Gateway.WebApi.Controllers
{
    [SetRoute("addon")]
    [ApiController]
    public class AddOnsController : ControllerBase
    {
        private readonly AddOnsQuery _addOnsQuery;
        private readonly ILogger<AddOnsController> _logger;

        public AddOnsController(AddOnsQuery addOnsQuery, ILogger<AddOnsController> logger)
        {
            _addOnsQuery = addOnsQuery;
            _logger = logger;
        }


        /// <summary>
        /// Вывод списка Донатов на главной странице
        /// </summary>
        /// <returns></returns>
        [HttpGet("layout")]
        public async Task<ActionResult<List<AddOnsListDto>>> GetGroupAddOnsList()
        {
            try
            {
                var addOnsList = await _addOnsQuery.GroupAddOnsList();
                return Ok(addOnsList);
            }
            catch (NotFoundException ex)
            {
                _logger.LogError(ex.Message);
                return StatusCode(404, ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return StatusCode(500, ex.Message);
            }
        }

        /// <summary>
        /// Вывод списка Донатов
        /// </summary>
        /// <returns></returns>
        [HttpGet("by-game")]
        public async Task<ActionResult<List<GroupAddOnsDto>>> GetAddOnsList(Guid id)
        {
            try
            {
                var addOns = await _addOnsQuery.AddOnsList(id);
                return Ok(addOns);
            }
            catch (NotFoundException ex)
            {
                _logger.LogError(ex.Message);
                return StatusCode(404, ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return StatusCode(500, ex.Message);
            }
        }



    }
}
