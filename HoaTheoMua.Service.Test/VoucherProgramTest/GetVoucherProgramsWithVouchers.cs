using HoaTheoMua.Service.Exceptions;
using Xunit;
using VoucherProgramService = HoaTheoMua.Service.VoucherProgram.Service;

namespace HoaTheoMua.Service.Test.VoucherProgramTest;

public class GetVoucherProgramsWithVouchersTest
{
    [Fact]
    public async Task KeywordFiltersByProgramName_IgnoringCase()
    {
        await using var db = VoucherProgramTestData.Database();
        var autumn = NamedProgram("Autumn sale");
        var winter = NamedProgram("Winter sale");
        db.VoucherPrograms.AddRange(autumn, winter);
        await db.SaveChangesAsync();

        var result = await new VoucherProgramService(db).GetDetail(autumn.Id);

        Assert.Equal(autumn.Id, result.Id);
    }

    [Fact]
    public async Task ProgramIdFiltersOneProgram()
    {
        await using var db = VoucherProgramTestData.Database();
        var first = NamedProgram("First");
        var second = NamedProgram("Second");
        db.VoucherPrograms.AddRange(first, second);
        await db.SaveChangesAsync();

        var result = await new VoucherProgramService(db).GetDetail(second.Id);

        Assert.Equal(second.Id, result.Id);
    }

    [Fact]
    public async Task DeletedProgram_IsExcludedEvenWhenProgramIdMatches()
    {
        await using var db = VoucherProgramTestData.Database();
        var deleted = NamedProgram("Old sale");
        deleted.IsDeleted = true;
        db.VoucherPrograms.Add(deleted);
        await db.SaveChangesAsync();

        await Assert.ThrowsAsync<NotFoundException>(() => new VoucherProgramService(db).GetDetail(deleted.Id));
    }

    [Fact]
    public async Task ReturnsOnlyNotDeletedVouchersOfEachProgram()
    {
        await using var db = VoucherProgramTestData.Database();
        var first = NamedProgram("First");
        var second = NamedProgram("Second");
        var firstVoucher = VoucherProgramTestData.NewVoucher(first.Id, "FIRST1");
        var firstDeleted = VoucherProgramTestData.NewVoucher(first.Id, "FIRST2", deleted: true);
        var secondVoucher = VoucherProgramTestData.NewVoucher(second.Id, "SECOND1");
        var standalone = VoucherProgramTestData.NewVoucher(null, "ALONE1");
        db.VoucherPrograms.AddRange(first, second);
        db.Vouchers.AddRange(firstVoucher, firstDeleted, secondVoucher, standalone);
        await db.SaveChangesAsync();

        var service = new VoucherProgramService(db);
        var firstItem = await service.GetDetail(first.Id);
        var secondItem = await service.GetDetail(second.Id);

        Assert.Equal(1, firstItem.VoucherCount);
        Assert.Equal(1, secondItem.VoucherCount);
    }

    private static Repository.Entity.VoucherProgram NamedProgram(string name)
    {
        var program = VoucherProgramTestData.Program();
        program.Name = name;
        return program;
    }
}
