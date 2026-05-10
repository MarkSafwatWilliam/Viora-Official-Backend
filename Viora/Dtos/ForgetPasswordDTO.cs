using System.ComponentModel.DataAnnotations;

namespace Viora.Dtos
{
    public class ForgetPasswordDTO
    {

        [EmailAddress]
        public string Email { get; set; }

        
        public string NewPassword { get; set; }

        public string Token { get; set; }
    }
}
