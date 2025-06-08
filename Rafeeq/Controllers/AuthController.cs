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

            var response = await _authService.RegisterAsync(dto); // Get the structured response

            if (!response.IsSuccess)
            {
                // Return specific error messages based on the response
                if (response.IsEmailAlreadyRegistered)
                {
                    return BadRequest(new { Message = response.Message });
                }
                return StatusCode(500, new { Message = response.Message }); // For other internal errors
            }

            // Return success message. No TokenData is included here.
            return Ok(new { Message = response.Message });
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
                // Use Unauthorized for authentication/authorization failures, BadRequest for bad input
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
            // The service handles email enumeration prevention, so always return success to the client
            await _authService.ForgotPasswordAsync(dto.Email);
            return Ok("If an account with that email exists, a password reset link has been sent."); // More generic message
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
            return Ok("Password has been reset successfully. You can now log in."); // Added message for clarity
        }

        [HttpGet("verify-email/{token}")]
        [AllowAnonymous]
        public async Task<IActionResult> VerifyEmail(string token)
        {
            var success = await _authService.VerifyEmailAsync(token);
            if (!success)
            {
                return BadRequest("Email verification failed. Invalid or expired token. Please try resending the verification email."); // More helpful message
            }
            return Ok("Email verified successfully! You can now log in.");
        }

        [HttpPost("ResendVerificationEmail")]
        [AllowAnonymous]
        public async Task<IActionResult> ResendVerification([FromBody] string email)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            // Again, for security, always return a success message if the email format is valid
            await _authService.ResendVerificationEmailAsync(email);
            return Ok("If an account with that email exists and is not verified, a new verification link has been sent."); // More generic message
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
                // Even if token invalidation fails, we tell the user they are logged out.
                // The client will clear the token anyway.
            }

            return Ok(new { Message = "Logged out successfully." });
        }
    }
}