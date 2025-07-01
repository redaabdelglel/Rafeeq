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

        public async Task<RegisterResponseDto> RegisterAsync(RegisterDto dto)
        {
            var existingUser = await _unitOfWork.UserRepository.GetUserByEmailAsync(dto.Email);
            if (existingUser != null)
            {
                return new RegisterResponseDto
                {
                    IsSuccess = false,
                    Message = "Email is already registered.",
                    IsEmailAlreadyRegistered = true
                };
            }

            var role = await _unitOfWork.context.Roles.FirstOrDefaultAsync(r => r.RoleName == dto.Role);
            if (role == null)
            {
                role = await _unitOfWork.context.Roles.FirstOrDefaultAsync(r => r.RoleName == "Mentee");
                if (role == null)
                {
                    return new RegisterResponseDto 
                    {
                        IsSuccess = false,
                        Message = "Internal server error: Default user role not found.",
                        IsEmailAlreadyRegistered = false
                    };
                }
            }

            var user = _mapp.Map<User>(dto);
            user.PasswordHash = PasswordHasher.HashPassword(dto.Password);
            user.RoleId = role.RoleId;
            user.IsMentor = (dto.Role == "Mentor");
            user.IsInterviewer = (dto.Role == "Mentor");
            user.IsEmailVerified = false;
            user.CreatedAt = DateTime.UtcNow;

            _unitOfWork.UserRepository.Add(user);
            await _unitOfWork.SaveAsync();

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

            var frontendUrl = _config.GetSection("FrontendUrl").Value;
            if (string.IsNullOrEmpty(frontendUrl))
            {
                return new RegisterResponseDto
                {
                    IsSuccess = true,
                    Message = "Registration successful, but unable to send verification email due to server misconfiguration.",
                    IsEmailAlreadyRegistered = false
                };
            }
            //var verificationLink = $"{frontendUrl}/verify-email/{token}";

            await _emailService.SendVerificationEmailAsync(user.Email, token);

            return new RegisterResponseDto
            {
                IsSuccess = true,
                Message = "Registration successful. Please check your email to verify your account.",
                IsEmailAlreadyRegistered = false
            };
        }

        public async Task<LoginResult> LoginAsync(LoginDto dto)
        {
            var user = await _unitOfWork.UserRepository.GetUserByEmailAsync(dto.Email);
            if (user == null || !PasswordHasher.VerifyPassword(dto.Password, user.PasswordHash))
            {
                return new LoginResult { ErrorMessage = "Invalid credentials." };
            }
            if (!user.IsEmailVerified.GetValueOrDefault())
            {
                return new LoginResult { ErrorMessage = "Email not verified. Please check your email for a verification" };
            }
            // ... rest of login logic ...
            var tokenResponse = _jwtService.GenerateTokens(user);
            await InvalidateRefreshTokenAsync(user.UserId);
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
            return new LoginResult { TokenData = tokenResponse };
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
                    return null;
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
                    user.IsEmailVerified = true;
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
                        IsEmailVerified = true,
                        RoleId = role!.RoleId,
                        IsMentor = (dto.Role == "Mentor"),
                        IsInterviewer = (dto.Role == "Mentor"),
                        CreatedAt = DateTime.UtcNow,
                        PasswordHash = PasswordHasher.HashPassword(Guid.NewGuid().ToString())
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

            await InvalidateRefreshTokenAsync(user.UserId);
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
                return;
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

            var frontendUrl = _config.GetSection("FrontendUrl").Value;
            if (string.IsNullOrEmpty(frontendUrl))
            {
                return;
            }
            //  var resetLink = $"{frontendUrl}/reset-password/{resetToken}";

            await _emailService.SendPasswordResetEmailAsync(user.Email, resetToken);
        }

        public async Task<bool> ResetPasswordAsync(string token, string newPassword)
        {
           
            var userToken = await _unitOfWork.UserTokenRepository.GetTokenByValueAndTypeAsync(token.Trim(), "PasswordReset");

            if (userToken == null)
            {
                return false;
            }

            if (userToken.IsUsed.GetValueOrDefault() || userToken.ExpiryDate < DateTime.UtcNow)
            {
                return false;
            }

            var user = await _unitOfWork.UserRepository.GetByIdAsync(userToken.UserId.GetValueOrDefault());
            if (user == null)
            {
                return false;
            }

            user.PasswordHash = PasswordHasher.HashPassword(newPassword);
            _unitOfWork.UserRepository.Update(user);

            userToken.IsUsed = true; 
            _unitOfWork.UserTokenRepository.Update(userToken);

            await _unitOfWork.SaveAsync();
            return true;
        }

        public async Task<bool> VerifyEmailAsync(string token)
        {
            Console.WriteLine($"[VerifyEmail] - Attempting to verify email with token: {token}");

            var userToken = await _unitOfWork.UserTokenRepository.GetTokenByValueAndTypeAsync(token, "EmailVerification");

            if (userToken == null)
            {
                Console.WriteLine($"[VerifyEmail] - Token '{token}' not found in database or not of type 'EmailVerification'.");
                return false;
            }

            // Using .GetValueOrDefault() for nullable bool
            if (userToken.IsUsed.GetValueOrDefault())
            {
                Console.WriteLine($"[VerifyEmail] - Token '{token}' found but is already used.");
                return false;
            }

            if (userToken.ExpiryDate < DateTime.UtcNow)
            {
                Console.WriteLine($"[VerifyEmail] - Token '{token}' found but has expired. Expiry: {userToken.ExpiryDate}, Current UTC: {DateTime.UtcNow}");
                return false;
            }

            Console.WriteLine($"[VerifyEmail] - Token '{token}' is valid and active. Attempting to find user.");
            var user = await _unitOfWork.UserRepository.GetByIdAsync(userToken.UserId.GetValueOrDefault());
            if (user == null)
            {
                Console.WriteLine($"[VerifyEmail] - User not found for UserId: {userToken.UserId} associated with token '{token}'.");
                return false;
            }

            user.IsEmailVerified = true;
            _unitOfWork.UserRepository.Update(user);

            userToken.IsUsed = true; 
            _unitOfWork.UserTokenRepository.Update(userToken);

            await _unitOfWork.SaveAsync();
            Console.WriteLine($"[VerifyEmail] - Email for user '{user.Email}' (ID: {user.UserId}) successfully verified.");
            return true;
        }

        public async Task ResendVerificationEmailAsync(string email)
        {
            var user = await _unitOfWork.UserRepository.GetUserByEmailAsync(email);
            if (user == null || user.IsEmailVerified.GetValueOrDefault())
            {
                return;
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

            var frontendUrl = _config.GetSection("FrontendUrl").Value;
            if (string.IsNullOrEmpty(frontendUrl))
            {
                return;
            }
            //var verificationLink = $"{frontendUrl}/verify-email/{newToken}";

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