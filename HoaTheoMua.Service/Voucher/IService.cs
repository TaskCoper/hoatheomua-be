using HoaTheoMua.Service.Models;

namespace HoaTheoMua.Service.Voucher;

public interface IService
{
    Task<string> CreateVoucher(Request.CreateVoucherRequest request);

    Task<string> DeleteVoucher(Guid id);

    Task<Response.VoucherExportFile> ExportVouchers(string? visibility, List<Guid>? programIds);

    Task<BasePaginationResponse> GetVoucherUsages(Guid id, int pageIndex, int pageSize);

    Task<Response.BulkUpdateResponse> SetVouchersActive(Request.SetVouchersActiveRequest request);

    Task<List<Response.PublicVoucherResponse>> GetPublicVouchers(decimal? productFee);

    Task<Response.EstimateVoucherResponse> EstimateVoucher(Request.EstimateVoucherRequest request);

    Task<BasePaginationResponse> GetVouchers(int pageIndex, int pageSize, string? keyword, string? visibility,
        bool? isActive, Guid? programId);
    
    
    
}