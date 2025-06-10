using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Rafeeq.DTOs.Auth;
using Rafeeq.Models;
using Rafeeq.Services.Auth;
using System;

namespace Rafeeq.Controllers
{
    [Route("api/admin")]
    [ApiController]
    public class AdminLogin : ControllerBase
    {
        private readonly RafeeqContext _context; // Change type from object to RafeeqContext
        private readonly IJwtService _jwtService;

        public AdminLogin(RafeeqContext context, IJwtService jwtService)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context)); // Add null check
            _jwtService = jwtService ?? throw new ArgumentNullException(nameof(jwtService)); // Add null check
        }

        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<IActionResult> adminLogin([FromBody] AdminLoginDto dto)
        {
            Console.WriteLine("==== Start Admin Login ====");
            Console.WriteLine($"Email Received: {dto?.Email}");
            Console.WriteLine($"Password Received: {dto?.Password}");

            if (dto == null)
            {
                Console.WriteLine("DTO is null");
                return BadRequest("Invalid request data");
            }

            var admin = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == dto.Email && u.RoleId == 1);

            if (admin == null)
            {
                Console.WriteLine("Admin not found or not roleId=1");
                return Unauthorized("Admin not found");
            }

            Console.WriteLine($"Found user in DB: {admin.FullName} with stored password: {admin.PasswordHash}");

            
            bool isPasswordCorrect = dto.Password == admin.PasswordHash;

            Console.WriteLine($"Password Verification Result: {isPasswordCorrect}");

            if (!isPasswordCorrect)
            {
                Console.WriteLine("Incorrect password");
                return Unauthorized("Invalid credentials");
            }

            var token = _jwtService.GenerateToken(admin);

            Console.WriteLine("Token successfully generated");
            Console.WriteLine("==== End Admin Login ====");

            return Ok(new { token });
        }
    }
    }
