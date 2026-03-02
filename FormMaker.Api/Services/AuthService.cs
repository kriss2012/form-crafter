using BCrypt.Net;
using FormMaker.Api.Data;
using FormMaker.Api.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace FormMaker.Api.Services;

/// <summary>
/// Service for handling authentication operations
/// </summary>
public class AuthService
{
    private readonly FormMakerDbContext _dbContext;
    private readonly TokenService _tokenService;

    public AuthService(FormMakerDbContext dbContext, TokenService tokenService)
    {
        _dbContext = dbContext;
        _tokenService = tokenService;
    }

    /// <summary>
    /// Registers a new user
    /// </summary>
    public async Task<AuthResult> RegisterAsync(string email, string password, string? displayName = null)
    {
        // Validate input
        if (string.IsNullOrWhiteSpace(email))
        {
            return AuthResult.CreateFailure("Email is required");
        }

        if (string.IsNullOrWhiteSpace(password))
        {
            return AuthResult.CreateFailure("Password is required");
        }

        if (password.Length < 8)
        {
            return AuthResult.CreateFailure("Password must be at least 8 characters");
        }

        // Check if user already exists
        var existingUser = await _dbContext.Users
            .FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower());

        if (existingUser != null)
        {
            return AuthResult.CreateFailure("Email already registered");
        }

        // Hash password
        var passwordHash = BCrypt.Net.BCrypt.HashPassword(password);

        // Create user
        var user = new User
        {
            Email = email.ToLower(),
            PasswordHash = passwordHash,
            DisplayName = displayName,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync();

        // Generate token
        var token = _tokenService.GenerateToken(user);

        return AuthResult.CreateSuccess(token, user);
    }

    /// <summary>
    /// Authenticates a user and returns a JWT token
    /// </summary>
    public async Task<AuthResult> LoginAsync(string email, string password)
    {
        // Validate input
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
        {
            return AuthResult.CreateFailure("Email and password are required");
        }

        // Find user
        var user = await _dbContext.Users
            .FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower());

        if (user == null)
        {
            return AuthResult.CreateFailure("Invalid email or password");
        }

        // Check if account is active
        if (!user.IsActive)
        {
            return AuthResult.CreateFailure("Account is disabled");
        }

        // Verify password
        bool isValidPassword = BCrypt.Net.BCrypt.Verify(password, user.PasswordHash);

        if (!isValidPassword)
        {
            return AuthResult.CreateFailure("Invalid email or password");
        }

        // Update last login
        user.LastLoginAt = DateTime.UtcNow;
        await _dbContext.SaveChangesAsync();

        // Generate token
        var token = _tokenService.GenerateToken(user);

        return AuthResult.CreateSuccess(token, user);
    }

    /// <summary>
    /// Gets a user by ID
    /// </summary>
    public async Task<User?> GetUserByIdAsync(Guid userId)
    {
        return await _dbContext.Users.FindAsync(userId);
    }

    /// <summary>
    /// Validates a token and returns the user
    /// </summary>
    public async Task<User?> GetUserFromTokenAsync(string token)
    {
        var userId = _tokenService.GetUserIdFromToken(token);
        if (userId == null) return null;

        return await GetUserByIdAsync(userId.Value);
    }

    /// <summary>
    /// Gets all users (for admin/debugging)
    /// </summary>
    public async Task<List<User>> GetAllUsersAsync()
    {
        return await _dbContext.Users.ToListAsync();
    }
}

/// <summary>
/// Result of an authentication operation
/// </summary>
public class AuthResult
{
    public bool Success { get; set; }
    public string? Token { get; set; }
    public User? User { get; set; }
    public string? ErrorMessage { get; set; }

    public static AuthResult CreateSuccess(string token, User user)
    {
        return new AuthResult
        {
            Success = true,
            Token = token,
            User = user
        };
    }

    public static AuthResult CreateFailure(string errorMessage)
    {
        return new AuthResult
        {
            Success = false,
            ErrorMessage = errorMessage
        };
    }
}
