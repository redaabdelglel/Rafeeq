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
        private readonly IEmailService _emailService;

        public AuthController(IAuthService authService, IEmailService emailService) // Inject IEmailService
        {
            _authService = authService;
            _emailService = emailService; // Initialize the field
        }


        [HttpPost("Register")]
        [AllowAnonymous]
        public async Task<IActionResult> Register([FromBody] RegisterDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState); 
            }

            var response = await _authService.RegisterAsync(dto);

            if (!response.IsSuccess)
            {
                if (response.IsEmailAlreadyRegistered)
                {
                    return BadRequest(new { Message = response.Message }); 
                }
                return StatusCode(500, new { Message = response.Message ?? "An unexpected server during registration." });
            }

           
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
            return Ok("Go to your account, a password reset link has been sent.");
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
                return BadRequest(" A password update failed.");
            }
            return Ok("Password has been reset successfully. You can now log in."); 
        }

        [HttpGet("verify-email/{token}")]
        [AllowAnonymous]
        public async Task<IActionResult> VerifyEmail(string token)
        {
            var success = await _authService.VerifyEmailAsync(token);
            if (!success)
            {
                return BadRequest("Email verification failed....");
            }
            return Ok("Email verified successfully! You can now log in.");
        }

        //[HttpPost("ResendVerificationEmail")]
        //[AllowAnonymous]
        //public async Task<IActionResult> ResendVerification([FromBody] string email)
        //{
        //    if (!ModelState.IsValid)
        //    {
        //        return BadRequest(ModelState);
        //    }
        //    await _authService.ResendVerificationEmailAsync(email);
        //    return Ok("Go to your account, a new verification link has been sent."); 
        //}



        [HttpPost("ResendVerificationEmail")]
        [AllowAnonymous]
        public async Task<IActionResult> ResendVerification([FromBody] ResendVerificationDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                await _authService.ResendVerificationEmailAsync(dto.Email);
                return Ok(new { Message = "Verification email sent successfully. Please check your inbox." });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
            catch (Exception)
            {
                return StatusCode(500, new { Message = "Failed to send verification email. Please try again later." });
            }
        }

        // commit Auth15
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



        [HttpPost("test-email")]
        [AllowAnonymous]
        public async Task<IActionResult> TestEmail([FromBody] string email)
        {
            try
            {
                await _emailService.SendEmailAsync(email, "Test Email", "<h1>Test successful!</h1>");
                return Ok(new { Message = "Test email sent successfully!" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }

        [HttpPost("test-verification")]
        [AllowAnonymous]
        public async Task<IActionResult> TestVerification([FromBody] string email)
        {
            try
            {
                await _emailService.SendVerificationEmailAsync(email, "test-token-123");
                return Ok(new { Message = "Test verification email sent!" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }

    }
}