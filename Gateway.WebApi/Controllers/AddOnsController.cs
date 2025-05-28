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
        /// Вывод списка Донатов в разделе с донатами
        /// </summary>
        /// <returns></returns>
        [HttpGet("by-group")]
        public async Task<ActionResult<List<GroupAddOnsDto>>> GetAddOnsList(Guid GroupAddOnId)
        {
            try
            {
                var addOns = await _addOnsQuery.AddOnsList(GroupAddOnId);
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

        /// <summary>
        /// Вывод списка Донатов для продукта
        /// </summary>
        /// <returns></returns>
        [HttpGet("by-product")]
        public async Task<ActionResult<List<GroupAddOnsDto>>> GetGameAddOnList(Guid PrdocutId)
        {
            try
            {
                var addOns = await _addOnsQuery.GetGameAddOnList(PrdocutId);
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
