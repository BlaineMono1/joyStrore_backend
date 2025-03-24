
using Business.Data.Iterfaces;
using Business.Data.Iterfaces.Store;
using Business.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Service.Application.Service.SectionQuery.Dto;

namespace Service.Application.Service.SectionQuery
{
    public class SectionQuery
    {
        private readonly ILogger<SectionQuery> _logger;
        private readonly IRepository<Section> _sectionRepository;
        private readonly IRepository<Edition> _editionRepository;

        public SectionQuery(ILogger<SectionQuery> logger, IRepository<Section> sectionRepository, IRepository<Edition> editionRepository)
        {
            _logger = logger;
            _sectionRepository = sectionRepository;
            _editionRepository = editionRepository;
        }


        public async Task CreateSections(string sectionName, string imagePath)
        {
            var section = new Section { Name = sectionName, FilePathImage = imagePath, Editions = new List<Edition>() };

            await _sectionRepository.Add(section);
        }

        public async Task DeleteSection(Guid SectionId)
        {
            
            var section = (await _sectionRepository.GetListQuery()).Include(s => s.Editions).AsTracking().First(s => s.Guid == SectionId);

            section.Editions.Clear();

            await  _sectionRepository.Update(section);

            await _sectionRepository.HardDelete(SectionId);
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

        public async Task<Section> SectionById(Guid SectionId)
        {
            var result = (await _sectionRepository.GetListQuery()).Include(s => s.Editions).First(s => s.Guid == SectionId);


            return result;
        }

        public async Task AddGameInSection(Guid SectionId, Guid EditionId)
        {
            var section = (await _sectionRepository.GetListQuery()).Include(s => s.Editions).AsTracking().First(s => s.Guid == SectionId);

            var edition = (await _editionRepository.GetListQuery()).AsTracking().First(e => e.Guid == EditionId);

            section.Editions.Add(edition);

            await _sectionRepository.Update(section);
        }

        public async Task DeleteGameFromSection(Guid SectionId, Guid EditionId)
        {
            var section = (await _sectionRepository.GetListQuery()).Include(s => s.Editions).AsTracking().First(s => s.Guid == SectionId);

            var edition = (await _editionRepository.GetListQuery()).AsTracking().First(e => e.Guid == EditionId);

            if (section.Editions is null || !section.Editions.Any()) throw new Exception($"Section with GUID {SectionId} has no editions, but trying to delete one");

            if (!section.Editions.Remove(edition)) throw new Exception($"Section with GUID {SectionId} has no edition with GUID {EditionId}");

            await _sectionRepository.Update(section);
        }

        public async Task<List<EditionsDto>> FindEditionsByName(string EditionName)
        {
            var editions = (await _editionRepository.GetListQuery()).Where(e => e.EditionName.Contains(EditionName));

            var result = editions.Select(item => new EditionsDto
            {
                EditionId = item.Guid,
                EditionName = item.EditionName
            }).ToList();

            return result;
        }

        public async Task UpdateEdition(Guid EditionId, string Name, string ImagePath)
        {
            var edition = await _editionRepository.GetById(EditionId);

            if (edition == null) throw new Exception($"Edition with GUID {EditionId} not found");

            if(!string.IsNullOrEmpty(Name)) edition.EditionName = Name;

            if(!string.IsNullOrEmpty(ImagePath)) edition.Image = ImagePath;

            await _editionRepository.Update(edition);
        }
    }
}
