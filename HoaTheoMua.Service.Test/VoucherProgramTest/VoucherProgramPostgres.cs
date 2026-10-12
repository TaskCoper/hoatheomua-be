using System.Data.Common;
using System.Collections.Concurrent;
using System.Text.Json;
using HoaTheoMua.Repository;
using HoaTheoMua.Repository.Enum;
using HoaTheoMua.Service.Exceptions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Npgsql;
using Xunit;
using FeatureService = HoaTheoMua.Service.VoucherProgram.Service;
using OrderEntity = HoaTheoMua.Repository.Entity.Order;
using static HoaTheoMua.Service.Test.VoucherProgramTest.VoucherProgramTestData;

namespace HoaTheoMua.Service.Test.VoucherProgramTest;

public class VoucherProgramPostgres
{
    [VoucherPostgresFact]
    public async Task RealDatabase_HandlesCollisions_RollsBackIncompleteBatch_AndUpdatesEntireProgramAtomically()
    {
        await using var fixture = await PostgresDatabase.Create();
        var rejectProgramSave = new RejectProgramSaveInterceptor();
        await using var db = fixture.Context(rejectProgramSave);
        var program = Program(true);
        db.VoucherPrograms.Add(program);
        db.Vouchers.Add(NewVoucher(null, "TAKEN", active: true));
        await db.SaveChangesAsync();

        using var cancellation = new CancellationTokenSource();
        var failing = new FeatureService(db, new CancellingGenerator(cancellation));
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            failing.GenerateProgramVouchers(program.Id, GenerateRequest(2), cancellation.Token));
        Assert.Empty(await db.Vouchers.Where(x => x.ProgramId == program.Id).ToListAsync());

        var service = new FeatureService(db, new SequenceGenerator("TAKEN", "NEWAA", "NEWAA", "NEWBB"));
        var result = await service.GenerateProgramVouchers(program.Id, GenerateRequest(2));
        Assert.Equal(new[] { "NEWAA", "NEWBB" }, result.Codes.Order());
        Assert.All(await db.Vouchers.Where(x => x.ProgramId == program.Id).ToListAsync(), voucher =>
        {
            Assert.True(voucher.IsActive);
            Assert.Equal(Repository.Enum.VoucherVisibility.Private, voucher.Visibility);
            Assert.Equal(Repository.Enum.VoucherDiscountType.Percent, voucher.DiscountType);
            Assert.Null(voucher.StartAt);
            Assert.Null(voucher.MinOrderAmount);
            Assert.Equal(0, voucher.UsedCount);
        });

        // Force SaveChanges to fail after ExecuteUpdate changed the vouchers; the transaction must roll back both.
        rejectProgramSave.Enabled = true;
        await Assert.ThrowsAsync<DbUpdateException>(() => service.UpdateVoucherProgram(program.Id, new() { IsActive = false }));
        db.ChangeTracker.Clear();
        Assert.All(await db.Vouchers.Where(x => x.ProgramId == program.Id).ToListAsync(), x => Assert.True(x.IsActive));
        await Assert.ThrowsAsync<DbUpdateException>(() => service.DeleteVoucherProgram(program.Id));
        db.ChangeTracker.Clear();
        Assert.False((await db.VoucherPrograms.SingleAsync()).IsDeleted);
        Assert.All(await db.Vouchers.Where(x => x.ProgramId == program.Id).ToListAsync(), x => Assert.False(x.IsDeleted));

        rejectProgramSave.Enabled = false;
        await service.UpdateVoucherProgram(program.Id, new() { IsActive = true });
        db.ChangeTracker.Clear();
        Assert.All(await db.Vouchers.Where(x => x.ProgramId == program.Id).ToListAsync(), x => Assert.True(x.IsActive));
        await service.UpdateVoucherProgram(program.Id, new() { IsActive = false });
        db.ChangeTracker.Clear();
        Assert.All(await db.Vouchers.Where(x => x.ProgramId == program.Id).ToListAsync(), x => Assert.False(x.IsActive));

