using System.Data;

namespace E_Shop.Models
{
    internal class ProductKey
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string Value { get; set; }

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
