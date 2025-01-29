using E_Shop.Models;
using E_Shop.Services;
using System.Data;

namespace E_Shop.Data.Services
{
    internal class ProductKeyService : IProductKeyService
    {
        private readonly DbAccess _db = DbAccess.GetInstance();

        public void Add(ProductKey productKey)
        {
            _db.Insert("ProductKeys", ["product_id", "value"],
                [productKey.ProductId.ToString(), productKey.Value]);
        }

        public ProductKey? Get(int id)
        {
            DataTable data = _db.Select("ProductKeys", ["*"], $"id={id}");

            if (data.Rows.Count < 1 )
            {
                return null;
            }
            return new ProductKey(data.Rows[0]);
        }

        public void Delete(int id)
        {
            _db.Delete("ProductKeys", $"id={id}");
        }

        public ProductKey[] GetAll(int productId)
        {
            List<ProductKey> productKeys = new List<ProductKey>();
            DataTable data = _db.Select("ProductKeys", ["*"], $"product_id={productId}");
            
            foreach (DataRow row in data.Rows)
            {
                productKeys.Add(new ProductKey(row));
            }

            return productKeys.ToArray();
        }
    }
}
