using System.ComponentModel.DataAnnotations;
using Auth.Core.Entities;

namespace Auth.Web.Models
{
    public class AuthenticateUserBindingModel
    {
       [Required(ErrorMessage = "Email is required.")]
       public string Email { get; set; }

       [Required(ErrorMessage = "Password is required.")]
       public string Password { get; set; } 
    }
}