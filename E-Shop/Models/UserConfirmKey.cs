using System.ComponentModel.DataAnnotations.Schema;

namespace E_Shop.Models
{
    [Table("UserConfirmKeys")]
    public class UserConfirmKey
    {
        [Column("user_id")]
        public int UserId { get; set; }

        [Column("secret_key")]
        public string SecretKey { get; set; }

        public UserConfirmKey() { }

        public UserConfirmKey(int userId, string secretKey)
        {
            UserId = userId;
            SecretKey = secretKey;
        }
    }
}
