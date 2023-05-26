using Forto4kiParser.Models;

namespace Forto4kiParser.Abstractions
{
    public interface IParserService
    {
        /// <summary>
        /// Получает все шины по заданному фильтру
        /// </summary>
        /// <param name="filter"></param>
        /// <returns></returns>
        Task<IEnumerable<Tyre>> GetTyres(Filter filter);
    }
}
