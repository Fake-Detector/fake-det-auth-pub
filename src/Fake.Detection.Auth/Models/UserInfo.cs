using System.ComponentModel.DataAnnotations.Schema;

namespace Fake.Detection.Auth.Models;

[Table("users")]
public class UserInfo
{ 
    [Column("id")] public long Id { get; set; }
    [Column("login")] public string Login { get; set; } = null!;
    [Column("name")] public string Name { get; set; } = null!;
    [Column("password_hash")] public string PasswordHash { get; set; } = null!;
    [Column("telegram_id")] public long? TelegramId { get; set; }
    [Column("created_at")] public DateTime CreatedAt { get; set; }
}