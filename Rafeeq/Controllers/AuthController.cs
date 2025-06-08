using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Rafeeq.DTOs.Auth;
using Rafeeq.Services.Auth;

namespace Rafeeq.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("Register")]
        [AllowAnonymous]
        public async Task<IActionResult> Register([FromBody] RegisterDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _authService.RegisterAsync(dto);
            if (result == null)
            {
                return BadRequest("Registration failed. Email might already be in use.");
            }
            return Ok(new { Message = "Registration successful. Please check your email to verify your account.", TokenData = result });
        }

        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<ActionResult<TokenResponseDto>> Login([FromBody] LoginDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var loginResult = await _authService.LoginAsync(dto); 

            if (!loginResult.IsSuccess)
            {
                return Unauthorized(loginResult.ErrorMessage);
            }

            return Ok(new { Message = "Login successful.", TokenData = loginResult.TokenData });
        }

        [HttpPost("ExternalLogin")]
        [AllowAnonymous]
        public async Task<ActionResult<TokenResponseDto>> ExternalLogin([FromBody] ExternalLoginDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var tokenResponse = await _authService.ExternalLoginAsync(dto);
            if (tokenResponse == null)
            {
                return BadRequest("External login failed.");
            }
            return Ok(new { Message = "External login successful.", TokenData = tokenResponse });
        }

        [HttpPost("RefreshToken")]
        [AllowAnonymous]
        public async Task<ActionResult<TokenResponseDto>> RefreshToken([FromBody] string refreshToken)
        {
            var tokenResponse = await _authService.RefreshTokenAsync(refreshToken);
            if (tokenResponse == null)
            {
                return Unauthorized("Invalid refresh token.");
            }
            return Ok(new { Message = "Token refreshed successfully.", TokenData = tokenResponse });
        }

        [HttpPost("ForgotPassword")]
        [AllowAnonymous]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto dto)
        {
            await _authService.ForgotPasswordAsync(dto.Email);
            return Ok("Check your account, a password reset link has been sent.");
        }

        [HttpPost("ResetPassword")]
        [AllowAnonymous]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var success = await _authService.ResetPasswordAsync(dto.Token, dto.NewPassword);
            if (!success)
            {
                return BadRequest("Invalid or expired token, or password update failed.");
            }
            return Ok("Password has been reset successfully.");
        }

        [HttpGet("verify-email/{token}")] 
        [AllowAnonymous]
        public async Task<IActionResult> VerifyEmail(string token)
        {
            var success = await _authService.VerifyEmailAsync(token);
            if (!success)
            {
                return BadRequest("Email verification failed. Invalid or expired token.");
            }
            return Ok("Email verified successfully."); 
        }

        [HttpPost("ResendVerificationEmail")]
        [AllowAnonymous]
        public async Task<IActionResult> ResendVerification([FromBody] string email)
        {
            if (!ModelState.IsValid) 
            {
                return BadRequest(ModelState); 
            }
            await _authService.ResendVerificationEmailAsync(email);
            return Ok("Check your account, a new verification link has been sent.");
        }

        [HttpPost("logout")]
        [AllowAnonymous] 
        public async Task<IActionResult> Logout([FromBody] LogoutDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var success = await _authService.InvalidateRefreshTokenByValueAsync(dto.RefreshToken);

            if (!success)
            {
               
                Console.WriteLine($"Logout: Could not invalidate refresh token: {dto.RefreshToken}");
            }

            return Ok(new { Message = "Logged out successfully." });
        }
    }
}