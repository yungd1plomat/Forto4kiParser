using Forto4kiParser.Models;

namespace Forto4kiParser.Abstractions
{
    public interface IOrderProvider
    {
        /// <summary>
        /// Получает шину из очереди на заказ
        /// </summary>
        /// <returns>
        /// Объект Шина - количество
        /// </returns>
        Task<KeyValuePair<Tyre, int>> GetQueueOrder();

        /// <summary>
        /// Добавляет шину на заказ в очередь
        /// </summary>
        /// <param name="tyre"></param>
        /// <param name="chunkCount"></param>
        /// <param name="maxCount"></param>
        void Enqueue(Tyre tyre, int chunkCount, int maxCount);
    }
}
