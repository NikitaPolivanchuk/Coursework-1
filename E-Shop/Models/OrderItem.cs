using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;

namespace E_Shop.Models
{
    [Table("OrderItems")]
    public class OrderItem
    {
        [Column("id")]
        [Key]
        public int Id { get; set; }

        [Column("order_id")]
        public int OrderId { get; set; }

        [Column("product_id")]
        public int ProductId { get; set; }

        [Column("number")]
        public int Number { get; set; }

        public OrderItem() { }

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
