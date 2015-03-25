using System.Collections.Generic;

namespace Sandbox
{
    /// <summary>
    /// Pagination model
    /// </summary>
    /// <summary lang="ru">
    /// Страница списка
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class PagingResponseModel<T>
    {
        /// <summary>
        /// Underlining objects collection
        /// </summary>
        /// <summary lang="ru">
        /// Элемнты списка 
        /// </summary>
        public IEnumerable<T> Items { get; set; }

        /// <summary>
        /// Number of current page
        /// </summary>
        /// <summary lang="ru">
        /// Номер страницы
        /// </summary>
        public int Page { get; set; }

        /// <summary>
        /// Total number of avaliable pages
        /// </summary>
        /// <summary lang="ru">
        /// Общее количество страниц
        /// </summary>
        public int TotalPages { get; set; }

        /// <summary>
        /// Total objects count
        /// </summary>
        /// <summary lang="ru">
        /// Общее количество элементов  в списке
        /// </summary>
        public int TotalCount { get; set; }


        /// <summary>
        /// Test
        /// </summary>
        public void Reset(IDictionary<string, int> values)
        {

        }
    }
}