
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
        private readonly IRepository<SectionsEditions> _sectionsEditionsRepository;

        public SectionQuery(ILogger<SectionQuery> logger, IRepository<Section> sectionRepository, IRepository<Edition> editionRepository, IRepository<SectionsEditions> sectionsEditionsRepository)
        {
            _logger = logger;
            _sectionRepository = sectionRepository;
            _editionRepository = editionRepository;
            _sectionsEditionsRepository = sectionsEditionsRepository;
        }


        public async Task CreateSections(string sectionName, string imagePath)
        {
            var section = new Section { Name = sectionName, FilePathImage = imagePath, Editions = new List<SectionsEditions>() };

            await _sectionRepository.Add(section);
        }

        public async Task DeleteSection(Guid SectionId)
        {
            
            var section = (await _sectionRepository.GetListQuery()).Include(s => s.Editions).AsTracking().First(s => s.Guid == SectionId);

            foreach(var del in section.Editions)
            {
                await _sectionsEditionsRepository.HardDelete(del.Guid);
            }

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
            var edition = await _editionRepository.GetById(EditionId);
            var section = (await _sectionRepository.GetListQuery()).Include(s => s.Editions).AsTracking().First(s => s.Guid == SectionId);


            var q = new SectionsEditions
            { EdtitonId = EditionId, SectionId = SectionId };


            await _sectionsEditionsRepository.Add(q);

            
        }

        public async Task DeleteGameFromSection(Guid SectionId, Guid EditionId)
        {
            var delete = (await _sectionsEditionsRepository.GetListQuery()).First(se => se.SectionId == SectionId && se.EdtitonId == EditionId);

            await _sectionsEditionsRepository.HardDelete(delete.Guid);
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
