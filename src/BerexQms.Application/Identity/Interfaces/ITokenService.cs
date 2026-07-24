using BerexQms.Domain.Identity.Entities;

namespace BerexQms.Application.Identity.Interfaces;

public interface ITokenService
{
    string GenerateAccessToken(User user, IEnumerable<string> roles);
    string GenerateRefreshToken();
}
