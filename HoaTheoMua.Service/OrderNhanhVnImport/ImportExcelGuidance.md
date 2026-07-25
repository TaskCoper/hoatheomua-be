# Hướng dẫn điều chỉnh hàm ImportFromExcelOrderNhanhVn

Tệp này hướng dẫn cách cập nhật hàm `ImportFromExcelOrderNhanhVn` trong `HoaTheoMua.Service/OrderNhanhVnImport/Service.cs` để nhập dữ liệu từ tệp Excel vào hai bảng `OrderNhanhVn` và `OrderNhanhVnDetail` một cách chính xác, xử lý mối quan hệ một-nhiều (một đơn hàng có nhiều chi tiết sản phẩm).

## 1. Cấu trúc tệp Excel dự kiến

Chúng ta giả định tệp Excel có cấu trúc phẳng, nơi mỗi dòng có thể chứa thông tin đơn hàng và thông tin sản phẩm. Các dòng có cùng `ID Đơn hàng Nhanh` sẽ thuộc về cùng một đơn hàng. Nếu một đơn hàng có nhiều sản phẩm, sẽ có nhiều dòng với cùng `ID Đơn hàng Nhanh`, mỗi dòng đại diện cho một chi tiết sản phẩm khác nhau.

**Ví dụ cấu trúc Excel:**

| ID Đơn hàng Nhanh | Tên khách hàng | ... (các trường OrderNhanhVn khác) ... | Tên sản phẩm | Số lượng | Giá bán | ... (các trường OrderNhanhVnDetail khác) ... |
|-------------------|----------------|-----------------------------------------|--------------|----------|---------|--------------------------------------------------|
| DH001             | Khách A        | ...                                     | SP001        | 2        | 100000  | ...                                              |
| DH001             | Khách A        | ...                                     | SP002        | 1        | 250000  | ...                                              |
| DH002             | Khách B        | ...                                     | SP003        | 3        | 50000   | ...                                              |

## 2. Cập nhật DTO trong `Request.cs`

Tệp `Request.cs` hiện tại có thể chứa `ExcelRow` DTO. Vì MiniExcel đọc tất cả các cột vào một cấu trúc phẳng, `ExcelRow` cần chứa tất cả các trường có thể có từ cả `OrderNhanhVn` và `OrderNhanhVnDetail`.

**`HoaTheoMua.Service/OrderNhanhVnImport/Request.cs` (cập nhật)**

