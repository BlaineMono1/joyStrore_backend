using Business.Data.Models;
using DataBaseToAccess.Repositiory;
using Service.Application.Service.GetNewsList.Dto;

namespace Service.Application.Service.GetNewsList
{
    public class NewsQuery
    {
        private readonly Repository<News> _newsRepository;
        public async Task<List<NewsDto>> GetNewsList()
        {
            var news = await _newsRepository.GetAllList();
            var result = new List<NewsDto>();
            result.AddRange(news.Select(el => new NewsDto
            {
                Url = el.Link,
                ImagePath = el.FilePathImage
            }));

            return result;
        }
    }
}
