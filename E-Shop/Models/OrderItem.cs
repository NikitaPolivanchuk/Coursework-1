using System.Data;

namespace E_Shop.Models
{
    internal class OrderItem
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public int ProductId { get; set; }
        public int Number { get; set; }

        public OrderItem(int orderId, int productId, int number)
        {
            OrderId = orderId;
            ProductId = productId;
            Number = number;
        }

        public OrderItem(DataRow data)
        {
            Id = (int)data[0];
            OrderId = (int)data[1];
            ProductId = (int)data[2];
            Number = (int)data[3];
        }
    }
}
