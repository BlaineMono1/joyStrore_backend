using Gateway.WebApi.Attributes;
using Microsoft.AspNetCore.Mvc;
using Service.Application.Service.AddOnsQuery.Dto;
using Service.Application.Service.AddOnsQuery;

namespace Gateway.WebApi.Controllers
{
    [SetRoute("api/[controller]/[action]")]
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
        [HttpGet]
        public async Task<ActionResult<List<AddOnsListDto>>> GetGroupAddOnsList()
        {
            try
            {
                _logger.LogInformation("Fetching group add ons list");
                var addOnsList = await _addOnsQuery.GroupAddOnsList();
                return Ok(addOnsList);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching group add ons list.");
                return StatusCode(500, "An error occurred while retrieving the group add ons list.");
            }
        }

        /// <summary>
        /// Вывод списка Донатов
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<ActionResult<List<GroupAddOnsDto>>> GetAddOnsList(Guid id)
        {
            try
            {
                _logger.LogInformation("Fetching add ons list with id {id}", id);
                var addOns = await _addOnsQuery.AddOnsList(id);
                return Ok(addOns);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching add ons list.");
                return StatusCode(500, "An error occurred while retrieving the add ons list.");
            }
        }

    }
}
