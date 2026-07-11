namespace Application.Users;

public interface IPasswordHasher
{
    string Hash(string password);
}
