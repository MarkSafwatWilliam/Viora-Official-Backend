using System.ComponentModel.DataAnnotations;

namespace Viora.Dtos
{
    public class VerifyAccountDTO
    {

        [EmailAddress]
        public string Email { get; set; }
        public string MotherName { get; set; }
        public string CityOfBirth { get; set; }
    }
}
