using Gateway.WebApi.Attributes;
using Microsoft.AspNetCore.Mvc;
using Service.Application.Service.AddOnsQuery.Dto;
using Service.Application.Service.AddOnsQuery;

namespace Gateway.WebApi.Controllers
{
    [SetRoute("api/[controller]/[action]")]
    [ApiController]
    public class AddOnsController
    {
        private readonly AddOnsQuery _addOnsQuery;
        /// <summary>
        /// Вывод списка Донатов на главной странице
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<ActionResult<List<AddOnsListDto>>> GetGroupAddOnsList()
        {
            return await _addOnsQuery.GroupAddOnsList();
        }

        /// <summary>
        /// Вывод списка Донатов
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<ActionResult<List<GroupAddOnsDto>>> GetAddOnsList(Guid id)
        {
            return await _addOnsQuery.AddOnsList(id);
        }

        /// <summary>
        /// Вывод Доната
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<ActionResult<AddOnDto>> GetAddOn(Guid id)
        {
            return await _addOnsQuery.AddOnById(id);
        }
    }
}
