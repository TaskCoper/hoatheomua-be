using HoaTheoMua.Repository;
using HoaTheoMua.Service.Exceptions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql;
using Xunit;
using FeatureService = HoaTheoMua.Service.VoucherProgram.Service;
using static HoaTheoMua.Service.Test.VoucherProgramTest.VoucherProgramTestData;

namespace HoaTheoMua.Service.Test.VoucherProgramTest;

public class VoucherProgramPostgres
{
    [VoucherPostgresFact]
    public async Task RealDatabase_HandlesCollisions_RollsBackIncompleteBatch_AndUpdatesEntireProgramAtomically()
    {
        await using var fixture = await PostgresDatabase.Create();
        await using var db = fixture.Context();
        var program = Program(true);
        db.VoucherPrograms.Add(program);
        db.Vouchers.Add(VoucherProgramTestData.Voucher(null, "TAKEN", deleted: true));
        await db.SaveChangesAsync();

        var failing = new FeatureService(db, new SequenceGenerator("NEWAA", "TAKEN"));
        await Assert.ThrowsAsync<ServerException>(() => failing.GenerateProgramVouchers(program.Id, GenerateRequest(2)));
        Assert.Empty(await db.Vouchers.Where(x => x.ProgramId == program.Id).ToListAsync());

        var service = new FeatureService(db, new SequenceGenerator("TAKEN", "NEWAA", "NEWAA", "NEWBB"));
        var result = await service.GenerateProgramVouchers(program.Id, GenerateRequest(2));
        Assert.Equal(new[] { "NEWAA", "NEWBB" }, result.Codes.Order());
        Assert.All(await db.Vouchers.Where(x => x.ProgramId == program.Id).ToListAsync(), voucher =>
        {
            Assert.False(voucher.IsActive);
            Assert.Equal(Repository.Enum.VoucherVisibility.Private, voucher.Visibility);
            Assert.Equal(Repository.Enum.VoucherDiscountType.Percent, voucher.DiscountType);
            Assert.Null(voucher.StartAt);
            Assert.Null(voucher.MinOrderAmount);
            Assert.Equal(0, voucher.UsedCount);
        });

        // Force the program write to fail after the voucher bulk update: both changes must roll back.
        await db.Database.ExecuteSqlRawAsync("""
            CREATE FUNCTION reject_program_change() RETURNS trigger LANGUAGE plpgsql AS $$
            BEGIN RAISE EXCEPTION 'forced test failure'; END; $$;
            CREATE TRIGGER reject_program_change BEFORE UPDATE ON "VoucherPrograms"
            FOR EACH ROW EXECUTE FUNCTION reject_program_change();
            """);
        await Assert.ThrowsAsync<DbUpdateException>(() => service.UpdateVoucherProgram(program.Id, new() { IsActive = true }));
        db.ChangeTracker.Clear();
        Assert.All(await db.Vouchers.Where(x => x.ProgramId == program.Id).ToListAsync(), x => Assert.False(x.IsActive));
        await Assert.ThrowsAsync<DbUpdateException>(() => service.DeleteVoucherProgram(program.Id));
        db.ChangeTracker.Clear();
        Assert.False((await db.VoucherPrograms.SingleAsync()).IsDeleted);
        Assert.All(await db.Vouchers.Where(x => x.ProgramId == program.Id).ToListAsync(), x => Assert.False(x.IsDeleted));
        await db.Database.ExecuteSqlRawAsync("DROP TRIGGER reject_program_change ON \"VoucherPrograms\";");

        await service.UpdateVoucherProgram(program.Id, new() { IsActive = true });
        db.ChangeTracker.Clear();
        Assert.All(await db.Vouchers.Where(x => x.ProgramId == program.Id).ToListAsync(), x => Assert.True(x.IsActive));
        await service.UpdateVoucherProgram(program.Id, new() { IsActive = false });
        db.ChangeTracker.Clear();
        Assert.All(await db.Vouchers.Where(x => x.ProgramId == program.Id).ToListAsync(), x => Assert.False(x.IsActive));

        var orderId = Guid.NewGuid();
        const string snapshot = "{\"code\":\"NEWAA\",\"discountAmount\":100000}";
        await db.Database.ExecuteSqlInterpolatedAsync(
            $"INSERT INTO \"Orders\" (\"Id\", \"VoucherSnapshot\") VALUES ({orderId}, CAST({snapshot} AS jsonb))");
        await service.DeleteVoucherProgram(program.Id);
        db.ChangeTracker.Clear();
        Assert.True((await db.VoucherPrograms.SingleAsync()).IsDeleted);
        Assert.All(await db.Vouchers.Where(x => x.ProgramId == program.Id).ToListAsync(), x => Assert.True(x.IsDeleted));
        Assert.Equal(new[] { "NEWAA", "NEWBB", "TAKEN" }, await db.Vouchers.OrderBy(x => x.Code).Select(x => x.Code).ToArrayAsync());
        await using var connection = await fixture.Source.OpenConnectionAsync();
        await using var query = new NpgsqlCommand("SELECT \"VoucherSnapshot\"->>'code' FROM \"Orders\"", connection);
        Assert.Equal("NEWAA", await query.ExecuteScalarAsync());
    }