```csharp
namespace HoaTheoMua.Service.OrderNhanhVnImport;

public static class Request
{
    public class ExcelRow
    {
        // Add SourceRowNumber for error reporting
        public int SourceRowNumber { get; set; }

        // Fields from OrderNhanhVn
        public string? IdDonHangNhanh { get; set; }
        public string? IdDonHangApi { get; set; }
        public string? LoaiDon { get; set; }
        public string? ThoiGianDonHang { get; set; } // string for parsing
        public string? NguoiTaoDon { get; set; }
        public string? CuaHang { get; set; }
        public string? Nguon { get; set; }
        public string? KhoTaoDon { get; set; }
        public string? ThanhPhoKhoHang { get; set; }
        public string? QuanHuyenKhoHang { get; set; }
        public string? KhoLayHang { get; set; }
        public string? ThanhPhoLayHang { get; set; }
        public string? QuanHuyenLayHang { get; set; }
        public string? TenKhachHang { get; set; }
        public string? MaKhachHang { get; set; }
        public string? SoDienThoaiKhachHang { get; set; }
        public string? EmailKhachHang { get; set; }
        public string? CapDoKhachHang { get; set; }
        public string? NgaySinhKhachHang { get; set; } // string for parsing
        public string? ThanhPhoKhachHang { get; set; }
        public string? QuanHuyenKhachHang { get; set; }
        public string? PhuongXaKhachHang { get; set; }
        public string? DiaChiKhachHang { get; set; }
        public string? TongKhoiLuong { get; set; } // string for parsing
        public string? GiaTriDonHang { get; set; } // string for parsing
        public string? PhiVanChuyen { get; set; } // string for parsing
        public string? PhiVuotCan { get; set; } // string for parsing
        public string? PhiKhaiGia { get; set; } // string for parsing
        public string? PhiChuyenHoan { get; set; } // string for parsing
        public string? PhiThuTienHo { get; set; } // string for parsing
        public string? PhiShipBaoKhach { get; set; } // string for parsing
        public string? CongVanChuyen { get; set; }
        public string? HangVanChuyen { get; set; }
        public string? DichVuVanChuyen { get; set; }
        public string? SoNgayChuyenHang { get; set; } // string for parsing
        public string? MaVanDon { get; set; }
        public string? BienBan { get; set; }
        public string? TienChuyenKhoan { get; set; } // string for parsing
        public string? TaiKhoanChuyenKhoan { get; set; }
        public string? TienChietKhau { get; set; } // string for parsing
        public string? TienDatCoc { get; set; } // string for parsing
        public string? TaiKhoanDatCoc { get; set; }
        public string? TongThu { get; set; } // string for parsing
        public string? TaiKhoanQuetThe { get; set; }
        public string? TienQuetThe { get; set; } // string for parsing
        public string? MaGiaoDichQuetThe { get; set; }
        public string? GiaVonDonHang { get; set; } // This field was in OrderNhanhVn, now often aggregated or moved to detail
        public string? GhiChuNoiBo { get; set; }
        public string? GhiChuCuaKhach { get; set; }
        public string? TrangThaiHoaDonDienTu { get; set; }
        public string? SoHoaDonDienTu { get; set; }
        public string? NgayXuatHoaDonDienTu { get; set; } // string for parsing
        public string? MaCoQuanThueHoaDonDienTu { get; set; }
        public string? TrangThaiDonHang { get; set; }
        public string? LyDoHuy { get; set; }
        public string? NhanVienChamSocKhachHang { get; set; }
        public string? NgayXacNhan { get; set; } // string for parsing
        public string? NgayGuiHangVanChuyen { get; set; } // string for parsing
        public string? NgayBanGiao { get; set; } // string for parsing
        public string? NgayThanhCong { get; set; } // string for parsing
        public string? GioThanhCong { get; set; }
        public string? NguonDonHang { get; set; }
        public string? GhiChuHangVanChuyen { get; set; }
        public string? CanNangHangVanChuyen { get; set; } // string for parsing
        public string? MaGiamGia { get; set; }
        public string? IdDonTra { get; set; }
        public string? IdDonGoc { get; set; }
        public string? SoThuTuTheoNgay { get; set; } // string for parsing
        public string? BienBanBanGiao { get; set; }
        public string? MaAffiliate { get; set; }
        public string? NhomKhachHang { get; set; }
        public string? TienVat { get; set; } // string for parsing
        public string? TyLeVat { get; set; }
        public string? TienTieuDiem { get; set; } // string for parsing
        public string? Nhan { get; set; }
        public string? NgayHenThanhToan { get; set; } // string for parsing
        public string? TrangThaiDoiSoat { get; set; }
        public string? NgayDoiSoat { get; set; } // string for parsing
        public string? TrangThaiThanhToanDoanhNghiep { get; set; }
        public string? NgayThanhToanDoanhNghiep { get; set; } // string for parsing
        public string? NhanVienBanHang { get; set; }
        public string? LoiNhuan { get; set; } // string for parsing
        public string? TyLeLoiNhuan { get; set; }
        public string? FacebookPsid { get; set; }
        public string? FacebookPageUserId { get; set; }
        public string? SoDienThoaiFacebook { get; set; }
        public string? GiaTriFacebook { get; set; }
        // string? NhanVienDongGoi { get; set; } // This field was in OrderNhanhVn, now moved to detail
        public string? IdPhapNhan { get; set; }
        public string? MaSoThue { get; set; }
        public string? EmailNguoiLienHe { get; set; }
        public string? EmailNguoiLienHeThuHai { get; set; }
        public string? SoDienThoaiNguoiLienHe { get; set; }
        public string? TenNguoiLienHe { get; set; }
        public string? MaThamChieu { get; set; }
        public string? DiaChiKhachHangTrenHoaDonDienTu { get; set; }
        public string? TenKhachHangTrenHoaDonDienTu { get; set; }
        public string? LoaiKhachHangHoaDon { get; set; }
        public string? CoLayHoaDon { get; set; }
        public string? MaCoQuanThueHoaDon { get; set; }
        public string? SoHoaDon { get; set; }
        public string? NgayPhatHanhHoaDon { get; set; } // string for parsing

        // Fields from OrderNhanhVnDetail (now in ExcelRow as well)
        public string? TenSanPham { get; set; }
        public string? QuaTangKem { get; set; }
        public string? Imei { get; set; }
        public string? LoHang { get; set; }
        public string? MaSanPhamCha { get; set; }
        public string? TenSanPhamCha { get; set; }
        public string? MaSanPham { get; set; }
        public string? MaVach { get; set; }
        public string? DanhMucSanPham { get; set; }
        public string? KhoiLuongSanPham { get; set; } // string for parsing
        public string? GiaBan { get; set; } // string for parsing
        public string? LoaiGiaSanPham { get; set; }
        public string? GiaBanTruocVat { get; set; } // string for parsing
        public string? TienVatSanPham { get; set; } // string for parsing
        public string? SoLuong { get; set; } // string for parsing
        public string? ChietKhauMotSanPham { get; set; } // string for parsing
        public string? ChietKhauSanPham { get; set; } // string for parsing
        public string? GiaSanPhamSauChietKhau { get; set; } // string for parsing
        public string? GhiChuSanPham { get; set; }
        public string? DonViTinh { get; set; }
        public string? GiaVon { get; set; } // string for parsing
        public string? SoLuongDonViNhoNhat { get; set; } // string for parsing
        public string? NhanVienDongGoi { get; set; } // This field was in OrderNhanhVn, now moved to detail
    }

    public class ImportResult
    {
        public int TotalRecords { get; set; }
        public int InsertedCount { get; set; }
        public int UpdatedCount { get; set; }
        public List<string> Errors { get; set; } = new List<string>();
    }
}
```

