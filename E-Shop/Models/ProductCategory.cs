using System.ComponentModel.DataAnnotations.Schema;


namespace E_Shop.Models
{
    [Table("Product_Category")]
    public class ProductCategory
    {
        [Column("product_id")]
        public int ProductId { get; set; }

        [Column("category_id")]
        public int CategoryId { get; set; }

        public ProductCategory() { }

        public ProductCategory(int productId, int categoryId)
        {
            ProductId = productId;
            CategoryId = categoryId;
        }
    }
}
