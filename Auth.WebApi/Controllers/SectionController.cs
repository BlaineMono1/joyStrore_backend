using Auth.WebApi.Attributes;
using Business.Data.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Service.Application.Service.SectionQuery;
using Service.Application.Service.SectionQuery.Dto;

namespace Auth.WebApi.Controllers
{
    [ApiController]
    [SetRoute("Sections")]
    public class SectionController : ControllerBase
    {
        private readonly ILogger<SectionController> _logger;
        private readonly SectionQuery _query;

        public SectionController(ILogger<SectionController> logger, SectionQuery query)
        {
            _logger = logger;
            _query = query;
        }

        [Authorize(Roles = "Admin,Worker")]
        [HttpGet("CreateSection")]
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

        [Authorize(Roles = "Admin,Worker")]
        [HttpPut("UpdateSection")]
        public async Task<ActionResult> UpdateSection(Guid SectionId, string SectionName)
        {
            try
            {
                await _query.UpdateSection(SectionId, SectionName);
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return StatusCode(500, ex.Message);
            }
        }

        [Authorize(Roles = "Admin,Worker")]
        [HttpDelete("DeleteSection")]
        public async Task<ActionResult> DeleteSections(Guid SectionId)
        {
            try
            {
                await _query.DeleteSection(SectionId);
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return StatusCode(500, ex.Message);
            }
        }

        [Authorize(Roles = "Admin,Worker")]
        [HttpGet("GetSectionsList")]
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

        [Authorize(Roles = "Admin,Worker")]
        [HttpGet("GetSection")]

        public async Task<ActionResult<Section>> GetSection(Guid SectionId)
        {
            try
            {
                var result = await _query.SectionById(SectionId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return StatusCode(500, ex.Message);
            }
        }

        [Authorize(Roles = "Admin,Worker")]
        [HttpPut("AddProduct")]

        public async Task<ActionResult> AddProduct(Guid SectionId, Guid ProductId)
        {
            try
            {
                await _query.AddProductInSection(SectionId, ProductId);
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return StatusCode(500, ex.Message);
            }
        }

        [Authorize(Roles = "Admin,Worker")]
        [HttpDelete("DeleteProduct")]

        public async Task<ActionResult> DeleteProduct(Guid SectionId, Guid ProductId)
        {
            try
            {
                await _query.DeleteProductFromSection(SectionId, ProductId);
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return StatusCode(500, ex.Message);
            }
        }

        [Authorize(Roles = "Admin,Worker")]
        [HttpGet("GetProductList")]

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

        [Authorize(Roles = "Admin,Worker")]
        [HttpPut("UpdateProduct")]
        public async Task<ActionResult> UpdateProduct(Guid ProductId, string Name, string url)
        {
            try
            {
                await _query.UpdateProduct(ProductId, Name, url);
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return StatusCode(500, ex.Message);
            }
        }
    }
}