## 3. Điều chỉnh Logic nhập Excel trong `Service.cs`

Logic chính cần được điều chỉnh là cách nhóm các dòng Excel thành các đơn hàng riêng lẻ và sau đó tạo các chi tiết sản phẩm cho mỗi đơn hàng.

**`HoaTheoMua.Service/OrderNhanhVnImport/Service.cs` (cập nhật)**

```csharp
using System.Globalization;
using HoaTheoMua.Repository;
using HoaTheoMua.Service.Exceptions;
using Microsoft.EntityFrameworkCore;
using MiniExcelLibs;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HoaTheoMua.Service.OrderNhanhVnImport;

using OrderEntity = HoaTheoMua.Repository.Entity.OrderNhanhVn;
using OrderDetailEntity = HoaTheoMua.Repository.Entity.OrderNhanhVnDetail;

public class Service : IService
{
    private readonly AppDbContext _dbContext;

    public Service(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Response.ImportResult> ImportFromExcelOrderNhanhVn(Stream fileStream)
    {
        var result = new Response.ImportResult();

        try
        {
            using var ms = await BufferStreamAsync(fileStream);
            await ValidateFileStructureAsync(ms);

            var rows = await ReadRowsAsync(ms);
            result.TotalRecords = rows.Count;

            var importBatch = await BuildUpsertBatchAsync(rows);
            await PersistBatchAsync(importBatch);

            result.InsertedCount = importBatch.ToInsert.Count;
            result.UpdatedCount = importBatch.ToUpdate.Count;
        }
        catch (AppException) { throw; }
        catch (Exception ex)
        {
            Console.WriteLine($"Import error: {ex.Message}");
            throw new BadRequestException($"Excel processing error: {ex.Message}");
        }

        return result;
    }

    private static async Task<MemoryStream> BufferStreamAsync(Stream source)
    {
        var ms = new MemoryStream();
        await source.CopyToAsync(ms);
        ms.Position = 0; // Reset position after copying
        return ms;
    }

    private static async Task ValidateFileStructureAsync(MemoryStream ms)
    {
        // Keep original validation or refine based on your actual Excel structure
        ms.Position = 0;
        var rawRows = await ms.QueryAsync(useHeaderRow: false);
        var headerRow = rawRows.FirstOrDefault() as IDictionary<string, object?>;

        if (headerRow is not { Count: > 0 })
            throw new BadRequestException("Excel file is empty");

        // Example: Check for key headers
        var expectedHeaders = new List<string> { "ID Đơn hàng Nhanh", "Tên khách hàng", "Tên sản phẩm", "Số lượng" };
        foreach (var header in expectedHeaders)
        {
            if (!headerRow.Any(h => h.Value?.ToString()?.Equals(header, StringComparison.OrdinalIgnoreCase) ?? false))
            {
                 throw new BadRequestException($"Invalid file format. Missing expected header: '{header}'. Please upload an Excel file exported from Nhanh.vn.");
            }
        }
    }

    private async Task<List<Request.ExcelRow>> ReadRowsAsync(MemoryStream ms)
    {
        ms.Position = 0;
        var rows = (await ms.QueryAsync<Request.ExcelRow>()).ToList();

        if (rows.Count == 0)
            throw new BadRequestException("Excel file contains no data");

        for (var i = 0; i < rows.Count; i++)
            rows[i].SourceRowNumber = i + 2; // +2 for 1-based indexing and header row

        return rows;
    }

    private struct ImportBatch
    {
        public List<OrderEntity> ToInsert;
        public List<OrderEntity> ToUpdate;
        public List<OrderDetailEntity> OrderDetailsToInsert;
        public List<OrderDetailEntity> OrderDetailsToUpdate; // If you need to update existing details
    }

    private async Task<ImportBatch> BuildUpsertBatchAsync(List<Request.ExcelRow> excelRows)
    {
        var batch = new ImportBatch
        {
            ToInsert = new List<OrderEntity>(),
            ToUpdate = new List<OrderEntity>(),
            OrderDetailsToInsert = new List<OrderDetailEntity>(),
            OrderDetailsToUpdate = new List<OrderDetailEntity>() // Initialize if needed
        };

        // Group excel rows by unique order identifier (IdDonHangNhanh)
        var groupedOrders = excelRows
            .Where(r => !string.IsNullOrWhiteSpace(r.IdDonHangNhanh))
            .GroupBy(r => r.IdDonHangNhanh!)
            .ToList();

        // Fetch existing orders once
        var orderIdsInFile = groupedOrders.Select(g => g.Key).ToList();
        var existingOrdersMap = await _dbContext.OrderNhanhVns
            .Include(o => o.OrderNhanhVnDetails) // Include details for existing orders
            .Where(o => orderIdsInFile.Contains(o.IdDonHangNhanh!) && !o.IsDeleted)
            .ToDictionaryAsync(o => o.IdDonHangNhanh!);

        foreach (var orderGroup in groupedOrders)
        {
            var firstRowInGroup = orderGroup.First();
            var orderIdNhanh = orderGroup.Key;

            var order = ResolveOrder(firstRowInGroup, existingOrdersMap, out bool isUpdate);

            // Apply order-level fields from the first row of the group
            ApplyOrderLevelFields(firstRowInGroup, order);

            if (isUpdate)
            {
                _dbContext.Entry(order).State = EntityState.Modified;
                batch.ToUpdate.Add(order);
            }
            else
            {
                order.Id = Guid.NewGuid();
                order.CreatedAt = DateTimeOffset.UtcNow;
                batch.ToInsert.Add(order);
            }

            // Clear existing details if updating to avoid duplicates or stale data
            if (isUpdate)
            {
                foreach (var existingDetail in order.OrderNhanhVnDetails.ToList()) // .ToList() to avoid modifying collection while iterating
                {
                    _dbContext.OrderNhanhVnDetails.Remove(existingDetail);
                }
                order.OrderNhanhVnDetails.Clear(); // Clear in-memory collection
            }
            
            // Now, process product details for this order
            foreach (var detailRow in orderGroup)
            {
                // Only create a detail if it has a product name (or some other key indicator)
                if (!string.IsNullOrWhiteSpace(detailRow.TenSanPham))
                {
                    var orderDetail = new OrderDetailEntity
                    {
                        Id = Guid.NewGuid(),
                        OrderNhanhVnId = order.Id, // Link to the parent order
                        CreatedAt = DateTimeOffset.UtcNow,
                        SourceRowNumber = detailRow.SourceRowNumber // Keep track of source row for details too
                    };
                    ApplyOrderDetailFields(detailRow, orderDetail);
                    order.OrderNhanhVnDetails.Add(orderDetail); // Add to the order's collection
                    batch.OrderDetailsToInsert.Add(orderDetail); // Add to the batch for insertion
                }
            }
        }
        
        return batch;
    }

    private static OrderEntity ResolveOrder(
        Request.ExcelRow row,
        Dictionary<string, OrderEntity> existingMap,
        out bool isUpdate)
    {
        var cleanId = NullIfEmpty(row.IdDonHangNhanh);

        if (cleanId != null && existingMap.TryGetValue(cleanId, out var existing))
        {
            isUpdate = true;
            return existing;
        }

        isUpdate = false;
        return new OrderEntity
        {
            // Id and CreatedAt are set in BuildUpsertBatchAsync
        };
    }

    /** Áp dụng các trường cấp đơn hàng từ ExcelRow vào OrderNhanhVn entity. */
    private static void ApplyOrderLevelFields(
        Request.ExcelRow row,
        OrderEntity order)
    {
        order.SourceRowNumber = row.SourceRowNumber; // Assume OrderNhanhVn also has SourceRowNumber
        order.IdDonHangNhanh = NullIfEmpty(row.IdDonHangNhanh);
        order.IdDonHangApi = NullIfEmpty(row.IdDonHangApi);
        order.LoaiDon = NullIfEmpty(row.LoaiDon);
        order.ThoiGianDonHang = ParseDateTimeOffset(row.ThoiGianDonHang);
        order.NguoiTaoDon = NullIfEmpty(row.NguoiTaoDon);
        order.CuaHang = NullIfEmpty(row.CuaHang);
        order.Nguon = NullIfEmpty(row.Nguon);
        order.KhoTaoDon = NullIfEmpty(row.KhoTaoDon);
        order.ThanhPhoKhoHang = NullIfEmpty(row.ThanhPhoKhoHang);
        order.QuanHuyenKhoHang = NullIfEmpty(row.QuanHuyenKhoHang);
        order.KhoLayHang = NullIfEmpty(row.KhoLayHang);
        order.ThanhPhoLayHang = NullIfEmpty(row.ThanhPhoLayHang);
        order.QuanHuyenLayHang = NullIfEmpty(row.QuanHuyenLayHang);
        order.TenKhachHang = NullIfEmpty(row.TenKhachHang);
        order.MaKhachHang = NullIfEmpty(row.MaKhachHang);
        order.SoDienThoaiKhachHang = NullIfEmpty(row.SoDienThoaiKhachHang);
        order.EmailKhachHang = NullIfEmpty(row.EmailKhachHang);
        order.CapDoKhachHang = NullIfEmpty(row.CapDoKhachHang);
        order.NgaySinhKhachHang = ParseDateTimeOffset(row.NgaySinhKhachHang);
        order.ThanhPhoKhachHang = NullIfEmpty(row.ThanhPhoKhachHang);
        order.QuanHuyenKhachHang = NullIfEmpty(row.QuanHuyenKhachHang);
        order.PhuongXaKhachHang = NullIfEmpty(row.PhuongXaKhachHang);
        order.DiaChiKhachHang = NullIfEmpty(row.DiaChiKhachHang);
        order.TongKhoiLuong = ParseDecimal(row.TongKhoiLuong);
        order.GiaTriDonHang = ParseDecimal(row.GiaTriDonHang);
        order.PhiVanChuyen = ParseDecimal(row.PhiVanChuyen);
        order.PhiVuotCan = ParseDecimal(row.PhiVuotCan);
        order.PhiKhaiGia = ParseDecimal(row.PhiKhaiGia);
        order.PhiChuyenHoan = ParseDecimal(row.PhiChuyenHoan);
        order.PhiThuTienHo = ParseDecimal(row.PhiThuTienHo);
        order.PhiShipBaoKhach = ParseDecimal(row.PhiShipBaoKhach);
        order.CongVanChuyen = NullIfEmpty(row.CongVanChuyen);
        order.HangVanChuyen = NullIfEmpty(row.HangVanChuyen);
        order.DichVuVanChuyen = NullIfEmpty(row.DichVuVanChuyen);
        order.SoNgayChuyenHang = ParseInt(row.SoNgayChuyenHang);
        order.MaVanDon = NullIfEmpty(row.MaVanDon);
        order.BienBan = NullIfEmpty(row.BienBan);
        order.TienChuyenKhoan = ParseDecimal(row.TienChuyenKhoan);
        order.TaiKhoanChuyenKhoan = NullIfEmpty(row.TaiKhoanChuyenKhoan);
        order.TienChietKhau = ParseDecimal(row.TienChietKhau);
        order.TienDatCoc = ParseDecimal(row.TienDatCoc);
        order.TaiKhoanDatCoc = NullIfEmpty(row.TaiKhoanDatCoc);
        order.TongThu = ParseDecimal(row.TongThu);
        order.TaiKhoanQuetThe = NullIfEmpty(row.TaiKhoanQuetThe);
        order.TienQuetThe = ParseDecimal(row.TienQuetThe);
        order.MaGiaoDichQuetThe = NullIfEmpty(row.MaGiaoDichQuetThe);
        // order.GiaVonDonHang = ParseDecimal(row.GiaVonDonHang); // Moved to detail if it's per-product
        order.GhiChuNoiBo = NullIfEmpty(row.GhiChuNoiBo);
        order.GhiChuCuaKhach = NullIfEmpty(row.GhiChuCuaKhach);
        order.TrangThaiHoaDonDienTu = NullIfEmpty(row.TrangThaiHoaDonDienTu);
        order.SoHoaDonDienTu = NullIfEmpty(row.SoHoaDonDienTu);
        order.NgayXuatHoaDonDienTu = ParseDateTimeOffset(row.NgayXuatHoaDonDienTu);
        order.MaCoQuanThueHoaDonDienTu = NullIfEmpty(row.MaCoQuanThueHoaDonDienTu);
        order.TrangThaiDonHang = NullIfEmpty(row.TrangThaiDonHang);
        order.LyDoHuy = NullIfEmpty(row.LyDoHuy);
        order.NhanVienChamSocKhachHang = NullIfEmpty(row.NhanVienChamSocKhachHang);
        order.NgayXacNhan = ParseDateTimeOffset(row.NgayXacNhan);
        order.NgayGuiHangVanChuyen = ParseDateTimeOffset(row.NgayGuiHangVanChuyen);
        order.NgayBanGiao = ParseDateTimeOffset(row.NgayBanGiao);
        order.NgayThanhCong = ParseDateTimeOffset(row.NgayThanhCong);
        order.GioThanhCong = NullIfEmpty(row.GioThanhCong);
        order.NguonDonHang = NullIfEmpty(row.NguonDonHang);
        order.GhiChuHangVanChuyen = NullIfEmpty(row.GhiChuHangVanChuyen);
        order.CanNangHangVanChuyen = ParseDecimal(row.CanNangHangVanChuyen);
        order.MaGiamGia = NullIfEmpty(row.MaGiamGia);
        order.IdDonTra = NullIfEmpty(row.IdDonTra);
        order.IdDonGoc = NullIfEmpty(row.IdDonGoc);
        order.SoThuTuTheoNgay = ParseInt(row.SoThuTuTheoNgay);
        order.BienBanBanGiao = NullIfEmpty(row.BienBanBanGiao);
        order.MaAffiliate = NullIfEmpty(row.MaAffiliate);
        order.NhomKhachHang = NullIfEmpty(row.NhomKhachHang);
        order.TienVat = ParseDecimal(row.TienVat);
        order.TyLeVat = NullIfEmpty(row.TyLeVat);
        order.TienTieuDiem = ParseDecimal(row.TienTieuDiem);
        order.Nhan = NullIfEmpty(row.Nhan);
        order.NgayHenThanhToan = ParseDateTimeOffset(row.NgayHenThanhToan);
        order.TrangThaiDoiSoat = NullIfEmpty(row.TrangThaiDoiSoat);
        order.NgayDoiSoat = ParseDateTimeOffset(row.NgayDoiSoat);
        order.TrangThaiThanhToanDoanhNghiep = NullIfEmpty(row.TrangThaiThanhToanDoanhNghiep);
        order.NgayThanhToanDoanhNghiep = ParseDateTimeOffset(row.NgayThanhToanDoanhNghiep);
        order.NhanVienBanHang = NullIfEmpty(row.NhanVienBanHang);
        order.LoiNhuan = ParseDecimal(row.LoiNhuan);
        order.TyLeLoiNhuan = NullIfEmpty(row.TyLeLoiNhuan);
        order.FacebookPsid = NullIfEmpty(row.FacebookPsid);
        order.FacebookPageUserId = NullIfEmpty(row.FacebookPageUserId);
        order.SoDienThoaiFacebook = NullIfEmpty(row.SoDienThoaiFacebook);
        order.GiaTriFacebook = NullIfEmpty(row.GiaTriFacebook);
        // order.NhanVienDongGoi = NullIfEmpty(row.NhanVienDongGoi); // Moved to detail
        order.IdPhapNhan = NullIfEmpty(row.IdPhapNhan);
        order.MaSoThue = NullIfEmpty(row.MaSoThue);
        order.EmailNguoiLienHe = NullIfEmpty(row.EmailNguoiLienHe);
        order.EmailNguoiLienHeThuHai = NullIfEmpty(row.EmailNguoiLienHeThuHai);
        order.SoDienThoaiNguoiLienHe = NullIfEmpty(row.SoDienThoaiNguoiLienHe);
        order.TenNguoiLienHe = NullIfEmpty(row.TenNguoiLienHe);
        order.MaThamChieu = NullIfEmpty(row.MaThamChieu);
        order.DiaChiKhachHangTrenHoaDonDienTu = NullIfEmpty(row.DiaChiKhachHangTrenHoaDonDienTu);
        order.TenKhachHangTrenHoaDonDienTu = NullIfEmpty(row.TenKhachHangTrenHoaDonDienTu);
        order.LoaiKhachHangHoaDon = NullIfEmpty(row.LoaiKhachHangHoaDon);
        order.CoLayHoaDon = NullIfEmpty(row.CoLayHoaDon);
        order.MaCoQuanThueHoaDon = NullIfEmpty(row.MaCoQuanThueHoaDon);
        order.SoHoaDon = NullIfEmpty(row.SoHoaDon);
        order.NgayPhatHanhHoaDon = ParseDateTimeOffset(row.NgayPhatHanhHoaDon);
        order.UpdatedAt = DateTimeOffset.UtcNow; // Update timestamp
    }

    /** Áp dụng các trường chi tiết sản phẩm từ ExcelRow vào OrderNhanhVnDetail entity. */
    private static void ApplyOrderDetailFields(
        Request.ExcelRow row,
        OrderDetailEntity orderDetail)
    {
        orderDetail.TenSanPham = NullIfEmpty(row.TenSanPham);
        orderDetail.QuaTangKem = NullIfEmpty(row.QuaTangKem);
        orderDetail.Imei = NullIfEmpty(row.Imei);
        orderDetail.LoHang = NullIfEmpty(row.LoHang);
        orderDetail.MaSanPhamCha = NullIfEmpty(row.MaSanPhamCha);
        orderDetail.TenSanPhamCha = NullIfEmpty(row.TenSanPhamCha);
        orderDetail.MaSanPham = NullIfEmpty(row.MaSanPham);
        orderDetail.MaVach = NullIfEmpty(row.MaVach);
        orderDetail.DanhMucSanPham = NullIfEmpty(row.DanhMucSanPham);
        orderDetail.KhoiLuongSanPham = ParseDecimal(row.KhoiLuongSanPham);
        orderDetail.GiaBan = ParseDecimal(row.GiaBan);
        orderDetail.LoaiGiaSanPham = NullIfEmpty(row.LoaiGiaSanPham);
        orderDetail.GiaBanTruocVat = ParseDecimal(row.GiaBanTruocVat);
        orderDetail.TienVatSanPham = ParseDecimal(row.TienVatSanPham);
        orderDetail.SoLuong = ParseInt(row.SoLuong);
        orderDetail.ChietKhauMotSanPham = ParseDecimal(row.ChietKhauMotSanPham);
        orderDetail.ChietKhauSanPham = ParseDecimal(row.ChietKhauSanPham);
        orderDetail.GiaSanPhamSauChietKhau = ParseDecimal(row.GiaSanPhamSauChietKhau);
        orderDetail.GhiChuSanPham = NullIfEmpty(row.GhiChuSanPham);
        orderDetail.DonViTinh = NullIfEmpty(row.DonViTinh);
        orderDetail.GiaVon = ParseDecimal(row.GiaVon);
        orderDetail.GiaVonDonHang = ParseDecimal(row.GiaVonDonHang); // From OrderNhanhVn to OrderNhanhVnDetail
        orderDetail.SoLuongDonViNhoNhat = ParseInt(row.SoLuongDonViNhoNhat);
        orderDetail.NhanVienDongGoi = NullIfEmpty(row.NhanVienDongGoi); // From OrderNhanhVn to OrderNhanhVnDetail
    }

    /** Lưu batch vào DB trong 1 transaction (rollback nếu lỗi). */
    private async Task PersistBatchAsync(ImportBatch batch)
    {
        await using var tx = await _dbContext.Database.BeginTransactionAsync();
        try
        {
            if (batch.ToInsert.Count > 0)
                await _dbContext.OrderNhanhVns.AddRangeAsync(batch.ToInsert);

            if (batch.ToUpdate.Count > 0)
            {
                // For updates, attach and mark as modified if not already tracked
                foreach (var order in batch.ToUpdate)
                {
                    _dbContext.OrderNhanhVns.Update(order);
                }
            }

            // Order details are added via the parent collection, so SaveChanges will handle them.
            // If there's a need to track details explicitly outside the parent (e.g., for updates),
            // you'd add similar logic here for OrderDetailsToUpdate.

            await _dbContext.SaveChangesAsync();
            await tx.CommitAsync();
        }
        catch
        {
            await tx.RollbackAsync();
            throw;
        }
    }

    private static string? NullIfEmpty(string? val)
    {
        if (string.IsNullOrWhiteSpace(val)) return null;
        var trimmed = val.Trim();
        return trimmed.Equals("null", StringComparison.OrdinalIgnoreCase) ? null : trimmed;
    }

    private static decimal? ParseDecimal(string? val)
    {
        if (string.IsNullOrWhiteSpace(val)) return null;
        val = val.Trim();

        if (decimal.TryParse(val, NumberStyles.Any, CultureInfo.InvariantCulture, out var dec))
            return dec;

        if (decimal.TryParse(val.Replace(',', '.'), NumberStyles.Any, CultureInfo.InvariantCulture, out dec))
            return dec;

        return null;
    }

    private static int? ParseInt(string? val)
    {
        if (string.IsNullOrWhiteSpace(val)) return null;
        val = val.Trim();

        if (double.TryParse(val, NumberStyles.Any, CultureInfo.InvariantCulture, out var dbl))
            return (int)Math.Round(dbl);

        return null;
    }

    private static DateTimeOffset? ParseDateTimeOffset(string? val)
    {
        if (string.IsNullOrWhiteSpace(val)) return null;
        val = val.Trim();

        // Try standard formats first
        if (DateTimeOffset.TryParseExact(val, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal, out var dto))
            return dto.ToUniversalTime();
        if (DateTimeOffset.TryParseExact(val, "dd/MM/yyyy HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal, out dto))
            return dto.ToUniversalTime();
        if (DateTimeOffset.TryParseExact(val, "MM/dd/yyyy HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal, out dto))
            return dto.ToUniversalTime();
        if (DateTimeOffset.TryParseExact(val, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal, out dto))
            return dto.ToUniversalTime();
        if (DateTimeOffset.TryParseExact(val, "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal, out dto))
            return dto.ToUniversalTime();
        if (DateTimeOffset.TryParseExact(val, "MM/dd/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal, out dto))
            return dto.ToUniversalTime();


        // Fallback to general parsing
        if (DateTimeOffset.TryParse(val, CultureInfo.InvariantCulture, DateTimeStyles.None, out dto))
            return dto.ToUniversalTime();

        return null;
    }
}
```