    [VoucherPostgresFact]
    public async Task ConcurrentBatches_CannotExceedProgramLimit()
    {
        await using var fixture = await PostgresDatabase.Create();
        var program = Program();
        await using (var seed = fixture.Context())
        {
            seed.VoucherPrograms.Add(program);
            await seed.SaveChangesAsync();
            await seed.Database.ExecuteSqlInterpolatedAsync($"""
                INSERT INTO "Vouchers"
                    ("Id", "ProgramId", "Code", "DiscountType", "DiscountValue", "Visibility",
                     "MaxUsageCount", "UsedCount", "IsActive", "IsDeleted")
                SELECT md5(n::text)::uuid, {program.Id}, 'OLD' || n::text, 'Cash', 10000, 'Private', 1, 0, false, false
                FROM generate_series(1, 9990) n
                """);
        }

        // Hold the parent lock so both independent requests start before either can count/insert.
        await using var blocker = await fixture.Source.OpenConnectionAsync();
        await using var transaction = await blocker.BeginTransactionAsync();
        await using (var command = new NpgsqlCommand("SELECT \"Id\" FROM \"VoucherPrograms\" FOR UPDATE", blocker, transaction))
            await command.ExecuteScalarAsync();
        await using var firstDb = fixture.Context();
        await using var secondDb = fixture.Context();
        var first = new FeatureService(firstDb).GenerateProgramVouchers(program.Id, GenerateRequest(10));
        var second = new FeatureService(secondDb).GenerateProgramVouchers(program.Id, GenerateRequest(10));
        await transaction.CommitAsync();

        var outcomes = await Task.WhenAll(Outcome(first), Outcome(second));
        Assert.Single(outcomes, success => success);
        await using var verify = fixture.Context();
        Assert.Equal(10000, await verify.Vouchers.CountAsync(x => x.ProgramId == program.Id));
        Assert.Equal(10000, await verify.Vouchers.Select(x => x.Code).Distinct().CountAsync());
    }

    private static async Task<bool> Outcome(Task<Service.VoucherProgram.Response.GenerateProgramVouchersResponse> task)
    {
        try
        {
            Assert.Equal(10, (await task).CreatedCount);
            return true;
        }
        catch (ConflictException error)
        {
            Assert.Equal("Voucher program cannot exceed 10000 vouchers.", error.Message);
            return false;
        }
    }

    private sealed class PostgresDatabase : IAsyncDisposable
    {
        private readonly NpgsqlConnection _admin;
        private readonly string _database;
        public NpgsqlDataSource Source { get; }

        private PostgresDatabase(NpgsqlConnection admin, string database, NpgsqlDataSource source)
        {
            _admin = admin;
            _database = database;
            Source = source;
        }

        public AppDbContext Context() => new(new DbContextOptionsBuilder<AppDbContext>().UseNpgsql(Source).Options);

        public static async Task<PostgresDatabase> Create()
        {
            var settings = new NpgsqlConnectionStringBuilder(Environment.GetEnvironmentVariable("VOUCHER_TEST_CONNECTION_STRING"));
            Assert.Contains(settings.Host, new[] { "localhost", "127.0.0.1" });
            var database = "voucher_test_" + Guid.NewGuid().ToString("N");
            var admin = new NpgsqlConnection(settings.ConnectionString);
            await admin.OpenAsync();
            await using (var create = new NpgsqlCommand($"CREATE DATABASE \"{database}\"", admin))
                await create.ExecuteNonQueryAsync();
            settings.Database = database;
            var fixture = new PostgresDatabase(admin, database, NpgsqlDataSource.Create(settings.ConnectionString));
            try
            {
                await using var db = fixture.Context();
                await db.Database.ExecuteSqlRawAsync("CREATE TABLE \"Orders\" (\"Id\" uuid PRIMARY KEY);");
                // Use the actual voucher migration, without migrating unrelated modules.
                var migrations = db.GetService<IMigrationsAssembly>();
                var migration = migrations.CreateMigration(migrations.Migrations["20261007021252_voucher"], db.Database.ProviderName!);
                foreach (var command in db.GetService<IMigrationsSqlGenerator>().Generate(migration.UpOperations, db.Model))
                    await db.Database.ExecuteSqlRawAsync(command.CommandText);

                return fixture;
            }
            catch
            {
                await fixture.DisposeAsync();
                throw;
            }
        }

        public async ValueTask DisposeAsync()
        {
            await Source.DisposeAsync();
            await using var drop = new NpgsqlCommand($"DROP DATABASE \"{_database}\" WITH (FORCE)", _admin);
            try { await drop.ExecuteNonQueryAsync(); }
            finally { await _admin.DisposeAsync(); }
        }
    }
}

public sealed class VoucherPostgresFactAttribute : FactAttribute
{
    public VoucherPostgresFactAttribute()
    {
        if (string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("VOUCHER_TEST_CONNECTION_STRING")))
            Skip = "Set VOUCHER_TEST_CONNECTION_STRING to a disposable local PostgreSQL server to run integration checks.";
    }
}
