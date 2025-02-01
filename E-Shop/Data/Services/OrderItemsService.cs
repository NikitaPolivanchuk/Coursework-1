using DbToolkit;
using DbToolkit.Enums;
using DbToolkit.Filtering;
using E_Shop.Models;
using System.Data;

namespace E_Shop.Data.Services
{
    public class OrderItemsService : IOrderItemsService
    {
        private readonly IDbConnection _connection;

        public OrderItemsService(DbConnectionProvider connectionProvider)
        {
            _connection = connectionProvider.Connection;
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
