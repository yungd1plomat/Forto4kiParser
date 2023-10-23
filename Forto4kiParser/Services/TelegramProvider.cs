using Forto4kiParser.Abstractions;
using Forto4kiParser.Helpers;
using Forto4kiParser.Models;
using Microsoft.Extensions.Primitives;
using System.Collections.Concurrent;
using System.Text;
using System.Xml;

namespace Forto4kiParser.Services
{
    public class TelegramProvider : ITelegramProvider
    {
        private readonly string _channelId;

        private readonly ConcurrentQueue<MessageParams> _messageQueue;

        public TelegramProvider(string channelId)
        {
            _channelId = channelId;
            _messageQueue = new ConcurrentQueue<MessageParams>();
        }

        public void Enqueue(Tyre tyre)
        {
            var description = GenerateDescription(tyre);
            var photoUrl = tyre.PhotoUrl;
            var messageParams = new MessageParams()
            {
                ChatId = _channelId,
                Description = description,
                Photo = photoUrl,
            };
            _messageQueue.Enqueue(messageParams);
        }

        public void EnqueueOrder(Tyre tyre, bool isSuccess, string orderUrl, int quantity)
        {
            var description = GenerateOrderDescription(tyre, isSuccess, orderUrl, quantity);
            var photoUrl = tyre.PhotoUrl;
            var messageParams = new MessageParams()
            {
                ChatId = _channelId,
                Description = description,
                Photo = photoUrl,
            };
            _messageQueue.Enqueue(messageParams);
        }

        public string GenerateOrderDescription(Tyre tyre, bool isSuccess, string orderUrl, int quantity)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("*Новый заказ*");

            sb.AppendLine();
            sb.AppendLine();

            sb.Append("*");
            sb.Append(tyre.Brand.Escape());
            sb.AppendLine();
            sb.Append(tyre.Name.Escape());
            sb.Append("*");

            sb.AppendLine();
            sb.AppendLine();

            sb.Append("*Успешно*: ");
            sb.Append(isSuccess);

            sb.AppendLine();
            sb.AppendLine();

            sb.Append("*Количество*: ");
            sb.Append(quantity);

            sb.AppendLine();
            sb.AppendLine();

            sb.Append($"[Ссылка]({orderUrl})");
            return sb.ToString();
        }

        public string GenerateDescription(Tyre tyre)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("*");
            sb.Append(tyre.Brand.Escape());
            sb.AppendLine();
            sb.Append(tyre.Name.Escape());
            sb.Append("*");

            sb.AppendLine();
            sb.AppendLine();

            sb.Append("*Сезонность*: ");
            sb.Append(tyre.Description);

            foreach (var warehouse in tyre.Warehouses)
            {
                sb.AppendLine();
                sb.AppendLine();

                sb.Append("*Склад \\- ");
                sb.Append(warehouse.Name.ToUpper().Escape());
                sb.Append("*");

                sb.AppendLine();

                sb.Append("*Интернет цена: *");
                sb.Append(warehouse.InternetPrice);

                sb.AppendLine();

                sb.Append("*Ваша цена: *");
                sb.Append(warehouse.SupplierPrice);

                sb.AppendLine();

                sb.Append("*Остаток: *");
                sb.Append(warehouse.Stock.Escape());
            }
            sb.AppendLine();
            sb.AppendLine();
            sb.Append($"[Ссылка](https://b2b.4tochki.ru/Product/Tire?fc_c={tyre.Sae}&fc_wh=232&fc_wh=9&fc_wh=3&fc_wh=4&fc_wh=1)");
            return sb.ToString();
        }

        public MessageParams? GetQueueMsg()
        {
            if (!_messageQueue.TryDequeue(out var message))
                return null;
            return message;
        }
    }
}
