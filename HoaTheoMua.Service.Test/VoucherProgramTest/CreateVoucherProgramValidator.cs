using HoaTheoMua.Service.VoucherProgram;
using HoaTheoMua.Service.VoucherProgram.Validate;
using Xunit;

namespace HoaTheoMua.Service.Test.VoucherProgramTest;

public class CreateVoucherProgramValidator
{
    [Theory]
    [InlineData("name", "Name", "Name is required")]
    [InlineData("longName", "Name", "Name must not exceed 200 characters")]
    [InlineData("start", "StartAt", "Start time is required")]
    [InlineData("end", "EndAt", "End time is required")]
    [InlineData("equalDates", "EndAt", "End time must be later than start time")]
    [InlineData("reversedDates", "EndAt", "End time must be later than start time")]
    [InlineData("min", "MinOrderAmount", "Min order amount must not be negative")]
    [InlineData("max", "MaxOrderAmount", "Max order amount must not be negative")]
    [InlineData("range", "MaxOrderAmount", "Max order amount cannot be less than min order amount")]
    public void RejectsInvalidFieldsWithPlanMessages(string scenario, string field, string message)
    {
        var request = ValidRequest();
        switch (scenario)
        {
            case "name": request.Name = "   "; break;
            case "longName": request.Name = new string('A', 201); break;
            case "start": request.StartAt = ""; break;
            case "end": request.EndAt = ""; break;
            case "equalDates": request.EndAt = request.StartAt; break;
            case "reversedDates": request.EndAt = "2026-10-14T23:59:59+07:00"; break;
            case "min": request.MinOrderAmount = -1; break;
            case "max": request.MaxOrderAmount = -1; break;
            case "range": request.MinOrderAmount = 200000; request.MaxOrderAmount = 199999; break;
        }

        var result = new CreateVoucherProgramRequestValidator().Validate(request);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, x => x.PropertyName == field && x.ErrorMessage == message);
    }

    [Theory]
    [InlineData(null, null)]
    [InlineData(0, null)]
    [InlineData(null, 0)]
    [InlineData(0, 0)]
    [InlineData(200000, 0)]
    [InlineData(200000, 200000)]
    public void AllowsOptionalAndInclusiveOrderAmountBounds(int? min, int? max)
    {
        var request = ValidRequest();
        request.MinOrderAmount = min;
        request.MaxOrderAmount = max;
        Assert.True(new CreateVoucherProgramRequestValidator().Validate(request).IsValid);
    }

    [Fact]
    public void ComparesInstantsAcrossDifferentOffsets()
    {
        var request = ValidRequest();
        request.StartAt = "2026-10-15T10:00:00+07:00";
        request.EndAt = "2026-10-15T04:00:00+00:00";
        Assert.True(new CreateVoucherProgramRequestValidator().Validate(request).IsValid);
    }

    private static Request.CreateVoucherProgramRequest ValidRequest() => new()
    {
        Name = "20/10",
        StartAt = "2026-10-15T00:00:00+07:00",
        EndAt = "2026-10-31T23:59:59+07:00"
    };
}
