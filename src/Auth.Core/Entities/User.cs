using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Auth.Core.Entities
{
    [Table("users", Schema = "auth")]
    public class User
    {
        [Key]
        [Column("id")]
        public Guid Id { get; set; }

        [Column("email")]
        public string Email { get; set; }        // Уникальный email

        [Column("password_hash")]
        public string PasswordHash { get; set; } // Хеш пароля (bcrypt)

        [Column("is_active")]
        public bool IsActive { get; set; } = true;

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}