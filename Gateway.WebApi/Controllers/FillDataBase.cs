using System.Text;
using System.Xml;
using Gateway.WebApi.Attributes;
using Microsoft.AspNetCore.Mvc;
using Service.Application.Exceptions;
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

        [HttpGet("test")]
        public async Task<ActionResult> Test(string value)
        {
            string url = "https://www.cbr.ru/scripts/XML_daily.asp";

            using (HttpClient client = new HttpClient())
            {
                try
                {
                    byte[] rawBytes = await client.GetByteArrayAsync(url);

                    string xmlContent = Encoding.Default.GetString(rawBytes);

                    XmlDocument doc = new XmlDocument();
                    doc.LoadXml(xmlContent);

                    XmlNode node = doc.SelectSingleNode($"//Valute[CharCode='{value}']");

                    if (node != null)
                    {
                        string unitRateStr = node["VunitRate"]?.InnerText;

                        var result = Convert.ToDecimal(unitRateStr);
                        return Ok(result);
                    }
                    else
                    {
                        throw new Exception($"Валюта {value} не найдена");
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception($"Ошибка получения курса валюты {value}: {ex.Message}");
                }
            }
            throw new Exception($"Не удалось получить курс для валюты {value}");
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

        [HttpPost]
        public async Task<ActionResult<Guid>> CreateGame(
            string ConceptId,
            string Name,
            string Languages,
            string Popular
        )
        {
            try
            {
                var result = await _parse.CreateGame(ConceptId, Name, Languages, Popular);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPost]
        public async Task<ActionResult<Guid>> CreateEdition(
            string CusaCodeUa,
            string CusaCodeTr,
            string Type,
            string Name,
            string EditionType,
            string Image,
            string Platform,
            string? Subscription,
            string? Features,
            DateTime Release,
            string Region,
            bool IsPreOrderr,
            decimal PriceUa,
            decimal PriceTr,
            Guid GameId,
            List<string> Geners
        )
        {
            try
            {
                var result = await _parse.CreateEdition(
                    CusaCodeUa,
                    CusaCodeTr,
                    Type,
                    Name,
                    EditionType,
                    Image,
                    Platform,
                    Subscription,
                    Features,
                    Release,
                    Region,
                    IsPreOrderr,
                    PriceUa,
                    PriceTr,
                    GameId,
                    Geners
                );
                return Ok(result);
            }
            catch (BadRequestExeption ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPost]
        public async Task<ActionResult<Guid>> CreateAddOn(
            string CusaCodeUa,
            string CusaCodeTr,
            string TypeName,
            string Name,
            string Type,
            string Image,
            string Platform,
            Guid GameId,
            decimal PriceUa,
            decimal PriceTr,
            string DiscountPercentUa,
            string DiscountPercentTr,
            DateTime? DiscountDateUa,
            DateTime? DiscountDateTr
        )
        {
            try
            {
                var result = await _parse.CreateAddOn(
                    CusaCodeUa,
                    CusaCodeTr,
                    TypeName,
                    Name,
                    Type,
                    Image,
                    Platform,
                    GameId,
                    PriceUa,
                    PriceTr
                );
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPost]
        public async Task<ActionResult<Guid>> CreateSub(
            string CusaCodeUa,
            string CusaCodeTr,
            string Name,
            string Type,
            string Image,
            string ImageLayout,
            string Platform,
            string Duration,
            string SectionName,
            decimal PriceUa,
            decimal PriceTr
        )
        {
            try
            {
                var result = await _parse.CreateSub(
                    CusaCodeUa,
                    CusaCodeTr,
                    Name,
                    Type,
                    Image,
                    ImageLayout,
                    Platform,
                    Duration,
                    SectionName,
                    PriceUa,
                    PriceTr
                );
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
    }
}
