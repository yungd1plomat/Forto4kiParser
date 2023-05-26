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
    }
}
