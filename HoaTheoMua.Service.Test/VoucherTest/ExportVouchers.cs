using HoaTheoMua.Repository.Enum;
using HoaTheoMua.Service.Exceptions;
using MiniExcelLibs;
using Xunit;
using VoucherService = HoaTheoMua.Service.Voucher.Service;

namespace HoaTheoMua.Service.Test.VoucherTest;

public class ExportVouchersTest
{
    [Fact]
    public async Task Export_StreamsTempFileFromStart_AndDeletesItOnDispose()
    {
        await using var db = VoucherTestData.Database();
        db.Vouchers.AddRange(
            VoucherTestData.Voucher("PUBLIC1"),
            VoucherTestData.Voucher("PRIVATE1", visibility: VoucherVisibility.Private));
        await db.SaveChangesAsync();

        var file = await new VoucherService(db).ExportVouchers("public", null);

        var fileStream = Assert.IsType<FileStream>(file.Content);
        var path = fileStream.Name;
        Assert.Equal(0, file.Content.Position);
        Assert.True(File.Exists(path));
        Assert.StartsWith("vouchers-public-", file.FileName);

        var rows = (await file.Content.QueryAsync(useHeaderRow: true)).ToList();
        var row = Assert.Single(rows);
        Assert.Equal("PUBLIC1", (string)((IDictionary<string, object>)row)["Code"]);

        await file.Content.DisposeAsync();
        Assert.False(File.Exists(path));
    }

    [Fact]
    public async Task Export_InvalidRequest_ThrowsBeforeCreatingAFile()
    {
        await using var db = VoucherTestData.Database();
        var before = Directory.GetFiles(Path.GetTempPath(), "voucher-export-*.xlsx").Length;

        await Assert.ThrowsAsync<ValidationExceptionErrorV1>(
            () => new VoucherService(db).ExportVouchers("everyone", null));

        Assert.Equal(before, Directory.GetFiles(Path.GetTempPath(), "voucher-export-*.xlsx").Length);
    }
}
