using HoaTheoMua.Repository.Entity;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HoaTheoMua.Repository.Configurations.Seeds;

public static class TransactionSeed
{
    public static void SeedTransaction(this EntityTypeBuilder<Transaction> builder)
    {
        var baseDate = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);
        
        builder.HasData(
            new Transaction { Id = Guid.Parse("55555555-5555-5555-5555-555555555551"), SePayId = 1001, Gateway = "SePay", TransactionDate = baseDate.UtcDateTime, AccountNumber = "123456789", SubAccount = "", Code = "SEP1001", Content = "Payment for TRK1001", TransferType = "IN", Description = "Test", TransferAmount = 100000, Accumulated = 100000, ReferenceCode = "REF1001", OrderId = Guid.Parse("44444444-4444-4444-4444-444444444441"), CreatedAt = baseDate, IsDeleted = false },
            new Transaction { Id = Guid.Parse("55555555-5555-5555-5555-555555555552"), SePayId = 1002, Gateway = "SePay", TransactionDate = baseDate.AddDays(1).UtcDateTime, AccountNumber = "123456789", SubAccount = "", Code = "SEP1002", Content = "Payment for TRK1002", TransferType = "IN", Description = "Test", TransferAmount = 150000, Accumulated = 250000, ReferenceCode = "REF1002", OrderId = Guid.Parse("44444444-4444-4444-4444-444444444442"), CreatedAt = baseDate.AddDays(1), IsDeleted = false },
            new Transaction { Id = Guid.Parse("55555555-5555-5555-5555-555555555553"), SePayId = 1003, Gateway = "SePay", TransactionDate = baseDate.AddDays(2).UtcDateTime, AccountNumber = "123456789", SubAccount = "", Code = "SEP1003", Content = "Payment for TRK1003", TransferType = "IN", Description = "Test", TransferAmount = 200000, Accumulated = 450000, ReferenceCode = "REF1003", OrderId = Guid.Parse("44444444-4444-4444-4444-444444444443"), CreatedAt = baseDate.AddDays(2), IsDeleted = false },
            new Transaction { Id = Guid.Parse("55555555-5555-5555-5555-555555555554"), SePayId = 1004, Gateway = "SePay", TransactionDate = baseDate.AddDays(3).UtcDateTime, AccountNumber = "123456789", SubAccount = "", Code = "SEP1004", Content = "Payment for TRK1004", TransferType = "IN", Description = "Test", TransferAmount = 250000, Accumulated = 700000, ReferenceCode = "REF1004", OrderId = Guid.Parse("44444444-4444-4444-4444-444444444444"), CreatedAt = baseDate.AddDays(3), IsDeleted = false },
            new Transaction { Id = Guid.Parse("55555555-5555-5555-5555-555555555555"), SePayId = 1005, Gateway = "SePay", TransactionDate = baseDate.AddDays(4).UtcDateTime, AccountNumber = "123456789", SubAccount = "", Code = "SEP1005", Content = "Refund for TRK1005", TransferType = "OUT", Description = "Test", TransferAmount = 50000, Accumulated = 650000, ReferenceCode = "REF1005", OrderId = Guid.Parse("44444444-4444-4444-4444-444444444445"), CreatedAt = baseDate.AddDays(4), IsDeleted = false },
            new Transaction { Id = Guid.Parse("55555555-5555-5555-5555-555555555556"), SePayId = 1006, Gateway = "SePay", TransactionDate = baseDate.AddDays(5).UtcDateTime, AccountNumber = "123456789", SubAccount = "", Code = "SEP1006", Content = "Payment for TRK1006", TransferType = "IN", Description = "Test", TransferAmount = 100000, Accumulated = 750000, ReferenceCode = "REF1006", OrderId = Guid.Parse("44444444-4444-4444-4444-444444444446"), CreatedAt = baseDate.AddDays(5), IsDeleted = false },
            new Transaction { Id = Guid.Parse("55555555-5555-5555-5555-555555555557"), SePayId = 1007, Gateway = "SePay", TransactionDate = baseDate.AddDays(6).UtcDateTime, AccountNumber = "123456789", SubAccount = "", Code = "SEP1007", Content = "Payment for TRK1007", TransferType = "IN", Description = "Test", TransferAmount = 300000, Accumulated = 1050000, ReferenceCode = "REF1007", OrderId = Guid.Parse("44444444-4444-4444-4444-444444444447"), CreatedAt = baseDate.AddDays(6), IsDeleted = false }
        );
    }
}
