using Auth.WebApi.Attributes;
using Business.Data.Models;
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

        [HttpPut("AddGame")]

        public async Task<ActionResult> AddGame(Guid SectionId, Guid EditionId)
        {
            try
            {
                await _query.AddGameInSection(SectionId, EditionId);
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return StatusCode(500, ex.Message);
            }
        }

        [HttpDelete("DeleteGame")]

        public async Task<ActionResult> DeleteGame(Guid SectionId, Guid EditionId)
        {
            try
            {
                await _query.DeleteGameFromSection(SectionId, EditionId);
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return StatusCode(500, ex.Message);
            }
        }

        [HttpGet("GetEditionsList")]

        public async Task<ActionResult<List<EditionsDto>>> GetEditionsList(string Name = "")
        {
            try
            {
                var result = await _query.FindEditionsByName(Name);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPut("UpdateEdition")]
        public async Task<ActionResult> UpdateEdition(Guid EditionId, string Name, string url)
        {
            try
            {
                await _query.UpdateEdition(EditionId, Name, url);
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
