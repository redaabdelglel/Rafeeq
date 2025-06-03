using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Rafeeq.DTOs.Auth;
using Rafeeq.Models;
using Rafeeq.UnitOfWork;
using Rafeeq.Helpers;
using System.Linq;
using Microsoft.Extensions.Configuration;
using Google.Apis.Auth;

namespace Rafeeq.Services.Auth
{
    public class AuthService : IAuthService
    {
        private readonly UnitOfWorkManager _unitOfWork;
        private readonly IMapper _mapp;
        private readonly IJwtService _jwtService;
        private readonly IEmailService _emailService;
        private readonly IConfiguration _config;

        public AuthService(UnitOfWorkManager unitOfWork, IMapper mapp, IJwtService jwtService, IEmailService emailService, IConfiguration config)
        {
            _unitOfWork = unitOfWork;
            _mapp = mapp;
            _jwtService = jwtService;
            _emailService = emailService;
            _config = config;
        }
        public async Task<TokenResponseDto?> ExternalLoginAsync(ExternalLoginDto dto)
        {
            User? user = null;
            string verifiedEmail = string.Empty;
            string verifiedFullName = string.Empty;
            string verifiedExternalId = string.Empty;
            string? verifiedProfilePicture = null;

            // Step 1: Verify the ID Token with the external provider
            switch (dto.Provider.ToLower())
            {
                case "google":
                    var googleClientId = _config["GoogleAuthSettings:ClientId"] ?? throw new InvalidOperationException("Google ClientId is not configured.");
                    try
                    {
                        // Verify Google ID Token
                        var payload = await GoogleJsonWebSignature.ValidateAsync(dto.IdToken, new GoogleJsonWebSignature.ValidationSettings
                        {
                            Audience = new[] { googleClientId }
                        });

                        verifiedEmail = payload.Email;
                        verifiedFullName = payload.Name;
                        verifiedExternalId = payload.Subject; // Google's unique user ID
                        verifiedProfilePicture = payload.Picture;
                    }
                    catch (InvalidJwtException ex)
                    {
                        // Token is invalid, expired, or tampered with
                        Console.WriteLine($"Google token validation failed: {ex.Message}");
                        return null; // Return null on invalid token
                    }
                    catch (Exception ex)
                    {
                        // Other errors during Google token validation
                        Console.WriteLine($"Error during Google token validation: {ex.Message}");
                        return null;
                    }
                    break;

                case "facebook":
                   
                    verifiedEmail = dto.Email;
                    verifiedFullName = dto.FullName;
                    verifiedExternalId = dto.IdToken; 
                    verifiedProfilePicture = dto.ProfilePicture;
                    break;

                case "linkedin":
                    
                    verifiedEmail = dto.Email;
                    verifiedFullName = dto.FullName;
                    verifiedExternalId = dto.IdToken; 
                    verifiedProfilePicture = dto.ProfilePicture;
                    break;

                default:
                    return null; // Unsupported provider
            }

            // Step 2: Check if user exists in your database by external ID or verified email
            user = await _unitOfWork.UserRepository.GetUserByExternalIdAndTypeAsync(verifiedExternalId, dto.Provider);

            if (user == null)
            {
                // If not found by external ID, check by email to link existing accounts
                user = await _unitOfWork.UserRepository.GetUserByEmailAsync(verifiedEmail);
                if (user != null)
                {
                    // Existing user found by email, link the external ID
                    user.ExternalId = verifiedExternalId;
                    user.ExternalType = dto.Provider;
                    user.ExternalToken = dto.IdToken; // Store the external token if needed for future API calls to provider
                    _unitOfWork.UserRepository.Update(user);
                    await _unitOfWork.SaveAsync();
                }
                else
                {
                    // New user - register them
                    var role = (await _unitOfWork.context.Roles.FirstOrDefaultAsync(r => r.RoleName == dto.Role));
                    if (role == null)
                    {
                        role = (await _unitOfWork.context.Roles.FirstOrDefaultAsync(r => r.RoleName == "Mentee"));
                    }

                    user = new User
                    {
                        FullName = verifiedFullName,
                        Email = verifiedEmail,
                        ExternalId = verifiedExternalId,
                        ExternalType = dto.Provider,
                        ExternalToken = dto.IdToken, // Store the external token
                        ProfilePicture = verifiedProfilePicture,
                        IsEmailVerified = true, // External providers usually verify email
                        RoleId = role!.RoleId,
                        IsMentor = (dto.Role == "Mentor"),
                        IsInterviewer = (dto.Role == "Mentor"),
                        CreatedAt = DateTime.UtcNow
                    };
                    _unitOfWork.UserRepository.Add(user);
                    await _unitOfWork.SaveAsync();
                }
            }
            else
            {
                // Existing user found by external ID - update their external token/profile picture if necessary
                user.ExternalToken = dto.IdToken; // Update the external token if it changes
                user.ProfilePicture = verifiedProfilePicture ?? user.ProfilePicture; // Update profile picture if provided
                _unitOfWork.UserRepository.Update(user);
                await _unitOfWork.SaveAsync();
            }

            // Step 3: Generate and return your application's tokens
            var tokenResponse = _jwtService.GenerateTokens(user);

            // Invalidate old refresh tokens and store new one
            await InvalidateRefreshTokenAsync(user.UserId);
            var refreshTokenExpiresInDays = double.Parse(_config.GetSection("Jwt")["RefreshExpiresInDays"] ?? "7");
            var refreshTokenEntity = new UserToken
            {
                UserId = user.UserId,
                TokenType = "RefreshToken",
                TokenValue = tokenResponse.RefreshToken,
                ExpiryDate = DateTime.UtcNow.AddDays(refreshTokenExpiresInDays),
                IsUsed = false,
                CreatedAt = DateTime.UtcNow
            };
            _unitOfWork.UserTokenRepository.Add(refreshTokenEntity);
            await _unitOfWork.SaveAsync();

            return tokenResponse;
        }





