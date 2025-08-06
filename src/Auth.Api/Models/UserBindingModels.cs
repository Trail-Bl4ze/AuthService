using System.ComponentModel.DataAnnotations;
using Auth.Core.Entities;

namespace Auth.Web.Models
{
    public class UserBindingModel
    {
        public Guid Id { get; set; }

        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress]
        public string Email { get; set; }

        [Required(ErrorMessage = "Password is required.")]
        public string PasswordHash { get; set; }

        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}