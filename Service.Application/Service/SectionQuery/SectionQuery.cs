using Business.Data.Iterfaces;
using Business.Data.Iterfaces.Store;
using Business.Data.Models;
using Microsoft.AspNetCore.DataProtection.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Service.Application.Exceptions;
using Service.Application.Service.SectionQuery.Dto;
using static Service.Application.Exceptions.NotFoundExeption;


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
        private readonly IRepository<GroupAddOn> _groupAddOnRepository;

        public SectionQuery(ILogger<SectionQuery> logger, IRepository<Section> sectionRepository, IProductRepository<Product> productRepository, 
            IRepository<SectionsProducts> sectionsEditionsRepository, IRepository<Edition> editionRepository, IRepository<AddOn> addOnRepository,
            IRepository<GroupAddOn> groupAddOnRepository)
        {
            _logger = logger;
            _sectionRepository = sectionRepository;
            _productRepository = productRepository;
            _sectionsEditionsRepository = sectionsEditionsRepository;
            _editionRepository = editionRepository;
            _addOnRepository = addOnRepository;
            _groupAddOnRepository = groupAddOnRepository;
        }


        public async Task CreateSections(string sectionName, string imagePath)
        {
            var section = new Section { Name = sectionName, FilePathImage = imagePath, Products = new List<SectionsProducts>() };

            await _sectionRepository.Add(section);
        }

        public async Task DeleteSection(Guid SectionId)
        {
            
            var section = (await _sectionRepository.GetListQuery()).Include(s => s.Products).FirstOrDefault(s => s.Guid == SectionId);

            if (section is null) throw new NotFoundException(nameof(Section), SectionId);

            foreach (var del in section.Products)
            {
                await _sectionsEditionsRepository.HardDelete(del.Guid);
            }

            await _sectionRepository.HardDelete(SectionId);
        }


        public async Task UpdateSection(Guid SectionId,string SectionName)
        {
            var section = await _sectionRepository.GetById(SectionId);

            if (section is null) throw new NotFoundException(nameof(Section), SectionId);

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
            var section = (await _sectionRepository.GetListQuery()).Include(s => s.Products).ThenInclude(p => p.Product).FirstOrDefault(s => s.Guid == SectionId);

            if (section is null) throw new NotFoundException(nameof(Section), SectionId);

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
            var section = (await _sectionRepository.GetListQuery()).Include(s => s.Products).AsTracking().FirstOrDefault(s => s.Guid == SectionId);

            if (product is null) throw new NotFoundException(nameof(Product), ProductId);
            if (section is null) throw new NotFoundException(nameof(Section), SectionId);

            var q = new SectionsProducts
            { ProductId = ProductId, SectionId = SectionId };


            await _sectionsEditionsRepository.Add(q);

            
        }

        public async Task DeleteProductFromSection(Guid SectionId, Guid ProductId)
        {
            var delete = (await _sectionsEditionsRepository.GetListQuery()).FirstOrDefault(se => se.SectionId == SectionId && se.ProductId == ProductId);

            if (delete is null) throw new NotFoundException(nameof(SectionsProducts), SectionId);

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

                if (edition is null) throw new NotFoundException(nameof(Edition), product.TypeId);

                if (!string.IsNullOrEmpty(Name)) edition.Name = Name;

                if (!string.IsNullOrEmpty(ImagePath)) edition.Image = ImagePath;

                await _editionRepository.Update(edition);
            }
            else if(product.Type == "AddOn")
            {
                var addOn = await _addOnRepository.GetById(product.TypeId);

                if (addOn is null) throw new NotFoundException(nameof(AddOn), product.TypeId);

                if (!string.IsNullOrEmpty(Name)) addOn.Name = Name;

                if (!string.IsNullOrEmpty(ImagePath)) addOn.Image = ImagePath;

                await _addOnRepository.Update(addOn);
            }
            else
            {
                throw new NotFoundException(nameof(product.Type), product.TypeId);
            }
        }

        public async Task<List<AddOnSectionList>> GetAddOnsGroups()
        {
            var lst = await _groupAddOnRepository.GetAllList(); 

            var result = new List<AddOnSectionList>();

            result.AddRange(lst.Select(item => new AddOnSectionList
            {
                GroupId = item.Guid,
                Name = item.Name
            }));

            return result;
        }

        public async Task CreateAddOnGroup(string Name, string Url)
        {
            var g = new GroupAddOn
            {
                Name = Name,
                FilePathImage = Url,
                AddOns = new List<AddOn>()
            };

            await _groupAddOnRepository.Add(g);
        }

        public async Task DeleteAddOnGroup(Guid GroupId)
        {
            var g = (await _groupAddOnRepository.GetListQuery()).Include(gr => gr.AddOns).FirstOrDefault(gr => gr.Guid == GroupId)
                    ?? throw new NotFoundException(nameof(GroupAddOn), GroupId);

            await _groupAddOnRepository.HardDelete(GroupId);
        }

        public async Task UpdateAddOnGroup(Guid GroupId, string Name, string Url)
        {
            var g = await _groupAddOnRepository.GetById(GroupId) ?? throw new NotFoundException(nameof(GroupAddOn), GroupId);

            g.Name = Name;
            g.FilePathImage = Url;

            await _groupAddOnRepository.Update(g);
        }

        public async Task<List<AddOnsLst>> AddOnsInGroup(Guid GroupId)
        {
            var g = (await _groupAddOnRepository.GetListQuery()).Include(gr => gr.AddOns).FirstOrDefault(gr => gr.Guid == GroupId) 
                ?? throw new NotFoundException(nameof(GroupAddOn), GroupId);

            var result = new List<AddOnsLst>();

            result.AddRange(g.AddOns.Select(item => new AddOnsLst
            {
                AddOnId = item.Guid,
                Name = item.Name
            }));

            return result;
        }

        public async Task DeleteAddOnFromGroup(Guid ProductId, Guid GroupId)
        {
            var group = (await _groupAddOnRepository.GetListQuery()).Include(gr => gr.AddOns).FirstOrDefault(gr => gr.Guid == GroupId)
               ?? throw new NotFoundException(nameof(GroupAddOn), GroupId);

            var product = await _productRepository.GetById(ProductId) ?? throw new NotFoundException(nameof(Product), ProductId); ;

            var addOn = await _addOnRepository.GetById(product.TypeId) ?? throw new NotFoundException(nameof(AddOn), product.TypeId);

            if (addOn.GroupAddOnId != null && addOn.GroupAddOnId != GroupId) throw new BadRequestExeption("Add on not in this group");

            addOn.GroupAddOnId = null;

            await _addOnRepository.Update(addOn);
        }

        public async Task AddAddOnInGroup(Guid ProductId, Guid GroupId)
        {
            var g = (await _groupAddOnRepository.GetListQuery()).Include(gr => gr.AddOns).FirstOrDefault(gr => gr.Guid == GroupId)
              ?? throw new NotFoundException(nameof(GroupAddOn), GroupId);

            var product = await _productRepository.GetById(ProductId) ?? throw new NotFoundException(nameof(Product), ProductId); ;

            var addOn = await _addOnRepository.GetById(product.TypeId) ?? throw new NotFoundException(nameof(AddOn), product.TypeId);

            if (addOn.GroupAddOnId != null) throw new BadRequestExeption($"Addon alredy in group {addOn.GroupAddOnId}");

            addOn.GroupAddOnId = GroupId;

            await _addOnRepository.Update(addOn);
        }
    }
}
