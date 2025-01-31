using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;

namespace E_Shop.Models
{
    [Table("UserConfirmKeys")]
    internal class UserConfirmKey
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