        public async Task ForgotPasswordAsync(string email)
        {
            var user = await _unitOfWork.UserRepository.GetUserByEmailAsync(email);
            if (user == null)
            {
                // To prevent email enumeration, always return success, but don't send email
                return;
            }

            // Invalidate any existing password reset tokens for this user
            var existingTokens = await _unitOfWork.UserTokenRepository.GetActiveTokensForUserAsync(user.UserId, "PasswordReset");
            foreach (var token in existingTokens)
            {
                token.IsUsed = true;
                _unitOfWork.UserTokenRepository.Update(token);
            }
            await _unitOfWork.SaveAsync();

            // Generate new password reset token
            var resetToken = Guid.NewGuid().ToString();
            var userToken = new UserToken
            {
                UserId = user.UserId,
                TokenType = "PasswordReset",
                TokenValue = resetToken,
                ExpiryDate = DateTime.UtcNow.AddHours(1), // Reset token expires in 1 hour
                IsUsed = false,
                CreatedAt = DateTime.UtcNow
            };
            _unitOfWork.UserTokenRepository.Add(userToken);
            await _unitOfWork.SaveAsync();

            await _emailService.SendPasswordResetEmailAsync(user.Email, resetToken);
        }



        public async Task<bool> InvalidateRefreshTokenAsync(int userId)
        {
            var tokens = await _unitOfWork.UserTokenRepository.GetActiveTokensForUserAsync(userId, "RefreshToken");
            foreach (var token in tokens)
            {
                token.IsUsed = true;
                _unitOfWork.UserTokenRepository.Update(token);
            }
            await _unitOfWork.SaveAsync();
            return true;
        }

