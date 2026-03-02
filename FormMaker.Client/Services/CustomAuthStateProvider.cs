using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;

namespace FormMaker.Client.Services;

/// <summary>
/// Custom authentication state provider for managing user auth state
/// </summary>
public class CustomAuthStateProvider : AuthenticationStateProvider
{
    private readonly ApiService _apiService;
    private UserDto? _currentUser;

    public CustomAuthStateProvider(ApiService apiService)
    {
        _apiService = apiService;
    }

    public UserDto? CurrentUser => _currentUser;

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        var token = await _apiService.GetTokenAsync();

        if (string.IsNullOrEmpty(token))
        {
            return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
        }

        try
        {
            // Get current user from API
            _currentUser = await _apiService.GetAsync<UserDto>("auth/me");

            if (_currentUser == null)
            {
                return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
            }

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, _currentUser.Id.ToString()),
                new Claim(ClaimTypes.Email, _currentUser.Email),
                new Claim(ClaimTypes.Name, _currentUser.DisplayName ?? _currentUser.Email)
            };

            var identity = new ClaimsIdentity(claims, "apiauth");
            var user = new ClaimsPrincipal(identity);

            return new AuthenticationState(user);
        }
        catch
        {
            // Token is invalid or expired
            await _apiService.RemoveTokenAsync();
            return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
        }
    }

    /// <summary>
    /// Mark user as authenticated
    /// </summary>
    public async Task MarkUserAsAuthenticated(string token, UserDto user)
    {
        await _apiService.SetTokenAsync(token);
        _currentUser = user;

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Name, user.DisplayName ?? user.Email)
        };

        var identity = new ClaimsIdentity(claims, "apiauth");
        var claimsPrincipal = new ClaimsPrincipal(identity);

        NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(claimsPrincipal)));
    }

    /// <summary>
    /// Mark user as logged out
    /// </summary>
    public async Task MarkUserAsLoggedOut()
    {
        await _apiService.RemoveTokenAsync();
        _currentUser = null;

        var identity = new ClaimsIdentity();
        var user = new ClaimsPrincipal(identity);

        NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(user)));
    }
}
