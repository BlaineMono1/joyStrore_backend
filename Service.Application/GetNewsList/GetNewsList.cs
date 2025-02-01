using Business.Data.Models;
using DataBaseToAccess.Repositiory.RepositoryEntity;
using System.Collections.Generic;

namespace Service.Application.GetNewsList
{
    public class GetNewsList
    {
        private readonly NewsRepository<News> _newsRepository; 
        public async Task<List<NewsDto>> GetNews()
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
