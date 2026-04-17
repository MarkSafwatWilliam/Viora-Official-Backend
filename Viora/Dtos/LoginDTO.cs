using System.ComponentModel.DataAnnotations;

namespace Viora.Dtos
{
    public class LoginDTO
    {
        [EmailAddress]
        public string Email { get; set; }
        public string Password { get; set; }
    }
}
