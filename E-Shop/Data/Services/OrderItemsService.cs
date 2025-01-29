using E_Shop.Models;
using E_Shop.Services;
using System.Data;

namespace E_Shop.Data.Services
{
    internal class OrderItemsService : IOrderItemsService
    {
        private readonly DbAccess _db;

        public OrderItemsService()
        {
            _db = DbAccess.GetInstance();
        }

        public void Add(OrderItem item)
        {
            _db.Insert("OrderItems", ["order_id", "product_id", "number"],
               [item.OrderId.ToString(), item.ProductId.ToString(), item.Number.ToString()]);
        }

        public OrderItem[] Get(int orderId)
        {
            DataTable data = _db.Select("OrderItems", ["*"], $"order_id={orderId}");

            List<OrderItem> items = new List<OrderItem>();

            foreach (DataRow row in data.Rows)
            {
                items.Add(new OrderItem(row));
            }

            return items.ToArray();
        }

        public void Delete(int orderId)
        {
            _db.Delete("OrderItems", $"order_id={orderId}");
        }
    }
}
