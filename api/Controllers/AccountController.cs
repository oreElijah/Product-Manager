using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using api.Dtos.Account;
using api.Mappers;
using api.models;
using api.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using api.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.RateLimiting;
using Hangfire;

namespace api.Controllers
{
    [ApiController]
    [Route("api/[controller]/v{version:apiVersion}/")]
    public class AccountController : ControllerBase
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly ITokenService _tokenservice;
        private readonly IEmailService _emailService;

        public AccountController(UserManager<AppUser> userManager, ITokenService tokenservice, IEmailService emailService)
        {
            _userManager = userManager;
            _tokenservice = tokenservice;
            _emailService = emailService;
        }

        [ApiVersion("1.0")]
        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<IActionResult> Register([FromBody] RegisterRequestDto registerDto)
        {
            // if (!ModelState.IsValid)
            // {
            //     return BadRequest(ModelState);
            // }

            if (string.IsNullOrWhiteSpace(registerDto.Email) || string.IsNullOrWhiteSpace(registerDto.Password))
            {
                return BadRequest("Email and password are required.");
            }

            var user = new AppUser
            {
                UserName = registerDto.Username,
                Email = registerDto.Email,
                FirstName = registerDto.Firstname,
                LastName = registerDto.Lastname
            };

            var newUser = await _userManager.CreateAsync(user, registerDto.Password);
            if (!newUser.Succeeded)
            {
                return BadRequest(newUser.Errors);
            }

            var roleResult = await _userManager.AddToRoleAsync(user, "User");
            if (!roleResult.Succeeded)
            {
                return BadRequest(roleResult.Errors);
            }
                 var verifyToken = await _userManager.GenerateEmailConfirmationTokenAsync(user);

            BackgroundJob.Enqueue<IEmailService>(x =>
               x.SendVerifyUserEmail(
                   user.FirstName,
                   user.Email,
                     verifyToken));

            var responseDto = registerDto.ToRegisterResponseDto("User registered successfully, Check your mail");
            return Ok(responseDto);
        }

        [ApiVersion("1.0")]
        [EnableRateLimiting("fixed")]
        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] LoginRequestDto loginDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest("Invalid Email or username");
                }
                if (string.IsNullOrWhiteSpace(loginDto.Email) || string.IsNullOrWhiteSpace(loginDto.Password))
                {
                    return BadRequest("Email and password are required.");
                }

                var user = await _userManager.FindByEmailAsync(loginDto.Email);
                if (user == null)
                {
                    return Unauthorized("Invalid Email or username");
                }
                var passwordValid = await _userManager.CheckPasswordAsync(user, loginDto.Password);
                if (!passwordValid)
                {
                    return Unauthorized("Invalid password");
                }
                var token = await _tokenservice.CreateToken(user);

                var responsedto = loginDto.ToLoginResponseDto(user, token);

                return Ok(responsedto);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
        [ApiVersion("1.0")]
        [HttpGet("verify_email")]
        [AllowAnonymous]
        public async Task<IActionResult> VerifyEmail([FromQuery] string email, [FromQuery] string token)
        {
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(token))
            {
                return BadRequest("Email and token are required.");
            }

            try
            {
                await _emailService.VerifyEmail(email, token);
                return Ok("Email verified successfully");
            }
            catch (Exception ex)
            {
                return BadRequest($"Email verification failed: {ex.Message}");
            }
        }

        [ApiVersion("1.0")]
        [HttpPost("logout")]
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            var jti = User.GetJti();
            if (string.IsNullOrWhiteSpace(jti))
            {
                return BadRequest("Token jti is missing.");
            }
            await _tokenservice.Logout(jti);
            return Ok("Logged out successfully");
        }

        [ApiVersion("1.0")]
        [HttpDelete("delete/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteUser([FromRoute] string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound("User not found");
            }

            var result = await _userManager.DeleteAsync(user);
            if (!result.Succeeded)
            {
                return BadRequest(result.Errors);
            }

            return Ok("User deleted successfully");
        }
    }
}