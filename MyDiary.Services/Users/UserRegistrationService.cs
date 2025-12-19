using MyDiary.Data.Interfaces.Repositories;
using MyDiary.Domain.Entities;
using MyDiary.Services.Security;

namespace MyDiary.Services.Users;

public sealed record UserRegistrationResult(bool IsSuccess, User? User, string Error);

public static class UserRegistrationService
{
    public static async Task<UserRegistrationResult> RegisterAsync(
        IUserRepository userRepository,
        string login,
        string name,
        string password,
        string repeatPassword,
        DateTime? nowUtc = null,
        CancellationToken cancellationToken = default)
    {
        login = (login ?? string.Empty).Trim();
        name = (name ?? string.Empty).Trim();
        password ??= string.Empty;
        repeatPassword ??= string.Empty;

        if (login.Length < 3)
        {
            return new UserRegistrationResult(false, null, "Логин должен содержать минимум 3 символа");
        }

        if (name.Length < 2)
        {
            return new UserRegistrationResult(false, null, "Имя должно содержать минимум 2 символа");
        }

        if (password.Length < 6)
        {
            return new UserRegistrationResult(false, null, "Пароль должен содержать минимум 6 символов");
        }

        if (!string.Equals(password, repeatPassword, StringComparison.Ordinal))
        {
            return new UserRegistrationResult(false, null, "Пароли должны совпадать");
        }

        var existing = await userRepository.GetByLoginAsync(login, cancellationToken);
        if (existing is not null)
        {
            return new UserRegistrationResult(false, null, "Данный логин уже занят");
        }

        var user = new User
        {
            Id = Guid.NewGuid(),
            Login = login,
            Name = name,
            PasswordHash = PasswordHasher.Hash(password),
            CreatedAtUtc = nowUtc ?? DateTime.UtcNow
        };

        await userRepository.AddAsync(user, cancellationToken);
        return new UserRegistrationResult(true, user, string.Empty);
    }
}