## Giải thích các thay đổi chính:

1.  **`Request.cs` - `ExcelRow` DTO**:
    *   `ExcelRow` đã được cập nhật để bao gồm **tất cả** các trường từ cả `OrderNhanhVn` và `OrderNhanhVnDetail` dưới dạng `string?`. Điều này là cần thiết vì MiniExcel đọc tất cả các cột trong một hàng Excel vào một đối tượng duy nhất.
    *   Các hàm `ParseDecimal`, `ParseInt`, `ParseDateTimeOffset` sẽ chịu trách nhiệm chuyển đổi các giá trị `string` từ Excel sang kiểu dữ liệu phù hợp.
    *   Đã thêm nhiều định dạng cho `ParseDateTimeOffset` để xử lý các định dạng ngày/giờ phổ biến hơn.

2.  **`Service.cs` - `ImportBatch`**:
    *   Cấu trúc `ImportBatch` hiện bao gồm `OrderDetailsToInsert` và `OrderDetailsToUpdate` để quản lý các chi tiết sản phẩm.

3.  **`Service.cs` - `BuildUpsertBatchAsync`**:
    *   Đây là thay đổi quan trọng nhất. Thay vì xử lý từng dòng Excel riêng lẻ như một đơn hàng, giờ đây nó nhóm các `ExcelRow` theo `IdDonHangNhanh`.
    *   `existingOrdersMap` giờ đây sử dụng `.Include(o => o.OrderNhanhVnDetails)` để tải các chi tiết sản phẩm hiện có cùng với đơn hàng chính.
    *   Đối với mỗi nhóm đơn hàng:
        *   Tạo/cập nhật `OrderNhanhVn` entity (chỉ từ dòng đầu tiên của nhóm).
        *   Nếu là cập nhật, tất cả các `OrderNhanhVnDetail` hiện có của đơn hàng đó sẽ bị xóa để đảm bảo dữ liệu mới nhất được thêm vào và tránh trùng lặp hoặc dữ liệu cũ.
        *   Sau đó, nó lặp qua tất cả các dòng trong nhóm (`orderGroup`) để tạo các `OrderDetailEntity` mới. Mỗi `detailRow` trong `orderGroup` đại diện cho một chi tiết sản phẩm.
        *   Các `OrderDetailEntity` mới này được thêm vào tập hợp `OrderNhanhVnDetails` của `OrderNhanhVn` chính và cũng được thêm vào `batch.OrderDetailsToInsert`.

