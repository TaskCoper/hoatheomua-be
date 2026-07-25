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

    // ── Orchestration helpers ──────────────────────────────────────────

    /** Sao chép file stream vào MemoryStream để có thể seek (đọc nhiều lần). */
    private static async Task<MemoryStream> BufferStreamAsync(Stream source)
    {
        var ms = new MemoryStream();
        await source.CopyToAsync(ms);
        return ms;
    }

    /** Kiểm tra dòng header: cột A phải chứa "ID" và file phải có 100+ cột. */
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

    /** Parse dòng Excel qua MiniExcel và gán SourceRowNumber để truy vết. */
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

    // ── Upsert batch ───────────────────────────────────────────────────

    private struct ImportBatch
    {
        public List<OrderEntity> ToInsert;
        public List<OrderEntity> ToUpdate;
    }

    /** Nạp đơn hàng từ DB theo IdDonHangNhanh, phân loại từng dòng là insert hay update. */
    private async Task<ImportBatch> BuildUpsertBatchAsync(List<Request.ExcelRow> rows)
    {
        var idsInFile = rows
            .Select(r => r.IdDonHangNhanh)
            .Where(id => id.HasValue)
            .Select(id => id!.Value)
            .Distinct()
            .ToList();
        //lúc này ta có được một list IdDonHangNhanh từ file excel mà ta đã nhờ miniExcel đọc qua
        var existingMap = idsInFile.Count > 0
            ? await _dbContext.OrderNhanhVns
                .Where(o => o.IdDonHangNhanh != null && idsInFile.Contains(o.IdDonHangNhanh.Value))
                .ToDictionaryAsync(o => o.IdDonHangNhanh!.Value)
            : new Dictionary<long, OrderEntity>();
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

    /** Tìm order theo IdDonHangNhanh, nếu có thì update, không có thì tạo mới. */
    private static OrderEntity ResolveOrder(
        Request.ExcelRow row,
        Dictionary<long, OrderEntity> existingMap,
        out bool isUpdate)
    {
        if (row.IdDonHangNhanh.HasValue && existingMap.TryGetValue(row.IdDonHangNhanh.Value, out var existing))
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

        order.IdDonHangNhanh = row.IdDonHangNhanh;
        order.IdDonHangApi = row.IdDonHangApi;
        order.LoaiDon = NullIfEmpty(row.LoaiDon);
        order.ThoiGianDonHang = row.ThoiGianDonHang;
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
        order.NgaySinhKhachHang = row.NgaySinhKhachHang;
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
        order.KhoiLuongSanPham = row.KhoiLuongSanPham;
        order.GiaBan = row.GiaBan;
        order.LoaiGiaSanPham = NullIfEmpty(row.LoaiGiaSanPham);
        order.GiaBanTruocVat = row.GiaBanTruocVat;
        order.TienVatSanPham = row.TienVatSanPham;
        order.SoLuong = row.SoLuong;
        order.ChietKhauMotSanPham = row.ChietKhauMotSanPham;
        order.ChietKhauSanPham = row.ChietKhauSanPham;
        order.GiaSanPhamSauChietKhau = row.GiaSanPhamSauChietKhau;
        order.GhiChuSanPham = NullIfEmpty(row.GhiChuSanPham);
        order.TongKhoiLuong = row.TongKhoiLuong;
        order.GiaTriDonHang = row.GiaTriDonHang;
        order.PhiVanChuyen = row.PhiVanChuyen;
        order.PhiVuotCan = row.PhiVuotCan;
        order.PhiKhaiGia = row.PhiKhaiGia;
        order.PhiChuyenHoan = row.PhiChuyenHoan;
        order.PhiThuTienHo = row.PhiThuTienHo;
        order.PhiShipBaoKhach = row.PhiShipBaoKhach;
        order.CongVanChuyen = NullIfEmpty(row.CongVanChuyen);
        order.HangVanChuyen = NullIfEmpty(row.HangVanChuyen);
        order.DichVuVanChuyen = NullIfEmpty(row.DichVuVanChuyen);
        order.SoNgayChuyenHang = row.SoNgayChuyenHang;
        order.MaVanDon = NullIfEmpty(row.MaVanDon);
        order.BienBan = NullIfEmpty(row.BienBan);
        order.TienChuyenKhoan = row.TienChuyenKhoan;
        order.TaiKhoanChuyenKhoan = NullIfEmpty(row.TaiKhoanChuyenKhoan);
        order.TienChietKhau = row.TienChietKhau;
        order.TienDatCoc = row.TienDatCoc;
        order.TaiKhoanDatCoc = NullIfEmpty(row.TaiKhoanDatCoc);
        order.TongThu = row.TongThu;
        order.TaiKhoanQuetThe = NullIfEmpty(row.TaiKhoanQuetThe);
        order.TienQuetThe = row.TienQuetThe;
        order.MaGiaoDichQuetThe = NullIfEmpty(row.MaGiaoDichQuetThe);
        order.GiaVon = row.GiaVon;
        order.GiaVonDonHang = row.GiaVonDonHang;
        order.DonViTinh = NullIfEmpty(row.DonViTinh);
        order.GhiChuNoiBo = NullIfEmpty(row.GhiChuNoiBo);
        order.GhiChuCuaKhach = NullIfEmpty(row.GhiChuCuaKhach);
        order.TrangThaiHoaDonDienTu = NullIfEmpty(row.TrangThaiHoaDonDienTu);
        order.SoHoaDonDienTu = NullIfEmpty(row.SoHoaDonDienTu);
        order.NgayXuatHoaDonDienTu = row.NgayXuatHoaDonDienTu;
        order.MaCoQuanThueHoaDonDienTu = NullIfEmpty(row.MaCoQuanThueHoaDonDienTu);
        order.TrangThaiDonHang = NullIfEmpty(row.TrangThaiDonHang);
        order.LyDoHuy = NullIfEmpty(row.LyDoHuy);
        order.NhanVienChamSocKhachHang = NullIfEmpty(row.NhanVienChamSocKhachHang);
        order.NgayXacNhan = row.NgayXacNhan;
        order.NgayGuiHangVanChuyen = row.NgayGuiHangVanChuyen;
        order.NgayBanGiao = row.NgayBanGiao;
        order.NgayThanhCong = row.NgayThanhCong;
        order.GioThanhCong = NullIfEmpty(row.GioThanhCong);
        order.NguonDonHang = NullIfEmpty(row.NguonDonHang);
        order.GhiChuHangVanChuyen = NullIfEmpty(row.GhiChuHangVanChuyen);
        order.CanNangHangVanChuyen = row.CanNangHangVanChuyen;
        order.MaGiamGia = NullIfEmpty(row.MaGiamGia);
        order.IdDonTra = row.IdDonTra;
        order.IdDonGoc = row.IdDonGoc;
        order.SoThuTuTheoNgay = row.SoThuTuTheoNgay;
        order.BienBanBanGiao = NullIfEmpty(row.BienBanBanGiao);
        order.MaAffiliate = NullIfEmpty(row.MaAffiliate);
        order.NhomKhachHang = NullIfEmpty(row.NhomKhachHang);
        order.TienVat = row.TienVat;
        order.TyLeVat = NullIfEmpty(row.TyLeVat);
        order.TienTieuDiem = row.TienTieuDiem;
        order.Nhan = NullIfEmpty(row.Nhan);
        order.NgayHenThanhToan = row.NgayHenThanhToan;
        order.TrangThaiDoiSoat = NullIfEmpty(row.TrangThaiDoiSoat);
        order.NgayDoiSoat = row.NgayDoiSoat;
        order.TrangThaiThanhToanDoanhNghiep = NullIfEmpty(row.TrangThaiThanhToanDoanhNghiep);
        order.NgayThanhToanDoanhNghiep = row.NgayThanhToanDoanhNghiep;
        order.NhanVienBanHang = NullIfEmpty(row.NhanVienBanHang);
        order.LoiNhuan = row.LoiNhuan;
        order.TyLeLoiNhuan = NullIfEmpty(row.TyLeLoiNhuan);
        order.FacebookPsid = NullIfEmpty(row.FacebookPsid);
        order.FacebookPageUserId = NullIfEmpty(row.FacebookPageUserId);
        order.SoDienThoaiFacebook = NullIfEmpty(row.SoDienThoaiFacebook);
        order.GiaTriFacebook = NullIfEmpty(row.GiaTriFacebook);
        order.SoLuongDonViNhoNhat = row.SoLuongDonViNhoNhat;
        order.NhanVienDongGoi = NullIfEmpty(row.NhanVienDongGoi);
        order.IdPhapNhan = row.IdPhapNhan;
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
        order.NgayPhatHanhHoaDon = row.NgayPhatHanhHoaDon;
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

    // ── Helpers ─────────────────────────────────────────────────────────

    /** Trả về null nếu chuỗi rỗng / chỉ chứa khoảng trắng / là chữ "null". */
    private static string? NullIfEmpty(string? val)
    {
        if (string.IsNullOrWhiteSpace(val)) return null;
        var trimmed = val.Trim();
        return trimmed.Equals("null", StringComparison.OrdinalIgnoreCase) ? null : trimmed;
    }
}