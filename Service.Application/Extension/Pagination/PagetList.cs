using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service.Application.Extension.Pagination
{
    /// <summary>
    /// Список элементов разбитый на страницы
    /// </summary>
    public class PaginatedList<T>
    {
        /// <summary>
        /// Номер страницы
        /// </summary>
        public int PageIndex { get; private set; }

        /// <summary>
        /// Кол-во элементов на странице
        /// </summary>
        public int PageSize { get; private set; }

        /// <summary>
        /// Общее кол-во элементов
        /// </summary>
        public int TotalCount { get; private set; }

        /// <summary>
        /// Общее кол-во страниц
        /// </summary>
        public int TotalPages { get; private set; }

        /// <summary>
        /// Список элементов
        /// </summary>
        public List<T> Entities { get; private set; }

        public PaginatedList(IQueryable<T> source, int pageIndex)
        {
            PageIndex = pageIndex;
            PageSize = 20;
            TotalCount = source.Count();
            TotalPages = (int)Math.Ceiling(TotalCount / (double)PageSize);

            Entities = source.Skip(PageIndex * PageSize).Take(PageSize).ToList();
        }

        /// <summary>
        /// Есть ли первая страница
        /// </summary>
        public bool HasPreviousPage
        {
            get
            {
                return (PageIndex > 0);
            }
        }

        /// <summary>
        /// Есть ли следующая страница
        /// </summary>
        public bool HasNextPage
        {
            get
            {
                return (PageIndex + 1 < TotalPages);
            }
        }

        /// <summary>
        /// Получение страницы с записями
        /// </summary>
        /// <param name="entities">Последовательность записей</param>
        /// <param name="page">Номер страницы</param>
        /// <returns></returns>
        protected PaginatedList<T> GetPageEntities(IQueryable<T> entities, int page = 0)
        {
            var result = new PaginatedList<T>(entities, page);
            return result;
        }

    }


}