4.  **`Service.cs` - `ApplyOrderLevelFields` và `ApplyOrderDetailFields`**:
    *   `ApplyOrderLevelFields` chỉ ánh xạ các trường thuộc về `OrderNhanhVn`. Các trường `GiaVonDonHang` và `NhanVienDongGoi` đã được chuyển sang `OrderDetailEntity`.
    *   `ApplyOrderDetailFields` là một phương thức mới chịu trách nhiệm ánh xạ các trường cụ thể của chi tiết sản phẩm từ `ExcelRow` vào `OrderDetailEntity`.

5.  **`Service.cs` - `PersistBatchAsync`**:
    *   Phương thức này vẫn xử lý việc thêm/cập nhật `OrderNhanhVn`. Khi bạn thêm các `OrderDetailEntity` vào tập hợp `OrderNhanhVnDetails` của `OrderNhanhVn` chính, EF Core sẽ tự động theo dõi và lưu các chi tiết này khi `_dbContext.SaveChangesAsync()` được gọi.
    *   Đã cập nhật logic để `_dbContext.OrderNhanhVns.Update(order);` được gọi trên từng đơn hàng được cập nhật.

Với những thay đổi này, hàm import của bạn sẽ có thể đọc tệp Excel, nhóm các dòng theo đơn hàng và tạo các entity `OrderNhanhVn` và `OrderNhanhVnDetail` liên quan một cách chính xác.

Sau khi bạn đã áp dụng các thay đổi này, hãy nhớ:
1.  Kiểm tra kỹ các tên cột trong tệp Excel của bạn phải khớp chính xác với các tên thuộc tính trong `Request.ExcelRow`.
2.  Chạy lại các bài kiểm tra (nếu có) hoặc tạo các bài kiểm tra mới để đảm bảo chức năng import hoạt động như mong đợi.

Nếu bạn có bất kỳ câu hỏi nào về các phần cụ thể hoặc cần thêm sự trợ giúp, đừng ngần ngại hỏi!