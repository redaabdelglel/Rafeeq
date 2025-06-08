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

        public async Task<TokenResponseDto?> RegisterAsync(RegisterDto dto)
        {
            var existingUser = await _unitOfWork.UserRepository.GetUserByEmailAsync(dto.Email);
            if (existingUser != null)
            {
                return null; // Email already in use
            }

            var role = await _unitOfWork.context.Roles.FirstOrDefaultAsync(r => r.RoleName == dto.Role);
            if (role == null)
            {
                role = await _unitOfWork.context.Roles.FirstOrDefaultAsync(r => r.RoleName == "Mentee"); // Default to Mentee if role not found
            }

            var user = _mapp.Map<User>(dto);
            user.PasswordHash = PasswordHasher.HashPassword(dto.Password);
            user.RoleId = role!.RoleId; // Role is guaranteed to be set
            user.IsMentor = (dto.Role == "Mentor");
            user.IsInterviewer = (dto.Role == "Mentor"); // Assuming mentors are also interviewers
            user.IsEmailVerified = false; // User must verify email after registration
            user.CreatedAt = DateTime.UtcNow;

            _unitOfWork.UserRepository.Add(user);
            await _unitOfWork.SaveAsync();

            // Generate email verification token
            var token = Guid.NewGuid().ToString();
            var userToken = new UserToken
            {
                UserId = user.UserId,
                TokenType = "EmailVerification",
                TokenValue = token,
                ExpiryDate = DateTime.UtcNow.AddHours(24), // Token expires in 24 hours
                IsUsed = false,
                CreatedAt = DateTime.UtcNow
            };
            _unitOfWork.UserTokenRepository.Add(userToken);
            await _unitOfWork.SaveAsync();

            await _emailService.SendVerificationEmailAsync(user.Email, token);

            // Generate JWT tokens for immediate login (optional, or force email verification first)
            var tokenResponse = _jwtService.GenerateTokens(user);

            // Store refresh token
            var refreshTokenExpiresInDays = double.Parse(_config.GetSection("Jwt")["RefreshTokenExpirationDays"] ?? "7");
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

        public async Task<LoginResult> LoginAsync(LoginDto dto) // Changed return type to LoginResult
        {
            var user = await _unitOfWork.UserRepository.GetUserByEmailAsync(dto.Email);

            if (user == null || !PasswordHasher.VerifyPassword(dto.Password, user.PasswordHash))
            {
                return new LoginResult { ErrorMessage = "Invalid credentials." }; // Specific message
            }

            if (!user.IsEmailVerified.GetValueOrDefault()) // Handle nullable bool
            {
                return new LoginResult { ErrorMessage = "Email not verified." }; // Specific message
            }

            var tokenResponse = _jwtService.GenerateTokens(user);

            await InvalidateRefreshTokenAsync(user.UserId); // Invalidate any existing refresh tokens
            var refreshTokenExpiresInDays = double.Parse(_config.GetSection("Jwt")["RefreshTokenExpirationDays"] ?? "7");
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

            return new LoginResult { TokenData = tokenResponse }; // Success
        }

        public async Task<TokenResponseDto?> ExternalLoginAsync(ExternalLoginDto dto)
        {
            User? user = null;
            string verifiedEmail = string.Empty;
            string verifiedFullName = string.Empty;
            string verifiedExternalId = string.Empty;
            string? verifiedProfilePicture = null;

            switch (dto.Provider.ToLower())
            {
                case "google":
                    var googleClientId = _config["GoogleAuthSettings:ClientId"] ?? throw new InvalidOperationException("Google ClientId is not configured.");
                    try
                    {
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
                        Console.WriteLine($"Google token validation failed: {ex.Message}");
                        return null;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error during Google token validation: {ex.Message}");
                        return null;
                    }
                    break;

                case "facebook": // Assuming you'd add Facebook token validation here
                    verifiedEmail = dto.Email;
                    verifiedFullName = dto.FullName;
                    verifiedExternalId = dto.IdToken; // Facebook's User ID (or similar)
                    verifiedProfilePicture = dto.ProfilePicture;
                    break;

                case "linkedin": // Assuming you'd add LinkedIn token validation here
                    verifiedEmail = dto.Email;
                    verifiedFullName = dto.FullName;
                    verifiedExternalId = dto.IdToken; // LinkedIn's User ID (or similar)
                    verifiedProfilePicture = dto.ProfilePicture;
                    break;

                default:
                    return null; // Unsupported provider
            }

            user = await _unitOfWork.UserRepository.GetUserByExternalIdAndTypeAsync(verifiedExternalId, dto.Provider);

            if (user == null)
            {
                user = await _unitOfWork.UserRepository.GetUserByEmailAsync(verifiedEmail);
                if (user != null)
                {
                    user.ExternalId = verifiedExternalId;
                    user.ExternalType = dto.Provider;
                    user.ExternalToken = dto.IdToken;
                    user.IsEmailVerified = true; // Assume email verified by external provider
                    _unitOfWork.UserRepository.Update(user);
                    await _unitOfWork.SaveAsync();
                }
                else
                {
                    var role = await _unitOfWork.context.Roles.FirstOrDefaultAsync(r => r.RoleName == dto.Role);
                    if (role == null)
                    {
                        role = await _unitOfWork.context.Roles.FirstOrDefaultAsync(r => r.RoleName == "Mentee");
                    }

                    user = new User
                    {
                        FullName = verifiedFullName,
                        Email = verifiedEmail,
                        ExternalId = verifiedExternalId,
                        ExternalType = dto.Provider,
                        ExternalToken = dto.IdToken,
                        ProfilePicture = verifiedProfilePicture,
                        IsEmailVerified = true, // External providers usually verify email
                        RoleId = role!.RoleId,
                        IsMentor = (dto.Role == "Mentor"),
                        IsInterviewer = (dto.Role == "Mentor"),
                        CreatedAt = DateTime.UtcNow,
                        PasswordHash = PasswordHasher.HashPassword(Guid.NewGuid().ToString()) // Set a dummy password for external logins
                    };
                    _unitOfWork.UserRepository.Add(user);
                    await _unitOfWork.SaveAsync();
                }
            }
            else
            {
                user.ExternalToken = dto.IdToken;
                user.ProfilePicture = verifiedProfilePicture ?? user.ProfilePicture;
                _unitOfWork.UserRepository.Update(user);
                await _unitOfWork.SaveAsync();
            }

            var tokenResponse = _jwtService.GenerateTokens(user);

            await InvalidateRefreshTokenAsync(user.UserId); // Invalidate any existing refresh tokens
            var refreshTokenExpiresInDays = double.Parse(_config.GetSection("Jwt")["RefreshTokenExpirationDays"] ?? "7");
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

            var user = await _unitOfWork.UserRepository.GetByIdAsync(storedToken.UserId.GetValueOrDefault());
            if (user == null)
            {
                return null;
            }

            storedToken.IsUsed = true;
            _unitOfWork.UserTokenRepository.Update(storedToken);
            await _unitOfWork.SaveAsync();

            var newTokens = _jwtService.GenerateTokens(user);

            var refreshTokenExpiresInDays = double.Parse(_config.GetSection("Jwt")["RefreshTokenExpirationDays"] ?? "7");
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

        public async Task ForgotPasswordAsync(string email)
        {
            var user = await _unitOfWork.UserRepository.GetUserByEmailAsync(email);
            if (user == null)
            {
                return; // Prevent email enumeration
            }

            var existingTokens = await _unitOfWork.UserTokenRepository.GetActiveTokensForUserAsync(user.UserId, "PasswordReset");
            foreach (var token in existingTokens)
            {
                token.IsUsed = true;
                _unitOfWork.UserTokenRepository.Update(token);
            }
            await _unitOfWork.SaveAsync();

            var resetToken = Guid.NewGuid().ToString();
            var userToken = new UserToken
            {
                UserId = user.UserId,
                TokenType = "PasswordReset",
                TokenValue = resetToken,
                ExpiryDate = DateTime.UtcNow.AddHours(1),
                IsUsed = false,
                CreatedAt = DateTime.UtcNow
            };
            _unitOfWork.UserTokenRepository.Add(userToken);
            await _unitOfWork.SaveAsync();

            await _emailService.SendPasswordResetEmailAsync(user.Email, resetToken);
        }

        public async Task<bool> ResetPasswordAsync(string token, string newPassword)
        {
            var userToken = await _unitOfWork.UserTokenRepository.GetTokenByValueAndTypeAsync(token, "PasswordReset");

            if (userToken == null || userToken.IsUsed.GetValueOrDefault() || userToken.ExpiryDate < DateTime.UtcNow)
            {
                return false; // Invalid, used, or expired token
            }

            var user = await _unitOfWork.UserRepository.GetByIdAsync(userToken.UserId.GetValueOrDefault());
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

            var user = await _unitOfWork.UserRepository.GetByIdAsync(userToken.UserId.GetValueOrDefault());
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

        public async Task ResendVerificationEmailAsync(string email)
        {
            var user = await _unitOfWork.UserRepository.GetUserByEmailAsync(email);
            if (user == null || user.IsEmailVerified.GetValueOrDefault())
            {
                return; // Prevent email enumeration and avoid sending to already verified users
            }

            var existingTokens = await _unitOfWork.UserTokenRepository.GetActiveTokensForUserAsync(user.UserId, "EmailVerification");
            foreach (var token in existingTokens)
            {
                token.IsUsed = true;
                _unitOfWork.UserTokenRepository.Update(token);
            }
            await _unitOfWork.SaveAsync();

            var newToken = Guid.NewGuid().ToString();
            var userToken = new UserToken
            {
                UserId = user.UserId,
                TokenType = "EmailVerification",
                TokenValue = newToken,
                ExpiryDate = DateTime.UtcNow.AddHours(24),
                IsUsed = false,
                CreatedAt = DateTime.UtcNow
            };
            _unitOfWork.UserTokenRepository.Add(userToken);
            await _unitOfWork.SaveAsync();

            await _emailService.SendVerificationEmailAsync(user.Email, newToken);
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

        public async Task<bool> InvalidateRefreshTokenByValueAsync(string refreshTokenValue)
        {
            var token = await _unitOfWork.UserTokenRepository.GetTokenByValueAndTypeAsync(refreshTokenValue, "RefreshToken");

            if (token == null || token.IsUsed.GetValueOrDefault() || token.ExpiryDate < DateTime.UtcNow)
            {
           
                return false;
            }

            token.IsUsed = true;
            _unitOfWork.UserTokenRepository.Update(token);
            await _unitOfWork.SaveAsync();
            return true;
        }
    }
}