using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using Viora.Data;
using Viora.Dtos;
using Viora.Models;
using Viora.Repositories;
using Viora.Services;

namespace Viora.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly GenericRepository<RefreshToken> _refreshTokenRepo;
        private readonly JwtAuthenticationService _jwtService;
        private readonly MangeAccountService _manageService;

        public AccountController(UserManager<ApplicationUser> userManager, JwtAuthenticationService jwtService, GenericRepository<RefreshToken> refreshTokenRepo,
            MangeAccountService manageService)
        {
            _userManager = userManager;
            _jwtService = jwtService;
            _refreshTokenRepo = refreshTokenRepo;
            _manageService = manageService;

        }


        //SignUp
        [AllowAnonymous]
        [HttpPost("register")]
        [SwaggerResponse(200, "SignUp Successfully")]
        [SwaggerResponse(400, "Failed to SignUp")]
        public async Task<IActionResult> Register([FromBody] CreateUserDTO dto)
        {
            if (dto == null || !ModelState.IsValid)
                return BadRequest("Invalid user data.");

            var user = new ApplicationUser
            {
                UserName = dto.Email,
                Email = dto.Email,
                DisplayName = dto.DisplayName,
                MotherName = dto.MotherName,
                CityOfBirth = dto.CityOfBirth,
                IsHelper = dto.IsHelper,
            };

            var result = await _userManager.CreateAsync(user, dto.Password);
            if (!result.Succeeded)
                return BadRequest(result.Errors);

            var roleName = dto.IsHelper ? "Volunteer" : "User";
            await _userManager.AddToRoleAsync(user, roleName);
            var roles = new List<string> { roleName };

            // Parallelize token generation
            var tokenTask = _jwtService.GenerateJwtToken(user, roles);
            var refreshTask = _jwtService.GenerateRefreshToken(user.Id);
            await Task.WhenAll(tokenTask, refreshTask);

            return Ok(new
            {
                user.Id,
                user.DisplayName,
                accessToken = tokenTask.Result,
                refreshToken = refreshTask.Result,
                user.Email,
                roles
            });
        }



        //login
        [AllowAnonymous]
        [SwaggerResponse(200, "Logged in successfully")]
        [SwaggerResponse(400, "Invalid data")]
        [SwaggerResponse(401, "Unauthorized")]
        [HttpPost("login")]

        public async Task<IActionResult> Login([FromBody] LoginDTO dto)
        {
            if (dto == null || !ModelState.IsValid)
                return BadRequest("Invalid login data.");

            var user = await _userManager.FindByEmailAsync(dto.Email);

            if (user == null || !await _userManager.CheckPasswordAsync(user, dto.Password))
                return Unauthorized("Invalid email or password.");

            var roles = await _userManager.GetRolesAsync(user);

            // Parallelize token generation
            var tokenTask = _jwtService.GenerateJwtToken(user, roles.ToList());
            var refreshTask = _jwtService.GenerateRefreshToken(user.Id);

            await Task.WhenAll(tokenTask, refreshTask);

            return Ok(new
            {
                user.Id,
                user.DisplayName,
                accessToken = tokenTask.Result,
                refreshToken = refreshTask.Result,
                user.Email,
                roles
            });
        }


        //refresh token
        [AllowAnonymous]
        [SwaggerResponse(200, "Token refreshed successfully")]
        [SwaggerResponse(401, "Invalid or expired refresh token")]
        [SwaggerResponse(404, "User not found")]
        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenDTO dto)
        {
            var oldToken = await _refreshTokenRepo.FindAsync(rt => rt.Token == dto.RefreshToken);
            if (oldToken == null || oldToken.ExpiredAt < DateTime.UtcNow || oldToken.IsRevoked)
                return Unauthorized();

            var user = await _userManager.FindByIdAsync(oldToken.UserId.ToString());
            if (user == null)
                return Unauthorized();

            // Revoke old token
            oldToken.IsRevoked = true;
            await _refreshTokenRepo.SaveChanges();

            var roles = await _userManager.GetRolesAsync(user);

            // Parallelize token generation
            var tokenTask = _jwtService.GenerateJwtToken(user, roles.ToList());
            var refreshTask = _jwtService.GenerateRefreshToken(user.Id);
            await Task.WhenAll(tokenTask, refreshTask);
            return Ok(new
            {
                accessToken = tokenTask.Result,
                refreshToken = refreshTask.Result
            });
        }


        //logout
        [Authorize]
        [SwaggerResponse(204, "Logged out successfully")]
        [SwaggerResponse(400, "Invalid refresh token")]
        [SwaggerResponse(401, "Unauthorized")]
        [HttpPost("logout")]

        public async Task<IActionResult> Logout([FromBody] RefreshTokenDTO dto)
        {
            if (string.IsNullOrEmpty(dto.RefreshToken))
                return BadRequest("Refresh token is required.");

            var token = await _refreshTokenRepo.FindAsync(rt => rt.Token == dto.RefreshToken);

            if (token == null || token.ExpiredAt < DateTime.UtcNow || token.IsRevoked)
                return BadRequest("Invalid refresh token.");

            token.IsRevoked = true;
            token.RevokedAt = DateTime.UtcNow;
            await _refreshTokenRepo.SaveChanges();

            return NoContent();
        }






        /// <summary>
        /// Verifies the user's identity using security information before allowing password reset.
        /// </summary>
        /// <remarks>
        /// This endpoint accepts the user's email and security answers (such as mother's name and city of birth).
        /// If verification succeeds, a temporary password reset token is generated and returned.
        /// The returned token must be used in the forget-password endpoint to complete the password reset.
        /// </remarks>
        /// <param name="dto">
        /// Verification data including:
        /// - Email
        /// - MotherName
        /// - CityOfBirth
        /// </param>
        /// <returns>
        /// Returns a temporary reset token if verification succeeds.
        /// </returns>
        [SwaggerResponse(StatusCodes.Status200OK, "Account verified successfully. Reset token returned.")]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "Verification failed due to invalid data.")]

        [HttpPost("verify-account")]
        public async Task<IActionResult> VerifyAccount([FromBody] VerifyAccountDTO dto)
        {
            try
            {
                var token = await _manageService.VerifyAccount(dto);
                return Ok(new { Token = token });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }



        /// <summary>
        /// Resets the user's password using a valid reset token.
        /// </summary>
        /// <remarks>
        /// This endpoint accepts the user's email, the reset token returned from verify-account,
        /// and the new password.
        /// If the token is valid, the user's password is updated successfully.
        /// </remarks>
        /// <param name="dto">
        /// Password reset data including:
        /// - Email
        /// - Token
        /// - NewPassword
        /// </param>
        /// <returns>
        /// Returns a success message if the password is reset successfully.
        /// </returns>
        [SwaggerResponse(StatusCodes.Status200OK, "Password reset successfully.")]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "Password reset failed due to invalid token or invalid password.")]

        [HttpPost("forget-password")]
        public async Task<IActionResult> ForgetPassword([FromBody] ForgetPasswordDTO dto)
        {
            try
            {
                var result = await _manageService.ForgetPassword(dto);
                if (result.Succeeded)
                    return Ok("Password reset successfully.");
                else
                    return BadRequest(result.Errors);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }

        }


    }
}
