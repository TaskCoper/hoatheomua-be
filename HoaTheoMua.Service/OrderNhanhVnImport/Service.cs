using System.Globalization;
using HoaTheoMua.Repository;
using HoaTheoMua.Service.Exceptions;
using Microsoft.EntityFrameworkCore;
using MiniExcelLibs;

namespace HoaTheoMua.Service.OrderNhanhVnImport;

using OrderEntity = HoaTheoMua.Repository.Entity.OrderNhanhVn;

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

            var isb = await BuildUpsertBatchAsync(rows);
            await PersistBatchAsync(isb);

            result.InsertedCount = isb.ToInsert.Count;
            result.UpdatedCount = isb.ToUpdate.Count;
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
        return ms;
    }
    
    private static async Task ValidateFileStructureAsync(MemoryStream ms)
    {
        ms.Position = 0;
        var rawRows = await ms.QueryAsync(useHeaderRow: false);
        var headerRow = rawRows.FirstOrDefault() as IDictionary<string, object?>;

        if (headerRow is not { Count: > 0 })
            throw new BadRequestException("Excel file is empty");

        headerRow.TryGetValue("A", out var valA);
        var colA = valA?.ToString() ?? "";

        if (string.IsNullOrWhiteSpace(colA) ||
            !colA.Contains("ID", StringComparison.OrdinalIgnoreCase) ||
            headerRow.Count < 100)
        {
            throw new BadRequestException(
                "Invalid file format. Please upload an Excel file exported from Nhanh.vn.");
        }
    }

    private async Task<List<Request.ExcelRow>> ReadRowsAsync(MemoryStream ms)
    {
        ms.Position = 0;
        var rows = (await ms.QueryAsync<Request.ExcelRow>()).ToList();

        if (rows.Count == 0)
            throw new BadRequestException("Excel file contains no data");

        for (var i = 0; i < rows.Count; i++)
            rows[i].SourceRowNumber = i + 2;

        return rows;
    }

    private struct ImportBatch
    {
        public List<OrderEntity> ToInsert;
        public List<OrderEntity> ToUpdate;
    }

    private async Task<ImportBatch> BuildUpsertBatchAsync(List<Request.ExcelRow> rows)
    {
        var idsInFile = rows
            .Select(r => r.IdDonHangNhanh)
            .Where(id => id != null)
            .Distinct()
            .ToList();
        //lúc này ta có được một list IdDonHangNhanh từ file excel mà ta đã nhờ miniExcel đọc qua
        var existingMap = idsInFile.Count > 0
            ? await _dbContext.OrderNhanhVns
                .Where(o => o.IdDonHangNhanh != null && idsInFile.Contains(o.IdDonHangNhanh) && !o.IsDeleted)
                .ToDictionaryAsync(o => o.IdDonHangNhanh!)
            : new Dictionary<string, OrderEntity>();
        //lúc này ta có một dictionary với key là: IdDonHangNhanh và value là entity tương ứng của nó
        var batch = new ImportBatch
        {
            ToInsert = new List<OrderEntity>(),
            ToUpdate = new List<OrderEntity>()
        };

        foreach (var row in rows)
        {
            var order = ResolveOrder(row, existingMap, out var isUpdate);
            ApplyRowToOrder(row, order);
            order.UpdatedAt = DateTimeOffset.UtcNow;

            if (isUpdate)
                batch.ToUpdate.Add(order);
            else
                batch.ToInsert.Add(order);
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
            Id = Guid.NewGuid(),
            CreatedAt = DateTimeOffset.UtcNow
        };
    }

    /** Áp dụng toàn bộ giá trị từ ExcelRow vào OrderNhanhVn entity (map đủ 124 cột). */
    private static void ApplyRowToOrder(
        Request.ExcelRow row,
        OrderEntity order)
    {
        order.SourceRowNumber = row.SourceRowNumber;
 
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
        order.TenSanPham = NullIfEmpty(row.TenSanPham);
        order.QuaTangKem = NullIfEmpty(row.QuaTangKem);
        order.Imei = NullIfEmpty(row.Imei);
        order.LoHang = NullIfEmpty(row.LoHang);
        order.MaSanPhamCha = NullIfEmpty(row.MaSanPhamCha);
        order.TenSanPhamCha = NullIfEmpty(row.TenSanPhamCha);
        order.MaSanPham = NullIfEmpty(row.MaSanPham);
        order.MaVach = NullIfEmpty(row.MaVach);
        order.DanhMucSanPham = NullIfEmpty(row.DanhMucSanPham);
        order.KhoiLuongSanPham = ParseDecimal(row.KhoiLuongSanPham);
        order.GiaBan = ParseDecimal(row.GiaBan);
        order.LoaiGiaSanPham = NullIfEmpty(row.LoaiGiaSanPham);
        order.GiaBanTruocVat = ParseDecimal(row.GiaBanTruocVat);
        order.TienVatSanPham = ParseDecimal(row.TienVatSanPham);
        order.SoLuong = ParseInt(row.SoLuong);
        order.ChietKhauMotSanPham = ParseDecimal(row.ChietKhauMotSanPham);
        order.ChietKhauSanPham = ParseDecimal(row.ChietKhauSanPham);
        order.GiaSanPhamSauChietKhau = ParseDecimal(row.GiaSanPhamSauChietKhau);
        order.GhiChuSanPham = NullIfEmpty(row.GhiChuSanPham);
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
        order.GiaVon = ParseDecimal(row.GiaVon);
        order.GiaVonDonHang = ParseDecimal(row.GiaVonDonHang);
        order.DonViTinh = NullIfEmpty(row.DonViTinh);
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
        order.SoLuongDonViNhoNhat = ParseInt(row.SoLuongDonViNhoNhat);
        order.NhanVienDongGoi = NullIfEmpty(row.NhanVienDongGoi);
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
                _dbContext.OrderNhanhVns.UpdateRange(batch.ToUpdate);

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

        if (DateTimeOffset.TryParseExact(val, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal, out var dto))
            return dto.ToUniversalTime();

        if (DateTimeOffset.TryParse(val, CultureInfo.InvariantCulture, DateTimeStyles.None, out dto))
            return dto.ToUniversalTime();

        return null;
    }
}