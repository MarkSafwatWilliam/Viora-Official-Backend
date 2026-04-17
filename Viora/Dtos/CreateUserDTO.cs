using System.ComponentModel.DataAnnotations;

namespace Viora.Dtos
{
    public class CreateUserDTO
    {
        [Required]
        public string DisplayName { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }
        [Required]
        public string Password { get; set; }
        [Required]
        public string MotherName { get; set; }
        [Required]
        public string CityOfBirth { get; set; }

        
        public bool IsHelper { get; set; }
    }
}
