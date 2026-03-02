namespace FormMaker.Api.Models;

/// <summary>
/// Request model for user registration
/// </summary>
public class RegisterRequest
{
    public required string Email { get; set; }
    public required string Password { get; set; }
    public string? DisplayName { get; set; }
}

/// <summary>
/// Request model for user login
/// </summary>
public class LoginRequest
{
    public required string Email { get; set; }
    public required string Password { get; set; }
}

/// <summary>
/// Response model for successful authentication
/// </summary>
public class AuthResponse
{
    public required string Token { get; set; }
    public required UserDto User { get; set; }
}

/// <summary>
/// User data transfer object (safe for client)
/// </summary>
public class UserDto
{
    public Guid Id { get; set; }
    public required string Email { get; set; }
    public string? DisplayName { get; set; }
    public bool EmailVerified { get; set; }
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// Generic error response
/// </summary>
public class ErrorResponse
{
    public required string Error { get; set; }
    public string? Details { get; set; }
}
