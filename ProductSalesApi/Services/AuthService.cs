using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using ProductSalesApi.Auth;
using ProductSalesApi.Data;
using ProductSalesApi.DTOs.Auth;
using ProductSalesApi.Entities;
using ProductSalesApi.Exceptions;

namespace ProductSalesApi.Services;

public interface IAuthService
{
    Task<LoginResponseDto> Register(RegisterRequestDto registerDto);
    Task<LoginResponseDto> Login(LoginRequestDto loginDto);
    Task<LoginResponseDto> RefreshToken(string refreshToken);
}

public class AuthService : IAuthService
{
    private readonly AppDbContext _context;
    private readonly JwtHelper _jwtHelper;
    private readonly ILogger<AuthService> _logger;

    public AuthService(AppDbContext context, JwtHelper jwtHelper, ILogger<AuthService> logger)
    {
        _context = context;
        _jwtHelper = jwtHelper;
        _logger = logger;
    }

    public async Task<LoginResponseDto> Register(RegisterRequestDto registerDto)
    {
        _logger.LogInformation("Starting registration for email: {Email}", registerDto.Email);

        var normalizedEmail = registerDto.Email.Trim().ToLower();
        var emailExists = await _context.Users
            .AnyAsync(u => u.Email == normalizedEmail);

        if (emailExists)
        {
            _logger.LogWarning("Registration failed: Email already exists: {Email}", registerDto.Email);
            throw new AppException("Email is already registered");
        }

        var user = new User
        {
            FirstName = registerDto.FirstName.Trim(),
            LastName = registerDto.LastName.Trim(),
            Email = normalizedEmail,
            PasswordHash = HashPassword(registerDto.Password),
            CreatedAt = DateTime.UtcNow
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        _logger.LogInformation("User created successfully with ID: {UserId}", user.Id);

        var jwtToken = _jwtHelper.GenerateToken(user);
        _logger.LogInformation("JWT token generated for user: {UserId}", user.Id);

        var refreshToken = new RefreshToken
        {
            Token = GenerateRefreshTokenString(),
            Expires = DateTime.UtcNow.AddDays(7),
            Created = DateTime.UtcNow,
            UserId = user.Id
        };

        _context.RefreshTokens.Add(refreshToken);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Refresh token created for user: {UserId}", user.Id);

        var response = new LoginResponseDto
        {
            Token = jwtToken,
            RefreshToken = refreshToken.Token,
            ExpiresIn = TimeSpan.FromHours(2).TotalSeconds,
            User = new UserDto
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email
            }
        };

        _logger.LogInformation("Registration completed successfully for user: {UserId}", user.Id);
        return response;
    }

    public async Task<LoginResponseDto> Login(LoginRequestDto loginDto)
    {
        _logger.LogInformation("Login attempt for email: {Email}", loginDto.Email);

        if (string.IsNullOrWhiteSpace(loginDto.Email) || string.IsNullOrWhiteSpace(loginDto.Password))
        {
            _logger.LogWarning("Login failed: Empty email or password");
            throw new AppException("Email and password are required");
        }

        var normalizedEmail = loginDto.Email.Trim().ToLower();
        _logger.LogInformation("Searching for user with email: {Email}", normalizedEmail);
        
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Email == normalizedEmail);

        if (user == null)
        {
            _logger.LogWarning("Login failed: User not found for email: {Email}", normalizedEmail);
            throw new AppException("Invalid credentials");
        }

        _logger.LogInformation("User found with ID: {UserId}", user.Id);

        var passwordHash = HashPassword(loginDto.Password);
        _logger.LogInformation("Stored hash: {StoredHash}", user.PasswordHash);
        _logger.LogInformation("Computed hash: {ComputedHash}", passwordHash);

        if (user.PasswordHash != passwordHash)
        {
            _logger.LogWarning("Login failed: Invalid password for user: {UserId}", user.Id);
            throw new AppException("Invalid credentials");
        }

        _logger.LogInformation("Password verified successfully for user: {UserId}", user.Id);

        var jwtToken = _jwtHelper.GenerateToken(user);
        _logger.LogInformation("JWT token generated for user: {UserId}", user.Id);

        var refreshToken = new RefreshToken
        {
            Token = GenerateRefreshTokenString(),
            Expires = DateTime.UtcNow.AddDays(7),
            Created = DateTime.UtcNow,
            UserId = user.Id
        };

        _context.RefreshTokens.Add(refreshToken);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Refresh token created for user: {UserId}", user.Id);

        var response = new LoginResponseDto
        {
            Token = jwtToken,
            RefreshToken = refreshToken.Token,
            ExpiresIn = TimeSpan.FromHours(2).TotalSeconds,
            User = new UserDto
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email
            }
        };

        _logger.LogInformation("Login completed successfully for user: {UserId}", user.Id);
        return response;
    }

    public async Task<LoginResponseDto> RefreshToken(string refreshToken)
    {
        _logger.LogInformation("Refresh token attempt");

        var token = await _context.RefreshTokens
            .Include(rt => rt.User)
            .FirstOrDefaultAsync(rt => rt.Token == refreshToken && !rt.IsRevoked);

        if (token == null || token.Expires < DateTime.UtcNow)
        {
            _logger.LogWarning("Refresh token failed: Invalid or expired token");
            throw new AppException("Invalid or expired refresh token");
        }

        token.Revoke();
        _context.RefreshTokens.Update(token);

        var user = token.User;
        var newJwtToken = _jwtHelper.GenerateToken(user);
        
        var newRefreshToken = new RefreshToken
        {
            Token = GenerateRefreshTokenString(),
            Expires = DateTime.UtcNow.AddDays(7),
            Created = DateTime.UtcNow,
            UserId = user.Id
        };

        _context.RefreshTokens.Add(newRefreshToken);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Token refreshed successfully for user: {UserId}", user.Id);

        return new LoginResponseDto
        {
            Token = newJwtToken,
            RefreshToken = newRefreshToken.Token,
            ExpiresIn = TimeSpan.FromHours(2).TotalSeconds,
            User = new UserDto
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email
            }
        };
    }

    private static string GenerateRefreshTokenString()
    {
        var randomNumber = new byte[32];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);
        return Convert.ToBase64String(randomNumber);
    }

    private static string HashPassword(string password)
    {
        using var sha256 = SHA256.Create();
        var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
        return Convert.ToBase64String(hashedBytes);
    }
}
