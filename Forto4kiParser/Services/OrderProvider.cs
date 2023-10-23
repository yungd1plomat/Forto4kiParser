using Forto4kiParser.Abstractions;
using Forto4kiParser.Models;
using System.Collections.Concurrent;

namespace Forto4kiParser.Services
{
    public class OrderProvider : IOrderProvider
    {
        private readonly ILogger _logger;

        private readonly ConcurrentQueue<Order> _orderQueue;

        public OrderProvider(ILogger<OrderProvider> logger) 
        {
            _logger = logger;
            _orderQueue = new ConcurrentQueue<Order>();
        }

        private void Enqueue(Tyre tyre, Warehouse warehouse, int chunkSize, int chunkCount, int remainder)
        {
            for (int i = 0; i < chunkCount; i++)
            {
                _orderQueue.Enqueue(new Order()
                {
                    Tyre = tyre,
                    Warehouse = warehouse,
                    Quantity = chunkSize,
                });
            }
            if (remainder > 0)
            {
                _orderQueue.Enqueue(new Order() 
                {
                    Tyre = tyre,
                    Warehouse = warehouse,
                    Quantity = remainder,
                });
            }
            _logger.LogInformation($"Добавили в очередь {chunkCount + remainder} заказов по {chunkSize} шин {tyre.Sae}");
        }

        public void Enqueue(Tyre tyre, int chunkSize, int minCount, int maxCount)
        {
            // Есть склад где кол-во >40, заказываем сколько есть
            var warehouse = tyre.Warehouses.FirstOrDefault(x => !int.TryParse(x.Stock, out _));
            int.TryParse(warehouse?.Stock, out var quantity);
            if (warehouse != null || quantity >= maxCount)
            {
                int chunkCount = maxCount / chunkSize;
                int remainder = maxCount % chunkSize;
                Enqueue(tyre, warehouse, chunkSize, chunkCount, remainder);
                return;
            }

            // Заказываем все что есть на складах до максимального количества
            int remain = maxCount;
            foreach (var house in tyre.Warehouses)
            {
                int.TryParse(house.Stock, out var stock);
                if (stock > 0 && stock >= minCount)
                {
                    if (remain <= stock)
                    {
                        int chunks = 1;
                        int remains = 0;
                        Enqueue(tyre, house, stock, chunks, remains);
                        return;
                    }
                    int chunkCount = stock / chunkSize;
                    int remainder = stock % chunkSize;
                    Enqueue(tyre, house, chunkSize, chunkCount, remainder);
                    remain -= stock;
                }
            }
        }

        public Order? GetQueueOrder()
        {
            if (!_orderQueue.TryDequeue(out var order))
                return null;
            return order;
        }
    }
}
