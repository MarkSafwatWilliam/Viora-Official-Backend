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

        public AccountController(UserManager<ApplicationUser> userManager, JwtAuthenticationService jwtService, GenericRepository<RefreshToken> refreshTokenRepo)
        {
            _userManager = userManager;
            _jwtService = jwtService;
            _refreshTokenRepo = refreshTokenRepo;
        }


        //SignUp
        [AllowAnonymous]
        [HttpPost("register")]
        [SwaggerResponse(200, "SignUp Successfully")]
        [SwaggerResponse(400, "Failed to SignUp")]
        public async Task<IActionResult> Register(CreateUserDTO dto)
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

        public async Task<IActionResult> Login(LoginDTO dto)
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
        public async Task<IActionResult> RefreshToken(RefreshTokenDTO dto)
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

        public async Task<IActionResult> Logout(RefreshTokenDTO dto)
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
    }
}
