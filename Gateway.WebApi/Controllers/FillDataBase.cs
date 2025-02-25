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
    }
}
