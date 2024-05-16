using Fake.Detection.Auth.Contexts;
using Fake.Detection.Auth.Models;
using Microsoft.EntityFrameworkCore;

namespace Fake.Detection.Auth.Repositories;

public class UserRepository : IUserRepository
{
    private readonly UserContext _context;

    public UserRepository(UserContext context) =>
        _context = context;


    public async Task<UserInfo> CreateAsync(string login, string name, string password, CancellationToken token)
    {
        var userInfo = await GetAsync(login, token);

        if (userInfo is not null)
            throw new ArgumentException("A user with the same name already exists");

        var createdUser = (await _context.UserInfos.AddAsync(new UserInfo
        {
            Login = login,
            Name = name,
            PasswordHash = password,
            CreatedAt = DateTime.UtcNow
        }, token)).Entity;

        await _context.SaveChangesAsync(token);

        return createdUser;
    }

    public Task<UserInfo?> GetAsync(string login, CancellationToken token) =>
        _context.UserInfos.FirstOrDefaultAsync(x => x.Login == login, cancellationToken: token);

    public Task<UserInfo?> GetAsync(long telegramId, CancellationToken token) =>
        _context.UserInfos.FirstOrDefaultAsync(x => x.TelegramId == telegramId, cancellationToken: token);

    public async Task<UserInfo> LinkTgAsync(string login, long? telegramId, CancellationToken token)
    {
        var userInfo = await GetAsync(login, token);

        if (userInfo is null || userInfo.TelegramId is not null && telegramId is not null &&
            userInfo.TelegramId != telegramId)
            throw new ArgumentException("A user with the same name already exists");

        if (userInfo.TelegramId == telegramId)
            return userInfo;

        userInfo.TelegramId = telegramId;

        await _context.SaveChangesAsync(token);

        return userInfo;
    }

    public async Task<UserInfo?> UpdateAsync(string login, string name, string password, CancellationToken token)
    {
        var userInfo = await GetAsync(login, token);

        if (userInfo is null)
            return null;

        userInfo.Name = name;
        userInfo.PasswordHash = password;

        await _context.SaveChangesAsync(token);

        return userInfo;
    }
}