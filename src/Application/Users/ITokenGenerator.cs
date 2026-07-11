using Domain.Users;

namespace Application.Users;

public interface ITokenGenerator
{
    string GenerateToken(User user);
}
