using Forto4kiParser.Models;
using System;

namespace Forto4kiParser.Abstractions
{
    public interface ITelegramProvider
    {
        /// <summary>
        /// Получает сообщение из очереди на отправку
        /// </summary>
        MessageParams? GetQueueMsg();

        /// <summary>
        /// Форматирует и генерирует описание для
        /// для фото
        /// </summary>
        string GenerateDescription(Tyre tyre);

        /// <summary>
        /// Сериализует сообщение и
        /// добавляет в очередь на отправку
        /// </summary>
        void Enqueue(Tyre tyre);

        /// <summary>
        /// Сериализует заказ и добавляет в очередь на отправку
        /// </summary>
        /// <param name="tyre">Шина</param>
        /// <param name="isSuccess">Статус ответа</param>
        /// <param name="orderUrl">Url адрес заказа</param>
        /// <param name="quantity">Количество</param>
        void EnqueueOrder(Tyre tyre, bool isSuccess, string orderUrl, int quantity);

        /// <summary>
        /// Генерирует описание для заказа
        /// </summary>
        /// <param name="tyre">Шина</param>
        /// <param name="isSuccess">Статус ответа</param>
        /// <param name="orderUrl">Url адрес заказа</param>
        /// <param name="quantity">Количество</param>
        /// <returns></returns>
        string GenerateOrderDescription(Tyre tyre, bool isSuccess, string orderUrl, int quantity);
    }
}