        public async Task<TokenResponseDto?> LoginAsync(LoginDto dto)
        {
            var user = await _unitOfWork.UserRepository.GetUserByEmailAsync(dto.Email);
            if (user == null || !PasswordHasher.VerifyPassword(dto.Password, user.PasswordHash))
            {
                return null; // Invalid credentials
            }
            // Check if email is verified
            if ((bool)!user.IsEmailVerified)
            {
                return null;
            }
            // Generate JWT tokens
            var tokenResponse = _jwtService.GenerateTokens(user);
            // Invalidate old refresh tokens and store new one
            await InvalidateRefreshTokenAsync(user.UserId); // Invalidate any existing refresh tokens
            var refreshTokenExpiresInDays = double.Parse(_config.GetSection("Jwt")["RefreshExpiresInDays"] ?? "7");
            var refreshTokenEntity = new UserToken
            {
                UserId = user.UserId,
                TokenType = "RefreshToken",
                TokenValue = tokenResponse.RefreshToken,
                ExpiryDate = DateTime.UtcNow.AddDays(refreshTokenExpiresInDays),
                IsUsed = false,
                CreatedAt = DateTime.UtcNow
            };
            _unitOfWork.UserTokenRepository.Add(refreshTokenEntity);
            await _unitOfWork.SaveAsync();

            return tokenResponse;
        }





        public async Task<TokenResponseDto?> RefreshTokenAsync(string refreshToken)
        {
            var storedToken = await _unitOfWork.UserTokenRepository.GetTokenByValueAndTypeAsync(refreshToken, "RefreshToken");

           
            if (storedToken == null || storedToken.IsUsed.GetValueOrDefault() || storedToken.ExpiryDate < DateTime.UtcNow)
            {
                return null;  
            }

            var user = await _unitOfWork.UserRepository.GetById(storedToken.UserId.GetValueOrDefault());
            if (user == null)
            {
                return null;
            }

            // Mark the old refresh token as used  
            storedToken.IsUsed = true;
            _unitOfWork.UserTokenRepository.Update(storedToken);
            await _unitOfWork.SaveAsync();

            // Generate new tokens  
            var newTokens = _jwtService.GenerateTokens(user);

            // Store the new refresh token  
            var refreshTokenExpiresInDays = double.Parse(_config.GetSection("Jwt")["RefreshExpiresInDays"] ?? "7");
            var newRefreshTokenEntity = new UserToken
            {
                UserId = user.UserId,
                TokenType = "RefreshToken",
                TokenValue = newTokens.RefreshToken,
                ExpiryDate = DateTime.UtcNow.AddDays(refreshTokenExpiresInDays),
                IsUsed = false,
                CreatedAt = DateTime.UtcNow
            };
            _unitOfWork.UserTokenRepository.Add(newRefreshTokenEntity);
            await _unitOfWork.SaveAsync();

            return newTokens;
        }



        public async Task<TokenResponseDto?> RegisterAsync(RegisterDto dto)
        {
            // 1. Check if user already exists
            var existingUser = await _unitOfWork.UserRepository.GetUserByEmailAsync(dto.Email);
            if (existingUser != null)
            {
                return null;

            }
            // 2. Get RoleId
            
            var role = (await _unitOfWork.context.Roles.FirstOrDefaultAsync(r => r.RoleName == dto.Role));
            if (role == null)
            {
                role = (await _unitOfWork.context.Roles.FirstOrDefaultAsync(r => r.RoleName == "Mentee"));
            }
            var user = _mapp.Map<User>(dto);
            user.PasswordHash = PasswordHasher.HashPassword(dto.Password);
            user.RoleId = role!.RoleId; 
            user.IsMentor = (dto.Role == "Mentor");
            user.IsInterviewer = (dto.Role == "Mentor");
            user.IsEmailVerified = true;
            user.CreatedAt = DateTime.UtcNow;

            // 4. Add user
            _unitOfWork.UserRepository.Add(user);
            await _unitOfWork.SaveAsync();

            // 5. Generate email verification token
            var token = Guid.NewGuid().ToString();
            var userToken = new UserToken
            {
                UserId = user.UserId,
                TokenType = "EmailVerification",
                TokenValue = token,
                ExpiryDate = DateTime.UtcNow.AddHours(24), 
                IsUsed = false,
                CreatedAt = DateTime.UtcNow
            };
            _unitOfWork.UserTokenRepository.Add(userToken);
            await _unitOfWork.SaveAsync();

            // 6.Send verification email
            await _emailService.SendVerificationEmailAsync(user.Email, token);

            // 7. Generate JWT token for immediate login (optional, or force email verification first)
            var tokenResponse = _jwtService.GenerateTokens(user);

            // 8. Store refresh token
            var refreshTokenExpiresInDays = double.Parse(_config.GetSection("Jwt")["RefreshExpiresInDays"] ?? "7");
            var refreshTokenEntity = new UserToken
            {
                UserId = user.UserId,
                TokenType = "RefreshToken",
                TokenValue = tokenResponse.RefreshToken,
                ExpiryDate = DateTime.UtcNow.AddDays(refreshTokenExpiresInDays),
                IsUsed = false,
                CreatedAt = DateTime.UtcNow
            };
            _unitOfWork.UserTokenRepository.Add(refreshTokenEntity);
            await _unitOfWork.SaveAsync();

            return tokenResponse;

        }

