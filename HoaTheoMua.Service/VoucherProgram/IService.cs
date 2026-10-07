using HoaTheoMua.Service.Models;

namespace HoaTheoMua.Service.VoucherProgram;

public interface IService
{
    Task<Response.VoucherProgramResponse> CreateVoucherProgram(
        Request.CreateVoucherProgramRequest request, CancellationToken cancellationToken = default);
    Task<BasePaginationResponse> GetVoucherPrograms(
        Request.GetVoucherProgramsRequest request, CancellationToken cancellationToken = default);
}
