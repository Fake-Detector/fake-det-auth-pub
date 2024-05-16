using Fake.Detection.Auth.Models;
using Microsoft.EntityFrameworkCore;

namespace Fake.Detection.Auth.Contexts;

public class UserContext : DbContext
{
    public UserContext(DbContextOptions<UserContext> options)
        : base(options)
    {
    }

    public DbSet<UserInfo> UserInfos { get; set; } = null!;
}