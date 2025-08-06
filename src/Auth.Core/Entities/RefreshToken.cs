using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Auth.Core.Entities
{
    [Table("refresh_tokens", Schema = "auth")]
    public class RefreshToken
    {
        [Key]
        [Column("id")]
        public Guid Id { get; set; } // Добавленный первичный ключ

        [Column("token")]
        public string Token { get; set; } // Уникальный токен

        [Column("expires_at")]
        public DateTime ExpiresAt { get; set; } // Срок действия

        [Column("is_revoked")]
        public bool IsRevoked { get; set; } // Отозван ли токен

        [Column("user_id")]
        public Guid UserId { get; set; } // Внешний ключ

        [Column("created_at")]
        public DateTime CreatedAt { get; set; }
    }
}