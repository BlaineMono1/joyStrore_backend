
using Business.Data.Iterfaces;
using Business.Data.Iterfaces.Store;
using Business.Data.Models;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Service.Application.Service.SectionQuery.Dto;


namespace Service.Application.Service.SectionQuery
{
    public class SectionQuery
    {
        private readonly ILogger<SectionQuery> _logger;
        private readonly IRepository<Section> _sectionRepository;
        private readonly IProductRepository<Product> _productRepository;
        private readonly IRepository<Edition> _editionRepository;
        private readonly IRepository<SectionsProducts> _sectionsEditionsRepository;
        private readonly IRepository<AddOn> _addOnRepository;

        public SectionQuery(ILogger<SectionQuery> logger, IRepository<Section> sectionRepository, IProductRepository<Product> productRepository, 
            IRepository<SectionsProducts> sectionsEditionsRepository, IRepository<Edition> editionRepository, IRepository<AddOn> addOnRepository)
        {
            _logger = logger;
            _sectionRepository = sectionRepository;
            _productRepository = productRepository;
            _sectionsEditionsRepository = sectionsEditionsRepository;
            _editionRepository = editionRepository;
            _addOnRepository = addOnRepository;
        }


        public async Task CreateSections(string sectionName, string imagePath)
        {
            var section = new Section { Name = sectionName, FilePathImage = imagePath, Products = new List<SectionsProducts>() };

            await _sectionRepository.Add(section);
        }

        public async Task DeleteSection(Guid SectionId)
        {
            
            var section = (await _sectionRepository.GetListQuery()).Include(s => s.Products).First(s => s.Guid == SectionId);

            foreach(var del in section.Products)
            {
                await _sectionsEditionsRepository.HardDelete(del.Guid);
            }

            await _sectionRepository.HardDelete(SectionId);
        }


        public async Task UpdateSection(Guid SectionId,string SectionName)
        {
            var section = await _sectionRepository.GetById(SectionId);

            if (section is null) throw new Exception($"Section with GUid {SectionId} not found");

            section.Name = SectionName;

            await _sectionRepository.Update(section);
        }

        public async Task<List<SectionsDto>> SectionsList()
        {
            var sections = await _sectionRepository.GetListQuery();

            var result = sections.Select(item => new SectionsDto
            {
                SectionId = item.Guid,
                SectionName = item.Name
            }).ToList();

            return result;
        }

        public async Task<SectionDto> SectionById(Guid SectionId)
        {
            var section = (await _sectionRepository.GetListQuery()).Include(s => s.Products).ThenInclude(p => p.Product).First(s => s.Guid == SectionId);

            var result = new SectionDto
            {
                SectionId = section.Guid,
                SectionName = section.Name,
                Products = new List<ProductDto>()
            };

            // Список всех TypeId для одного запроса
            var typeIds = section.Products.Select(p => p.Product.TypeId).ToList();

            // Загружаем все Edition и AddOn одним запросом
            var editions = await (await _editionRepository.GetListQuery())
                .Where(e => typeIds.Contains(e.Guid))
                .ToDictionaryAsync(e => e.Guid, e => e.Name);

            var addOns = await (await _addOnRepository.GetListQuery())
                .Where(a => typeIds.Contains(a.Guid))
                .ToDictionaryAsync(a => a.Guid, a => a.Name);

            foreach (var item in section.Products)
            {
                var productName = item.Product.Type == "Game"
                    ? editions.GetValueOrDefault(item.Product.TypeId, "Unknown Edition")
                    : addOns.GetValueOrDefault(item.Product.TypeId, "Unknown AddOn");

                result.Products.Add(new ProductDto
                {
                    ProductId = item.ProductId,
                    ProductName = productName
                });
            }

                return result;
        }

        public async Task AddProductInSection(Guid SectionId, Guid ProductId)
        {
            var product = await _productRepository.GetById(ProductId);
            var section = (await _sectionRepository.GetListQuery()).Include(s => s.Products).AsTracking().First(s => s.Guid == SectionId);


            var q = new SectionsProducts
            { ProductId = ProductId, SectionId = SectionId };


            await _sectionsEditionsRepository.Add(q);

            
        }

        public async Task DeleteProductFromSection(Guid SectionId, Guid ProductId)
        {
            var delete = (await _sectionsEditionsRepository.GetListQuery()).First(se => se.SectionId == SectionId && se.ProductId == ProductId);

            await _sectionsEditionsRepository.HardDelete(delete.Guid);
        }

        public async Task<List<ProductDto>> FindProductByName(string Name, bool isEdition)
        {
            var result = new List<ProductDto>();
            if (isEdition)
            {
                var products = (await _productRepository.GetListQuery()).Include(p => p.Edition).Where(p => p.Type == "Game" && p.Edition.Name.Contains(Name));
                result = products.Select(item => new ProductDto
                {
                    ProductId = item.Guid,
                    ProductName = item.Edition.Name
                }).ToList();
            }
            else
            {
                var products = (await _productRepository.GetListQuery()).Include(p => p.AddOn).Where(p => p.Type == "AddOn" && p.AddOn.Name.Contains(Name));
                result = products.Select(item => new ProductDto
                {
                    ProductId = item.Guid,
                    ProductName = item.AddOn.Name

                }).ToList();
            }

            

            return result;
        }


        public async Task UpdateProduct(Guid ProductId, string Name, string ImagePath)
        {
            var product = await _productRepository.GetById(ProductId);

            if(product is null) throw new Exception($"Product with GUID {ProductId} not found");

            if (product.Type == "Game")
            {
                var edition = await _editionRepository.GetById(product.TypeId);

                if (edition is null) throw new Exception($"Edition with GUID {product.TypeId} not found");

                if (!string.IsNullOrEmpty(Name)) edition.Name = Name;

                if (!string.IsNullOrEmpty(ImagePath)) edition.Image = ImagePath;

                await _editionRepository.Update(edition);
            }
            else if(product.Type == "AddOn")
            {
                var addOn = await _addOnRepository.GetById(product.TypeId);

                if (addOn is null) throw new Exception($"AddOn with GUID {product.TypeId} not found");

                if (!string.IsNullOrEmpty(Name)) addOn.Name = Name;

                if (!string.IsNullOrEmpty(ImagePath)) addOn.Image = ImagePath;

                await _addOnRepository.Update(addOn);
            }
            else
            {
                throw new Exception($"Unknown product type {product.Type}");
            }
        }
    }
}
