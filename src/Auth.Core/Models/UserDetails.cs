namespace Auth.Core.Models
{
    public class UserDetails
    {
        public Guid Id { get; set; }

        public string Email { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; }
    }
}