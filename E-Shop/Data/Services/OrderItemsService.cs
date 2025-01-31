using DbToolkit;
using DbToolkit.Enums;
using DbToolkit.Filtering;
using E_Shop.Models;
using E_Shop.Services;
using System.Data;

namespace E_Shop.Data.Services
{
    internal class OrderItemsService : IOrderItemsService
    {
        private readonly IDbConnection _connection;

        public OrderItemsService()
        {
            _connection = DbConnectionProvider.GetInstance().Connection;
        }

        public void Add(OrderItem item)
        {
            _connection.Insert(item);
        }

        public OrderItem[] GetAll(int orderId)
        {
            var filters = new Filters();
            filters.AddFilter("order_id", SqlOperator.Equal, orderId);

            return _connection.Select<OrderItem>(filters).ToArray();
        }

        public void Delete(OrderItem item)
        {
            _connection.Delete(item);
        }
    }
}
