using FarmaApi.Models;

namespace FarmaApi.Services;

public interface ITokenService
{
    string CreateToken(User user, bool rememberMe = false);
    int GetTokenExpiryMinutes(bool rememberMe = false);
}
