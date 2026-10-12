using HoaTheoMua.Service.VoucherProgram;
using HoaTheoMua.Service.VoucherProgram.Validate;
using Xunit;
using static HoaTheoMua.Service.Test.VoucherProgramTest.VoucherProgramTestData;

namespace HoaTheoMua.Service.Test.VoucherProgramTest;

public class GenerateProgramVouchersValidator
{
    [Fact]
    public void Update_ReportsNegativeMinAndMaxAsSeparateFieldErrors()
    {
        var request = new Request.UpdateVoucherProgramRequest { MinOrderAmount = -1, MaxOrderAmount = -2 };

        var result = new UpdateVoucherProgramRequestValidator().Validate(request);

        Assert.Equal(3, result.Errors.Count);
        Assert.Contains(result.Errors, error => error.PropertyName == "MinOrderAmount"
            && error.ErrorMessage == "Giá trị đơn hàng tối thiểu phải từ 0 trở lên.");
        Assert.Contains(result.Errors, error => error.PropertyName == "MaxOrderAmount"
            && error.ErrorMessage == "Giá trị đơn hàng tối đa phải lớn hơn 0.");
    }

    [Fact]
    public void Generate_ReportsInvalidPrefixAndSuffixAsSeparateFieldErrors()
    {
        var request = GenerateRequest();
        request.CodePrefix = "htm";
        request.CodeSuffix = "-26";

        var result = new GenerateProgramVouchersRequestValidator().Validate(request);

        Assert.Equal(2, result.Errors.Count);
        Assert.Contains(result.Errors, error => error.PropertyName == "CodePrefix"
            && error.ErrorMessage == "Phần đầu mã chỉ được gồm chữ cái viết hoa hoặc chữ số.");
        Assert.Contains(result.Errors, error => error.PropertyName == "CodeSuffix"
            && error.ErrorMessage == "Phần cuối mã chỉ được gồm chữ cái viết hoa hoặc chữ số.");
    }

    [Theory]
    [InlineData(5, 1, "cash")]
    [InlineData(10, 1000, "percent")]
    public async Task AcceptsValidBoundariesAndOptionalFields(int length, int quantity, string type)
    {
        var request = GenerateRequest(quantity);
        request.CodeLength = length;
        request.CodePrefix = new string('A', 20 - length);
        request.CodeSuffix = null;
        request.DiscountType = type;
        request.DiscountValue = type == "percent" ? 100 : 50000;
        request.MaxDiscountAmount = null;
        request.Description = null;

        Assert.True((await new GenerateProgramVouchersRequestValidator().ValidateAsync(request)).IsValid);
    }

    [Theory]
    [InlineData("CodeLength", "4", "Phần mã ngẫu nhiên phải dài ít nhất 5 ký tự.")]
    [InlineData("CodeLength", "11", "Phần mã ngẫu nhiên không được dài quá 10 ký tự.")]
    [InlineData("CodeLength", "0", "Phần mã ngẫu nhiên phải dài ít nhất 5 ký tự.")]
    [InlineData("CodePrefix", "htm", "Phần đầu mã chỉ được gồm chữ cái viết hoa hoặc chữ số.")]
    [InlineData("CodePrefix", "HTM\n", "Phần đầu mã chỉ được gồm chữ cái viết hoa hoặc chữ số.")]
    [InlineData("CodeSuffix", "-26", "Phần cuối mã chỉ được gồm chữ cái viết hoa hoặc chữ số.")]
    [InlineData("CodeSuffix", "AAAAAAAAAAAAAAAAAAAA", "Mã voucher không được dài quá 20 ký tự, tính cả phần đầu và phần cuối mã.", "CodeLength")]
    [InlineData("Quantity", "0", "Số lượng voucher cần tạo phải từ 1 trở lên.")]
    [InlineData("Quantity", "1001", "Mỗi lần chỉ được tạo tối đa 1.000 voucher.")]
    [InlineData("MaxUsageCount", "0", "Giới hạn lượt sử dụng phải lớn hơn 0.")]
    [InlineData("DiscountType", "PERCENT", "Loại giảm giá chỉ chấp nhận cash (giảm tiền) hoặc percent (giảm theo phần trăm).")]
    [InlineData("DiscountType", "", "Vui lòng chọn loại giảm giá.")]
    [InlineData("DiscountValue", "0", "Mức giảm giá phải lớn hơn 0.")]
    [InlineData("DiscountValue", "101", "Mức giảm theo phần trăm không được vượt quá 100%.")]
    [InlineData("MaxDiscountAmount", "0", "Mức giảm tối đa phải lớn hơn 0.")]
    [InlineData("MaxDiscountAmount", "-1", "Mức giảm tối đa phải lớn hơn 0.")]
    [InlineData("DiscountType", "cash", "Chỉ đặt mức giảm tối đa khi giảm theo phần trăm.", "MaxDiscountAmount")]
    [InlineData("Description", "long", "Mô tả không được dài quá 500 ký tự.")]
    public async Task RejectsInvalidGenerationFields(string field, string value, string message, string? errorField = null)
    {
        var request = GenerateRequest();
        switch (field)
        {
            case "CodeLength": request.CodeLength = int.Parse(value); break;
            case "CodePrefix": request.CodePrefix = value; break;
            case "CodeSuffix": request.CodeSuffix = value; break;
            case "Quantity": request.Quantity = int.Parse(value); break;
            case "MaxUsageCount": request.MaxUsageCount = int.Parse(value); break;
            case "DiscountType": request.DiscountType = value; break;
            case "DiscountValue": request.DiscountValue = decimal.Parse(value); break;
            case "MaxDiscountAmount": request.MaxDiscountAmount = decimal.Parse(value); break;
            case "Description": request.Description = new string('A', 501); break;
        }

        var result = await new GenerateProgramVouchersRequestValidator().ValidateAsync(request);

        Assert.Contains(result.Errors, error => error.PropertyName == (errorField ?? field) && error.ErrorMessage == message);
    }

    [Theory]
    [InlineData(null, false)]
    [InlineData(false, true)]
    [InlineData(true, true)]
    public async Task Update_RequiresAtLeastOneField_AcceptsBothBooleanValues(bool? active, bool valid)
    {
        var result = await new UpdateVoucherProgramRequestValidator()
            .ValidateAsync(new Request.UpdateVoucherProgramRequest { IsActive = active });

        Assert.Equal(valid, result.IsValid);
        if (!valid)
            Assert.Equal("Vui lòng nhập ít nhất một thông tin cần cập nhật cho chương trình voucher.", Assert.Single(result.Errors).ErrorMessage);
    }
}
