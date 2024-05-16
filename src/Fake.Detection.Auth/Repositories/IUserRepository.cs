using Fake.Detection.Auth.Models;

namespace Fake.Detection.Auth.Repositories;

public interface IUserRepository
{
    Task<UserInfo> CreateAsync(string login, string name, string password, CancellationToken token);

    Task<UserInfo?> GetAsync(string login, CancellationToken token);
    Task<UserInfo?> GetAsync(long telegramId, CancellationToken token);
    Task<UserInfo> LinkTgAsync(string login, long? telegramId, CancellationToken token);
    Task<UserInfo?> UpdateAsync(string login, string name, string password, CancellationToken token);
}