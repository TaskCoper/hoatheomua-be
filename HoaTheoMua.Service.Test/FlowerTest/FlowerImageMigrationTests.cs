using HoaTheoMua.Repository;
using HoaTheoMua.Repository.Migrations;
using HoaTheoMua.Repository.Enum;
using HoaTheoMua.Service.Test.PostGenTest;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql;
using Xunit;

namespace HoaTheoMua.Service.Test.FlowerTest;

public class FlowerImageMigrationTests
{
    [Fact]
    public void EfModelMatchesLatestMigrationSnapshot()
    {
        var builder = new NpgsqlDataSourceBuilder("Host=localhost;Database=model_check;Username=test;Password=test-only");
        builder.EnableDynamicJson();
        builder.MapEnum<SystemPromptType>("SystemPromptType");
        builder.MapEnum<PlatformType>("PlatformType");
        builder.MapEnum<CardFormType>("CardFormType");
        builder.MapEnum<ClientHistoryType>("ClientHistoryType");
        builder.MapEnum<FlowerReferenceType>("FlowerReferenceType");
        using var source = builder.Build();
        using var db = new AppDbContext(new DbContextOptionsBuilder<AppDbContext>().UseNpgsql(source).Options);
        Assert.False(db.Database.HasPendingModelChanges());
    }

    [PostGenPostgresFact]
    public async Task BackfillsLegacyMockupAndPreservesAllThreeImagesAcrossRollback()
    {
        var settings = new NpgsqlConnectionStringBuilder(Environment.GetEnvironmentVariable("POSTGEN_TEST_CONNECTION_STRING"));
        Assert.Contains(settings.Host, new[] { "localhost", "127.0.0.1" });
        var database = "flower_images_" + Guid.NewGuid().ToString("N");
        await using var admin = new NpgsqlConnection(settings.ConnectionString);
        await admin.OpenAsync();
        await using (var create = new NpgsqlCommand($"CREATE DATABASE \"{database}\"", admin)) await create.ExecuteNonQueryAsync();
        settings.Database = database;
        try
        {
            await using var connection = new NpgsqlConnection(settings.ConnectionString); await connection.OpenAsync();
            await Execute("""
                CREATE TABLE "GeneratedFlower" ("Id" integer PRIMARY KEY, "ImageUrl" varchar(2048) NOT NULL, "Metadata" jsonb NOT NULL);
                INSERT INTO "GeneratedFlower" VALUES
                  (1, 'https://example.com/flower.png', '{"flowerWithMockupImageUrl":"https://example.com/mockup.png"}'),
                  (2, 'https://example.com/legacy.png', '{}');
                """);
            await using var db = new AppDbContext(new DbContextOptionsBuilder<AppDbContext>().UseNpgsql(settings.ConnectionString).Options);
            var sql = db.GetService<IMigrationsSqlGenerator>();
            var migration = new AddGeneratedFlowerImageStages();
            foreach (var command in sql.Generate(migration.UpOperations)) await Execute(command.CommandText);
            Assert.Equal("https://example.com/mockup.png", await Scalar("SELECT \"FlowerWithMockupImageUrl\" FROM \"GeneratedFlower\" WHERE \"Id\"=1"));
            Assert.Equal(DBNull.Value, await Scalar("SELECT \"FlowerWithMockupImageUrl\" FROM \"GeneratedFlower\" WHERE \"Id\"=2"));
            var error = await Assert.ThrowsAsync<PostgresException>(() => Execute("UPDATE \"GeneratedFlower\" SET \"FlowerWithLogoImageUrl\"='https://example.com/logo.png' WHERE \"Id\"=2"));
            Assert.Equal(PostgresErrorCodes.CheckViolation, error.SqlState);
            await Execute("UPDATE \"GeneratedFlower\" SET \"FlowerWithLogoImageUrl\"='https://example.com/logo.png' WHERE \"Id\"=1");
            foreach (var command in sql.Generate(migration.DownOperations)) await Execute(command.CommandText);
            Assert.Equal("https://example.com/logo.png", await Scalar("SELECT \"Metadata\"->>'flowerWithLogoImageUrl' FROM \"GeneratedFlower\" WHERE \"Id\"=1"));
            foreach (var command in sql.Generate(migration.UpOperations)) await Execute(command.CommandText);
            Assert.Equal("https://example.com/logo.png", await Scalar("SELECT \"FlowerWithLogoImageUrl\" FROM \"GeneratedFlower\" WHERE \"Id\"=1"));
            Assert.Equal("https://example.com/flower.png", await Scalar("SELECT \"ImageUrl\" FROM \"GeneratedFlower\" WHERE \"Id\"=1"));

            async Task Execute(string text) { await using var command = new NpgsqlCommand(text, connection); await command.ExecuteNonQueryAsync(); }
            async Task<object?> Scalar(string text) { await using var command = new NpgsqlCommand(text, connection); return await command.ExecuteScalarAsync(); }
        }
        finally
        {
            await using var drop = new NpgsqlCommand($"DROP DATABASE \"{database}\" WITH (FORCE)", admin);
            await drop.ExecuteNonQueryAsync();
        }
    }
}
