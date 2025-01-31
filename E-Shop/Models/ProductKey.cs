using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;

namespace E_Shop.Models
{
    [Table("ProductKeys")]
    internal class ProductKey
    {
        [Column("id")]
        [Key]
        public int Id { get; set; }

        [Column("product_id")]
        public int ProductId { get; set; }

        [Column("value")]
        public string Value { get; set; }

        public ProductKey() { }

        public ProductKey(DataRow data)
        {
            Id = (int)data[0];
            ProductId = (int)data[1];
            Value = (string)data[2];
        }

        public ProductKey(int productId, string value)
        {
            ProductId = productId;
            Value = value;
        }
    }
}
