using Gateway.WebApi.Attributes;
using Microsoft.AspNetCore.Mvc;
using Service.Application.Service.AddOnsQuery;
using Services.ParseService;

namespace Gateway.WebApi.Controllers
{
    [SetRoute("api/[controller]/[action]")]
    [ApiController]
    public class FillDataBase : ControllerBase
    {
        private readonly Parse _parse;

        public FillDataBase(Parse parse)
        {
            _parse = parse;
        }

        [HttpGet]
        public async Task<ActionResult> FillBd()
        {
            try
            {
                await _parse.StartParse();
                return Ok();
            }
            catch (Exception ex)
            {                
                return StatusCode(500, "An error occurred while parsing");
            }
        }

        [HttpGet]
        public async Task<ActionResult> RegUser()
        {
            try
            {
                await _parse.RegUser();
                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, "An error occurred while reg user");
            }
        }

        [HttpGet]
        public async Task<ActionResult> CreateSection()
        {
            try
            {
                await _parse.CreateSections();
                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, "An error occurred while create sections");
            }
        }


        [HttpGet]
        public async Task<ActionResult> CreateSub()
        {
            try
            {
                await _parse.CreateSubs();
                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, "An error occurred while create sections");
            }
        }


        [HttpGet]
        public async Task<ActionResult> CreateGamesMarkUp()
        {
            try
            {
                await _parse.CreateGameMarcup();
                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, "An error occurred while parsing");
            }
        }

    }
}
