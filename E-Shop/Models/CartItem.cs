using System.ComponentModel.DataAnnotations.Schema;

namespace E_Shop.Models
{
    [Table("CartItems")]
    internal class CartItem
    {
        [Column("user_id")]
        public int UserId { get; set; }

        [Column("product_id")]
        public int ProductId { get; set; }

        [Column("number")]
        public int Number { get; set; }

        public CartItem() { }

        public CartItem(int userId, int productId, int number)
        {
            UserId = userId;
            ProductId = productId;
            Number = number;
        }
    }
}
