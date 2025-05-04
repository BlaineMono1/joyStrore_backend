using Business.Data.Iterfaces;
using Business.Data.Models;
using Microsoft.Extensions.Logging;
using Service.Application.Exceptions;
using Service.Application.Service.GetNewsList.Dto;
using Service.Application.Service.NewsQuery.Dto;
using static Service.Application.Exceptions.NotFoundExeption;

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
            var news = await _newsRepository.GetListQuery();
            if (news is null)
            {
                _logger.LogError("News is empty");
                return result;
            }
            result.AddRange(news.Select(el => new NewsDto
            {
                Url = el.Link,
                ImagePath = el.FilePathImage
            }));

            return result;
        }

        public async Task<List<NewsListDto>> GetNewsListAdminPanel()
        {
            var news = await _newsRepository.GetListQuery();
            if (news is null)
            {
                _logger.LogError("News is empty");

                return new List<NewsListDto>();
            }

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

        public async Task DeleteNews(Guid NewsId)
        {           
            await _newsRepository.HardDelete(NewsId);
        }

        public async Task UpdateNews(Guid NewsId, string? Name, string? Url, string? Image)
        {
            var current = await _newsRepository.GetById(NewsId);

            if (current is null) throw new NotFoundException(nameof(News), NewsId);

            if(!string.IsNullOrEmpty(Name)) current.Name = Name;

            if (!string.IsNullOrEmpty(Url)) current.Link = Url;

            if (!string.IsNullOrEmpty(Image)) current.FilePathImage = Image;

            await _newsRepository.Update(current);
        }

        public async Task<NewsListDto> GetNewsById(Guid NewsId)
        {
            var current = await _newsRepository.GetById(NewsId);
            if (current is null) throw new NotFoundException(nameof(News), NewsId);

            return new NewsListDto { NewsId = current.Guid, NewsName = current.Name, Url = current.FilePathImage };
        }
    }
}