        public async Task ResendVerificationEmailAsync(string email)
        {
            var user = await _unitOfWork.UserRepository.GetUserByEmailAsync(email);
            if (user == null || user.IsEmailVerified.GetValueOrDefault())
            {
                // Prevent email enumeration and avoid sending to already verified users  
                return;
            }

            // Invalidate any existing email verification tokens for this user  
            var existingTokens = await _unitOfWork.UserTokenRepository.GetActiveTokensForUserAsync(user.UserId, "EmailVerification");
            foreach (var token in existingTokens)
            {
                token.IsUsed = true;
                _unitOfWork.UserTokenRepository.Update(token);
            }
            await _unitOfWork.SaveAsync();

            // Generate new email verification token  
            var newToken = Guid.NewGuid().ToString();
            var userToken = new UserToken
            {
                UserId = user.UserId,
                TokenType = "EmailVerification",
                TokenValue = newToken,
                ExpiryDate = DateTime.UtcNow.AddHours(24), // Token expires in 24 hours  
                IsUsed = false,
                CreatedAt = DateTime.UtcNow
            };
            _unitOfWork.UserTokenRepository.Add(userToken);
            await _unitOfWork.SaveAsync();

            await _emailService.SendVerificationEmailAsync(user.Email, newToken);
        }




        public async Task<bool> ResetPasswordAsync(string token, string newPassword)
        {
            var userToken = await _unitOfWork.UserTokenRepository.GetTokenByValueAndTypeAsync(token, "PasswordReset");

            // Fix for CS0019: Explicitly check for null and use GetValueOrDefault for nullable bool  
            if (userToken == null || userToken.IsUsed.GetValueOrDefault() || userToken.ExpiryDate < DateTime.UtcNow)
            {
                return false; // Invalid, used, or expired token  
            }

            // Fix for CS8602: Use GetValueOrDefault for nullable UserId  
            var user = await _unitOfWork.UserRepository.GetById(userToken.UserId.GetValueOrDefault());
            if (user == null)
            {
                return false;
            }

            user.PasswordHash = PasswordHasher.HashPassword(newPassword);
            _unitOfWork.UserRepository.Update(user);

            userToken.IsUsed = true; // Mark token as used  
            _unitOfWork.UserTokenRepository.Update(userToken);

            await _unitOfWork.SaveAsync();
            return true;
        }

        public async Task<bool> VerifyEmailAsync(string token)
        {
            var userToken = await _unitOfWork.UserTokenRepository.GetTokenByValueAndTypeAsync(token, "EmailVerification");

            if (userToken == null || userToken.IsUsed.GetValueOrDefault() || userToken.ExpiryDate < DateTime.UtcNow)
            {
                return false; 
            }

            // Fix for CS8602: Use GetValueOrDefault for nullable UserId  
            var user = await _unitOfWork.UserRepository.GetById(userToken.UserId.GetValueOrDefault());
            if (user == null)
            {
                return false;
            }

            user.IsEmailVerified = true;
            _unitOfWork.UserRepository.Update(user);

            userToken.IsUsed = true; // Mark token as used  
            _unitOfWork.UserTokenRepository.Update(userToken);

            await _unitOfWork.SaveAsync();
            return true;
        }
    }
}
