using HoaTheoMua.Service.Models;

namespace HoaTheoMua.Service.VoucherProgram;

public interface IService
{
    Task<string> UpdateVoucherProgram(Guid id, Request.UpdateVoucherProgramRequest request,
        CancellationToken cancellationToken = default);
    Task<string> DeleteVoucherProgram(Guid id, CancellationToken cancellationToken = default);
    Task<Response.GenerateProgramVouchersResponse> GenerateProgramVouchers(
        Guid id, Request.GenerateProgramVouchersRequest request, CancellationToken cancellationToken = default);
    Task<string> CreateVoucherProgram(
        Request.CreateVoucherProgramRequest request, CancellationToken cancellationToken = default);
    Task<BasePaginationResponse> GetVoucherPrograms(
        Request.GetVoucherProgramsRequest request, CancellationToken cancellationToken = default);
    Task<Response.VoucherProgramResponse> GetDetail(Guid id);

    Task<BasePaginationResponse> GetVoucherByProgramId(int pageIndex, int pageSize, string? keyword, Guid programId,
        bool? isActive);
}
