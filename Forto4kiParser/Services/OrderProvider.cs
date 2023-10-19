using Forto4kiParser.Abstractions;
using Forto4kiParser.Models;
using System.Collections.Concurrent;

namespace Forto4kiParser.Services
{
    public class OrderProvider : IOrderProvider
    {
        private readonly ILogger _logger;

        private readonly ConcurrentQueue<KeyValuePair<Tyre, int>> _orderQueue;

        public OrderProvider(ILogger<OrderProvider> logger) 
        {
            _logger = logger;
            _orderQueue = new ConcurrentQueue<KeyValuePair<Tyre, int>>();
        }

        private void Enqueue(Tyre tyre, int chunkSize, int chunkCount, int remainder)
        {
            for (int i = 0; i < chunkCount; i++)
            {
                _orderQueue.Enqueue(new KeyValuePair<Tyre, int>(tyre, chunkSize));
            }
            if (remainder > 0)
            {
                _orderQueue.Enqueue(new KeyValuePair<Tyre, int>(tyre, remainder));
            }
            _logger.LogInformation($"Добавили в очередь {chunkCount + remainder} заказов по {chunkSize} шин {tyre.Sae}");
        }

        public void Enqueue(Tyre tyre, int chunkSize, int maxCount)
        {
            // Есть склад где кол-во >40, заказываем сколько есть
            if (tyre.Warehouses.Any(x => !int.TryParse(x.Stock, out var quantity) || quantity >= maxCount))
            {
                int chunkCount = maxCount / chunkSize;
                int remainder = maxCount % chunkSize;
                Enqueue(tyre, chunkSize, chunkCount, remainder);
                return;
            }
            int remain = maxCount;
            // Заказываем все что есть на складах до максимального количества
            foreach (var warehouse in tyre.Warehouses)
            {
                int.TryParse(warehouse.Stock, out var quantity);
                if (quantity > 0)
                {
                    if (remain <= quantity)
                    {
                        Enqueue(tyre, quantity, 1, 0);
                        return;
                    }
                    int chunkCount = quantity / chunkSize;
                    int remainder = quantity % chunkSize;
                    Enqueue(tyre, chunkSize, chunkCount, remainder);
                    remain -= quantity;
                }
            }
        }

        public KeyValuePair<Tyre, int>? GetQueueOrder()
        {
            if (!_orderQueue.TryDequeue(out var order))
                return null;
            return order;
        }
    }
}
