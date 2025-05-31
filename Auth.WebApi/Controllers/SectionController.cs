using Auth.WebApi.Attributes;
using Business.Data.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Service.Application.Service.SectionQuery;
using Service.Application.Service.SectionQuery.Dto;
using static Service.Application.Exceptions.NotFoundExeption;

namespace Auth.WebApi.Controllers
{
    [ApiController]
    [SetRoute("sections")]
    public class SectionController : ControllerBase
    {
        private readonly ILogger<SectionController> _logger;
        private readonly SectionQuery _query;

        public SectionController(ILogger<SectionController> logger, SectionQuery query)
        {
            _logger = logger;
            _query = query;
        }

        /// <summary>
        /// Создание секций в админ панели
        /// </summary>
        /// <param name="sectionName"></param>
        /// <param name="imagePath"></param>
        [Authorize(Roles = "Admin,Worker")]
        [HttpGet("create-section")]
        public async Task<ActionResult> CreateSection(string sectionName, string imagePath)
        {
            try
            {
                await _query.CreateSections(sectionName, imagePath);
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return StatusCode(500, ex.Message);
            }
        }
        /// <summary>
        /// Изменение имени секции в админ панели
        /// </summary>
        /// <param name="SectionId"></param>
        /// <param name="SectionName"></param>
        [Authorize(Roles = "Admin,Worker")]
        [HttpPut("update-sections")]
        public async Task<ActionResult> UpdateSection(Guid SectionId, string SectionName)
        {
            try
            {
                await _query.UpdateSection(SectionId, SectionName);
                return Ok();
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
        /// Удаление секции в админ панели
        /// </summary>
        /// <param name="SectionId"></param>
        [Authorize(Roles = "Admin,Worker")]
        [HttpDelete("delete-section")]
        public async Task<ActionResult> DeleteSections(Guid SectionId)
        {
            try
            {
                await _query.DeleteSection(SectionId);
                return Ok();
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
        /// Получение секций в админ панели
        /// </summary>
        [Authorize(Roles = "Admin,Worker")]
        [HttpGet("get-section-list")]
        public async Task<ActionResult<List<SectionsDto>>> GetSectionsList()
        {

            try
            {
                var result = await _query.SectionsList();
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return StatusCode(500, ex.Message);
            }
        }

        /// <summary>
        /// Получение секции по id в админ панели
        /// </summary>
        /// <param name="SectionId"></param>
        [Authorize(Roles = "Admin,Worker")]
        [HttpGet("get-section")]

        public async Task<ActionResult<Section>> GetSection(Guid SectionId)
        {
            try
            {
                var result = await _query.SectionById(SectionId);
                return Ok(result);
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
        /// Добавление продукта в секцию
        /// </summary>
        /// <param name="SectionId"></param>
        /// /// <param name="ProductId"></param>
        [Authorize(Roles = "Admin,Worker")]
        [HttpPut("add-product")]

        public async Task<ActionResult> AddProduct(Guid SectionId, Guid ProductId)
        {
            try
            {
                await _query.AddProductInSection(SectionId, ProductId);
                return Ok();
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
        /// Удаление продукта из секции
        /// </summary>
        /// <param name="SectionId"></param>
        /// /// <param name="ProductId"></param>
        [Authorize(Roles = "Admin,Worker")]
        [HttpDelete("delete-product")]

        public async Task<ActionResult> DeleteProduct(Guid SectionId, Guid ProductId)
        {
            try
            {
                await _query.DeleteProductFromSection(SectionId, ProductId);
                return Ok();
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
        /// Фильтр продуктов для добавления в секцию
        /// </summary>
        /// <param name="Name"></param>
        /// /// <param name="isEdition"></param>
        [Authorize(Roles = "Admin,Worker")]
        [HttpGet("get-prodcut-list")]

        public async Task<ActionResult<List<ProductDto>>> GetEditionsList(string Name = "", bool isEdition = true)
        {
            try
            {
                var result = await _query.FindProductByName(Name, isEdition);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return StatusCode(500, ex.Message);
            }
        }

        /// <summary>
        /// Обновление карточки продукта
        /// </summary>
        /// <param name="ProductId"></param>
        ///<param name="Name"></param>
        ///<param name="url"></param>
        ///
        [Authorize(Roles = "Admin,Worker")]
        [HttpPut("update-product")]
        public async Task<ActionResult> UpdateProduct(Guid ProductId, string Name, string url)
        {
            try
            {
                await _query.UpdateProduct(ProductId, Name, url);
                return Ok();
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
        /// Получение списка донатов в админ панели
        /// </summary>
        [Authorize(Roles = "Admin,Worker")]
        [HttpGet("get-addon-list")]
        public async Task<ActionResult<List<AddOnSectionList>>> GetAddOnsGroups()
        {

            try
            {
                var result = await _query.GetAddOnsGroups();
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return StatusCode(500, ex.Message);
            }
        }

        /// <summary>
        /// Добавление групы с донатом
        /// </summary>
        [Authorize(Roles = "Admin,Worker")]
        [HttpGet("add-addon-group")]
        public async Task<ActionResult<List<AddOnSectionList>>> CreateAddOnGroup(string Name, string Url)
        {

            try
            {
               await _query.CreateAddOnGroup(Name, Url);
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return StatusCode(500, ex.Message);
            }
        }

        /// <summary>
        /// Изменение имени списка донатов в админ панели
        /// </summary>
        /// <param name="GroupdId"></param>
        /// <param name="Name"></param>
        /// /// <param name="Url"></param>
        [Authorize(Roles = "Admin,Worker")]
        [HttpPut("update-addon-group")]
        public async Task<ActionResult> UpdateGroupAddOn(Guid GroupdId, string Name, string Url)
        {
            try
            {
                await _query.UpdateAddOnGroup(GroupdId, Name, Url);
                return Ok();
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
        /// Удаление списка донатов в админ панели
        /// </summary>
        /// <param name="GroupdId"></param>
        [Authorize(Roles = "Admin,Worker")]
        [HttpDelete("delete-group")]
        public async Task<ActionResult> DeleteAddOnGroup(Guid GroupdId)
        {
            try
            {
                await _query.DeleteAddOnGroup(GroupdId);
                return Ok();
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
        /// cписок донатов в группе
        /// </summary>
        /// <param name="GroupdId"></param>
        [Authorize(Roles = "Admin,Worker")]
        [HttpGet("add-on-in-group")]
        public async Task<ActionResult<List<AddOnsLst>>> AddOnsInGroup(Guid GroupdId)
        {
            try
            {
                var result = await _query.AddOnsInGroup(GroupdId);
                return Ok(result);
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
        /// cписок донатов в группе
        /// </summary>
        /// <param name="GroupdId"></param>
        /// <param name="ProductId"></param>
        [Authorize(Roles = "Admin,Worker")]
        [HttpGet("add-addon-in-group")]
        public async Task<ActionResult> AddAddOnInGroup(Guid ProductId, Guid GroupdId)
        {
            try
            {
                await _query.AddAddOnInGroup(ProductId, GroupdId);
                return Ok();
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
        /// cписок донатов в группе
        /// </summary>
        /// <param name="GroupdId"></param>
        /// <param name="ProductId"></param>
        [Authorize(Roles = "Admin,Worker")]
        [HttpDelete("delete-addon-in-group")]
        public async Task<ActionResult> DeleteAddOnFromGroup(Guid ProductId, Guid GroupdId)
        {
            try
            {
                await _query.DeleteAddOnFromGroup(ProductId, GroupdId);
                return Ok();
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
