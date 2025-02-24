using Business.Data.Iterfaces;
using Business.Data.Models;
using Microsoft.Extensions.Logging;
using Service.Application.Service.GetNewsList.Dto;

namespace Service.Application.Service.GetNewsList
{
    public class NewsQuery
    {
        private readonly IRepository<News> _newsRepository;

        private readonly ILogger<NewsQuery> _logger;
        public NewsQuery(ILogger<NewsQuery> logger, IRepository<News> newsRepository)
        {
            _logger = logger;
            _newsRepository = newsRepository;
        }

        public async Task<List<NewsDto>> GetNewsList()
        {
            var result = new List<NewsDto>();
            try
            {
                var news = await _newsRepository.GetAllList();
                if (news is null) _logger.LogWarning("News is empty");
                result.AddRange(news.Select(el => new NewsDto
                {
                    Url = el.Link,
                    ImagePath = el.FilePathImage
                }));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving the news list.");
                throw;
            }
            return result;
        }
    }
}
