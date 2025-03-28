using Business.Data.Iterfaces;
using Business.Data.Models;
using Microsoft.Extensions.Logging;
using Service.Application.Service.GetNewsList.Dto;
using Service.Application.Service.NewsQuery.Dto;

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
                var news = await _newsRepository.GetListQuery();
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

        public async Task<List<NewsListDto>> GetNewsListAdminPanel()
        {
            var news = await _newsRepository.GetListQuery();
            if (news is null) _logger.LogWarning("News is empty");

            var result = news.Select(item => new NewsListDto
            {
                NewsId = item.Guid,
                NewsName = item.Name,
                Url = item.Link
            }).ToList();

            return result;
        }

        public async Task CreateNews(string Name, string Url, string Image)
        {
            var entity = new News
            {
                Name = Name,
                Link = Url,
                FilePathImage = Image
            };

            await _newsRepository.Add(entity);
        }

    }
}
