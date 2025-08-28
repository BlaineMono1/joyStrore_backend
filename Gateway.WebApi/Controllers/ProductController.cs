using Business.Data.Iterfaces.Store;
using Business.Data.Models;
using Gateway.WebApi.Attributes;
using Microsoft.AspNetCore.Mvc;
using Service.Application.Extension.Pagination;
using Service.Application.Service.ProductQuery;
using Service.Application.Service.ProductQuery.Dto;
using static Service.Application.Exceptions.NotFoundExeption;

namespace Gateway.WebApi.Controllers
{
    [SetRoute("product")]
    [ApiController]
    public class ProductController : ControllerBase
    {
        private readonly ProductQuery _productQuery;
        private readonly ILogger<ProductController> _logger;

        public ProductController(ProductQuery productQuery, ILogger<ProductController> logger)
        {
            _productQuery = productQuery;
            _logger = logger;
        }

        /// <summary>
        /// Вывод продукта по его Id
        /// </summary>
        [HttpGet("productById")]
        public async Task<ActionResult<ProductDto>> GetProduct(Guid ProductId)
        {
            try
            {
                var product = await _productQuery.GetProduct(ProductId);
                return Ok(product);
            }
            catch (NotFoundException ex)
            {
                _logger.LogError(ex.Message);
                return StatusCode(404, ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        /// <summary>
        /// Фильтрация продуктов
        /// </summary>
        [HttpPost("filter")]
        public async Task<ActionResult<List<ProductListDto>>> FilterProducts(
            string? name = null,
            string? filterName = null,
            string? platform = null,
            bool byDesc = false,
            bool byDiscount = false,
            List<string>? geners = null,
            int Page = 0,
            decimal MinPrice = 0,
            decimal MaxPrice = 1e18M
        )
        {
            try
            {
                var games = await _productQuery.FilterProducts(
                    name,
                    filterName,
                    platform,
                    byDesc,
                    byDiscount,
                    geners,
                    MinPrice,
                    MaxPrice
                );

                var result = await _productQuery.MapProducts(
                    new PaginatedList<Product>(games, Page).Entities
                );
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return StatusCode(500, ex.Message);
            }
        }

        /// <summary>
        /// Выпадающий список
        /// </summary>
        [HttpGet("drop-down-list")]
        public async Task<ActionResult<List<DropDownListDto>>> GetDropDownList(Guid productId)
        {
            try
            {
                var result = await _productQuery.DropDownList(productId);
                return Ok(result);
            }
            catch (NotFoundException ex)
            {
                _logger.LogError(ex.Message);
                return StatusCode(404, ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
    }
}
