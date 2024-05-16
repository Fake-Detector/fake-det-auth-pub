using FluentMigrator;

namespace Telegram.Data.Collector.DAL.Migrations;

[Migration(20231508, TransactionBehavior.None)]
public class InitSchema : Migration {
    public override void Up()
    {
        Create.Table("users")
            .WithColumn("id").AsInt64().PrimaryKey("post_pk").Identity()
            .WithColumn("login").AsString().NotNullable().Unique()
            .WithColumn("name").AsString().NotNullable()
            .WithColumn("password_hash").AsString().NotNullable()
            .WithColumn("telegram_id").AsInt64().Nullable()
            .WithColumn("created_at").AsDateTimeOffset().NotNullable();
    }

    public override void Down()
    {
        Delete.Table("users");
    }
}