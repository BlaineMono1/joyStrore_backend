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
        public async Task<ActionResult> ParseGames(int startPage, int endPage)
        {
            try
            {
                await _parse.ParseGames(startPage, endPage);
                return Ok();
            }
            catch (Exception ex)
            {                
                return StatusCode(500, ex.Message);
            }
        }

        [HttpGet]
        public async Task<ActionResult<string>> ParseAddOns(int startPage = 1, int endPage = 2)
        {
            try
            {
                var result = await _parse.ParseAddOns(startPage, endPage);
                return Ok(result);
            }
            catch(Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpGet]
        public async Task<ActionResult> UpdateProductsPrice()
        {
            try
            {
                await _parse.UpdateProductsPrice();
                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

    }
}
