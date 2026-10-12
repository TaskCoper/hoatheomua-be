using HoaTheoMua.Service.VoucherProgram;
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

        var result = await new VoucherProgramService(db)
            .GetVoucherProgramsWithVouchers(1, 10, "  AUTUMN ", null);

        var item = Assert.Single(result.Value.Items.Cast<Response.VoucherProgramDetailResponse>());
        Assert.Equal(autumn.Id, item.Id);
        Assert.Equal(1, result.Value.TotalCount);
    }

    [Fact]
    public async Task ProgramIdFiltersOneProgram()
    {
        await using var db = VoucherProgramTestData.Database();
        var first = NamedProgram("First");
        var second = NamedProgram("Second");
        db.VoucherPrograms.AddRange(first, second);
        await db.SaveChangesAsync();

        var result = await new VoucherProgramService(db)
            .GetVoucherProgramsWithVouchers(1, 10, null, second.Id);

        var item = Assert.Single(result.Value.Items.Cast<Response.VoucherProgramDetailResponse>());
        Assert.Equal(second.Id, item.Id);
    }

    [Fact]
    public async Task DeletedProgram_IsExcludedEvenWhenProgramIdMatches()
    {
        await using var db = VoucherProgramTestData.Database();
        var deleted = NamedProgram("Old sale");
        deleted.IsDeleted = true;
        db.VoucherPrograms.Add(deleted);
        await db.SaveChangesAsync();

        var result = await new VoucherProgramService(db)
            .GetVoucherProgramsWithVouchers(1, 10, null, deleted.Id);

        Assert.Empty(result.Value.Items);
        Assert.Equal(0, result.Value.TotalCount);
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

        var result = await new VoucherProgramService(db)
            .GetVoucherProgramsWithVouchers(1, 10, null, null);

        var items = result.Value.Items.Cast<Response.VoucherProgramDetailResponse>().ToArray();
        Assert.Equal(2, items.Length);
        var firstItem = items.Single(x => x.Id == first.Id);
        var secondItem = items.Single(x => x.Id == second.Id);
        Assert.Equal(new[] { firstVoucher.Id }, firstItem.Vouchers.Select(x => x.Id));
        Assert.Equal(new[] { secondVoucher.Id }, secondItem.Vouchers.Select(x => x.Id));
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