        const string snapshot = "{\"code\":\"NEWAA\",\"discountAmount\":100000}";
        var order = TestOrder(snapshot);
        db.Orders.Add(order);
        await db.SaveChangesAsync();
        await service.DeleteVoucherProgram(program.Id);
        db.ChangeTracker.Clear();
        Assert.True((await db.VoucherPrograms.SingleAsync()).IsDeleted);
        Assert.All(await db.Vouchers.Where(x => x.ProgramId == program.Id).ToListAsync(), x => Assert.True(x.IsDeleted));
        Assert.Equal(new[] { "NEWAA", "NEWBB", "TAKEN" }, await db.Vouchers.OrderBy(x => x.Code).Select(x => x.Code).ToArrayAsync());
        var savedSnapshot = await db.Orders.Where(x => x.Id == order.Id).Select(x => x.VoucherSnapshot).SingleAsync();
        Assert.Equal("NEWAA", JsonDocument.Parse(savedSnapshot!).RootElement.GetProperty("code").GetString());
    }

    [VoucherPostgresFact]
    public async Task ConcurrentBatches_CannotExceed10000ActiveCodes()
    {
        await using var fixture = await PostgresDatabase.Create();
        var program = Program();
        await using (var seed = fixture.Context())
        {
            seed.VoucherPrograms.Add(program);
            seed.Vouchers.AddRange(Enumerable.Range(1, 9990)
                .Select(number => NewVoucher(program.Id, $"OLD{number}", active: true)));
            await seed.SaveChangesAsync();
        }

        var synchronize = new SynchronizeFirstQueryInterceptor(2);
        await using var firstDb = fixture.Context(synchronize);
        await using var secondDb = fixture.Context(synchronize);
        var first = new FeatureService(firstDb).GenerateProgramVouchers(program.Id, GenerateRequest(10));
        var second = new FeatureService(secondDb).GenerateProgramVouchers(program.Id, GenerateRequest(10));

        var results = await Task.WhenAll(Outcome(first), Outcome(second));
        Assert.Single(results, success => success);
        await using var verify = fixture.Context();
        Assert.Equal(10000, await verify.Vouchers.CountAsync(x => x.ProgramId == program.Id));
        Assert.Equal(10000, await verify.Vouchers.CountAsync(x => x.ProgramId == program.Id && x.IsActive));

        static async Task<bool> Outcome(Task<Service.VoucherProgram.Response.GenerateProgramVouchersResponse> task)
        {
            try
            {
                Assert.Equal(10, (await task).CreatedCount);
                return true;
            }
            catch (ConflictException error)
            {
                Assert.Equal("Mỗi chương trình chỉ được có tối đa 10.000 voucher đang hoạt động.", error.Message);
                return false;
            }
        }
    }

    [VoucherPostgresFact]
    public async Task CodeActivatedAfterExistenceCheck_RetriesBatchWithoutPartialRows()
    {
        await using var fixture = await PostgresDatabase.Create();
        var program = Program();
        await using (var seed = fixture.Context())
        {
            seed.VoucherPrograms.Add(program);
            await seed.SaveChangesAsync();
        }

        var race = new ClaimCodeBeforeInsert(async () =>
        {
            await using var competitor = fixture.Context();
            // No parent FK: this independent write can commit while generation holds the program lock.
            competitor.Vouchers.Add(NewVoucher(null, "RACE1", active: true));
            await competitor.SaveChangesAsync();
        });
        await using var db = fixture.Context(race);
        var service = new FeatureService(db, new SequenceGenerator("RACE1", "SAFE1", "SAFE2"));

        var result = await service.GenerateProgramVouchers(program.Id, GenerateRequest(2));

        Assert.True(race.Claimed);
        Assert.Equal(new[] { "SAFE1", "SAFE2" }, result.Codes);
        Assert.Equal(2, await db.Vouchers.CountAsync(x => x.ProgramId == program.Id));
        Assert.Empty(db.ChangeTracker.Entries<Repository.Entity.Voucher>());
        Assert.Equal(1, await db.Vouchers.CountAsync(x => x.Code == "RACE1"));
        Assert.Equal(2, await db.Vouchers.CountAsync(x => x.ProgramId == program.Id && x.IsActive));
    }

    [VoucherPostgresFact]
    public async Task ConcurrentActivationOfSameCode_InDifferentPrograms_OnlyOneSucceeds()
    {
        await using var fixture = await PostgresDatabase.Create();
        var firstProgram = Program();
        var secondProgram = Program();
        await using (var seed = fixture.Context())
        {
            seed.VoucherPrograms.AddRange(firstProgram, secondProgram);
            seed.Vouchers.AddRange(NewVoucher(firstProgram.Id, "SHARED"), NewVoucher(secondProgram.Id, "SHARED"));
            await seed.SaveChangesAsync();
        }

        var synchronize = new SynchronizeFirstQueryInterceptor(2);
        await using var firstDb = fixture.Context(synchronize);
        await using var secondDb = fixture.Context(synchronize);
        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(30));
        var first = Activate(new FeatureService(firstDb), firstProgram.Id);
        var second = Activate(new FeatureService(secondDb), secondProgram.Id);
        var results = await Task.WhenAll(first, second);
        Assert.Single(results, success => success);
        await using var verify = fixture.Context();
        Assert.Equal(1, await verify.Vouchers.CountAsync(x => x.IsActive));
        Assert.Equal(1, await verify.VoucherPrograms.CountAsync(x => x.IsActive));

        async Task<bool> Activate(FeatureService service, Guid id)
        {
            try
            {
                await service.UpdateVoucherProgram(id, new() { IsActive = true }, timeout.Token);
                return true;
            }
            catch (ConflictException)
            {
                return false;
            }
        }
    }

    private sealed class ClaimCodeBeforeInsert(Func<Task> claimCode) : DbCommandInterceptor
    {
        public bool Claimed { get; private set; }

        public override async ValueTask<InterceptionResult<DbDataReader>> ReaderExecutingAsync(
            DbCommand command, CommandEventData eventData, InterceptionResult<DbDataReader> result,
            CancellationToken cancellationToken = default)
        {
            if (!Claimed && eventData.CommandSource == CommandSource.SaveChanges)
            {
                Claimed = true;
                await claimCode();
            }

            return result;
        }
    }

    private sealed class RejectProgramSaveInterceptor : SaveChangesInterceptor
    {
        public bool Enabled { get; set; }

        public override ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData,
            InterceptionResult<int> result, CancellationToken cancellationToken = default)
        {
            if (Enabled && eventData.Context!.ChangeTracker.Entries<Repository.Entity.VoucherProgram>()
                    .Any(entry => entry.State == EntityState.Modified))
                throw new DbUpdateException("Forced program write failure.");

            return ValueTask.FromResult(result);
        }
    }

    private sealed class SynchronizeFirstQueryInterceptor(int participantCount) : DbCommandInterceptor
    {
        private readonly ConcurrentDictionary<Guid, byte> _contexts = new();
        private readonly TaskCompletionSource _ready = new(TaskCreationOptions.RunContinuationsAsynchronously);
        private int _arrived;

        public override async ValueTask<InterceptionResult<DbDataReader>> ReaderExecutingAsync(
            DbCommand command, CommandEventData eventData, InterceptionResult<DbDataReader> result,
            CancellationToken cancellationToken = default)
        {
            await Synchronize(eventData, cancellationToken);
            return result;
        }

        public override async ValueTask<InterceptionResult<int>> NonQueryExecutingAsync(
            DbCommand command, CommandEventData eventData, InterceptionResult<int> result,
            CancellationToken cancellationToken = default)
        {
            await Synchronize(eventData, cancellationToken);
            return result;
        }

        private async Task Synchronize(CommandEventData eventData, CancellationToken cancellationToken)
        {
            if (_contexts.TryAdd(eventData.Context!.ContextId.InstanceId, 0))
            {
                if (Interlocked.Increment(ref _arrived) == participantCount)
                    _ready.TrySetResult();
                await _ready.Task.WaitAsync(cancellationToken);
            }

        }
    }

    private static OrderEntity TestOrder(string voucherSnapshot) => new()
    {
        Id = Guid.NewGuid(),
        Ward = 1,
        City = 1,
        Address = "Test address",
        ShortAddress = "Test",
        FullName = "Test customer",
        Phone = "0900000000",
        Remarks = "",
        Status = "Paid",
        TrackNumber = "TEST-ORDER",
        OrderTime = 0,
        PhoneReceiver = "0900000000",
        FullNameReceiver = "Test receiver",
        PaymentGateway = PaymentGateway.Sepay,
        ShippingGateway = ShippingGateway.Ahamove,
        VoucherSnapshot = voucherSnapshot,
        CreatedAt = DateTimeOffset.UtcNow
    };

    internal sealed class PostgresDatabase : IAsyncDisposable
    {
        private readonly string _connectionString;

        private PostgresDatabase(string connectionString)
        {
            _connectionString = connectionString;
        }

        public void Configure(DbContextOptionsBuilder options) => options.UseNpgsql(_connectionString);

        public AppDbContext Context(params IInterceptor[] interceptors) => new(new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql(_connectionString).AddInterceptors(interceptors).Options);

        public static async Task<PostgresDatabase> Create()
        {
            var settings = new NpgsqlConnectionStringBuilder(Environment.GetEnvironmentVariable("VOUCHER_TEST_CONNECTION_STRING"));
            Assert.Contains(settings.Host, new[] { "localhost", "127.0.0.1" });
            settings.Database = "voucher_test_" + Guid.NewGuid().ToString("N");
            settings.Pooling = false;
            var fixture = new PostgresDatabase(settings.ConnectionString);
            try
            {
                await using var db = fixture.Context();
                await db.Database.EnsureCreatedAsync();
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
            await using var db = Context();
            await db.Database.EnsureDeletedAsync();
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

public sealed class VoucherPostgresTheoryAttribute : TheoryAttribute
{
    public VoucherPostgresTheoryAttribute()
    {
        if (string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("VOUCHER_TEST_CONNECTION_STRING")))
            Skip = "Set VOUCHER_TEST_CONNECTION_STRING to a disposable local PostgreSQL server to run integration checks.";
    }
}
