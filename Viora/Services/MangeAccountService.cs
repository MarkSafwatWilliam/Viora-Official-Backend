using Microsoft.AspNetCore.Identity;
using Viora.Dtos;
using Viora.Models;
using Viora.Repositories;

namespace Viora.Services
{
    public class MangeAccountService
    {

        private readonly UserManager<ApplicationUser> _userManager;

        public MangeAccountService(UserManager<ApplicationUser> userManager)
        {

            _userManager = userManager;
        }



        public async Task<string> VerifyAccount(VerifyAccountDTO userData)
        {
            var user = await _userManager.FindByEmailAsync(userData.Email);

            if (user == null ||
                !string.Equals(user.MotherName, userData.MotherName, StringComparison.OrdinalIgnoreCase) ||
                !string.Equals(user.CityOfBirth, userData.CityOfBirth, StringComparison.OrdinalIgnoreCase))
            {
                throw new UnauthorizedAccessException("Invalid verification data.");
            }

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);

            return token;
        }


        public async Task<IdentityResult> ForgetPassword(ForgetPasswordDTO resetData)
        {
            var user = await _userManager.FindByEmailAsync(resetData.Email);
            if (user == null)
                throw new Exception("Account not found.");

            var result = await _userManager.ResetPasswordAsync(user, resetData.Token, resetData.NewPassword);


            return result;
        }
    }
}
