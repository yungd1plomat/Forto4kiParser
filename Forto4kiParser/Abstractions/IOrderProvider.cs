using Forto4kiParser.Models;

namespace Forto4kiParser.Abstractions
{
    public interface IOrderProvider
    {
        /// <summary>
        /// Получает шину из очереди на заказ
        /// </summary>
        /// <returns>
        /// Объект Заказ
        /// </returns>
        Order? GetQueueOrder();

        /// <summary>
        /// Добавляет шину на заказ в очередь
        /// </summary>
        /// <param name="tyre">Шина которую необходимо заказать</param>
        /// <param name="chunkSize">Максимальное количество в заказе</param>
        /// <param name="minCount">Минимальное количество в заказе</param>
        /// <param name="maxCount">Максимальное количество заказов</param>
        void Enqueue(Tyre tyre, int chunkSize, int minCount, int maxCount);
    }
}
