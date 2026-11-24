// Models/InMemoryOrderConnector.cs
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AQUA.Models;

namespace AQUA.Models
{
    public class InMemoryOrderConnector : IOrderConnector
    {
        private readonly Queue<OrderInfo> _orders = new Queue<OrderInfo>();
        private readonly object _lock = new object();

        public Task<OrderInfo> GetNextOrder()
        {
            lock (_lock)
            {
                if (_orders.Count == 0)
                    return Task.FromResult<OrderInfo>(null);

                return Task.FromResult(_orders.Dequeue());
            }
        }

        public Task AddOrder(OrderInfo order)
        {
            lock (_lock)
            {
                order.OrderId = Guid.NewGuid().ToString();
                _orders.Enqueue(order);
                Console.WriteLine($"Order added: {order.OrderId} for {order.CustomerEmail}");
            }
            return Task.CompletedTask;
        }

        public Task RemoveOrder(OrderInfo order)
        {
            // In this simple implementation, order is already dequeued in GetNextOrder
            // We just log the removal
            Console.WriteLine($"Order removed/processed: {order.OrderId}");
            return Task.CompletedTask;
        }

        public int GetQueueCount()
        {
            lock (_lock)
            {
                return _orders.Count;
            }
        }

        public List<OrderInfo> GetAllOrders()
        {
            lock (_lock)
            {
                return new List<OrderInfo>(_orders);
            }
        }
    }
}