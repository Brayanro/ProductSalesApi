using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProductSalesApi.DTOs.Auth;
using ProductSalesApi.Exceptions;
using ProductSalesApi.Services;

namespace ProductSalesApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IAuthService authService, ILogger<AuthController> logger)
    {
        _authService = authService;
        _logger = logger;
    }

    [HttpPost("register")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(LoginResponseDto))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Register([FromBody] RegisterRequestDto registerDto)
    {
        if (!ModelState.IsValid)
        {
            _logger.LogWarning("Invalid model state for registration");
            return BadRequest(ModelState);
        }

        try
        {
            var response = await _authService.Register(registerDto);
            _logger.LogInformation("User registered successfully: {Email}", registerDto.Email);
            return Ok(response);
        }
        catch (AppException ex)
        {
            _logger.LogWarning(ex, "Registration failed for email: {Email}", registerDto.Email);
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during registration for email: {Email}", registerDto.Email);
            return StatusCode(500, new { message = "An error occurred while processing your request", details = ex.Message });
        }
    }

    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(LoginResponseDto))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto loginDto)
    {
        if (!ModelState.IsValid)
        {
            _logger.LogWarning("Invalid model state for login");
            return BadRequest(ModelState);
        }

        try
        {
            var response = await _authService.Login(loginDto);
            _logger.LogInformation("User logged in successfully: {Email}", loginDto.Email);
            return Ok(response);
        }
        catch (AppException ex)
        {
            _logger.LogWarning(ex, "Login failed for email: {Email}", loginDto.Email);
            return Unauthorized(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during login for email: {Email}", loginDto.Email);
            return StatusCode(500, new { message = "An error occurred while processing your request", details = ex.Message });
        }
    }

    [HttpPost("refresh-token")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(LoginResponseDto))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> RefreshToken([FromBody] string refreshToken)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(refreshToken))
            {
                return BadRequest(new { message = "Refresh token is required" });
            }

            var response = await _authService.RefreshToken(refreshToken);
            _logger.LogInformation("Token refreshed successfully");
            return Ok(response);
        }
        catch (AppException ex)
        {
            _logger.LogWarning(ex, "Failed to refresh token");
            return Unauthorized(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while refreshing token");
            return StatusCode(500, new { message = "An error occurred while processing your request" });
        }
    }
}
