using System.Globalization;
using HoaTheoMua.Repository;
using HoaTheoMua.Service.Exceptions;
using Microsoft.EntityFrameworkCore;
using MiniExcelLibs;

namespace HoaTheoMua.Service.OrderNhanhVnImport;

using OrderEntity = HoaTheoMua.Repository.Entity.OrderNhanhVn;
using DetailEntity = HoaTheoMua.Repository.Entity.OrderNhanhVnDetail;

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

            var groups = GroupRowsByOrder(rows);

            var isb = await BuildUpsertBatchAsync(groups);
            await PersistBatchAsync(isb);

            result.InsertedCount = isb.OrdersToInsert.Count;
            result.UpdatedCount = isb.OrdersToUpdate.Count;
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

        return rows;
    }
    
    private class OrderGroup
    {
        public required string IdDonHangNhanh { get; init; }
        public required Request.ExcelRow HeaderRow { get; init; }
        public List<Request.ExcelRow> ItemRows { get; } = new();
    }

    private static List<OrderGroup> GroupRowsByOrder(List<Request.ExcelRow> rows)
    {
        var groups = new List<OrderGroup>();
        OrderGroup? current = null;

        foreach (var row in rows)
        {
            var id = NullIfEmpty(row.IdDonHangNhanh);

            if (id != null)
            {
                // Dòng có ID -> bắt đầu 1 đơn hàng mới
                current = new OrderGroup { IdDonHangNhanh = id, HeaderRow = row };
                current.ItemRows.Add(row);
                groups.Add(current);
            }
            else if (current != null)
            {
                // Dòng sản phẩm phụ, kế thừa đơn hàng gần nhất phía trên
                current.ItemRows.Add(row);
            }
            // else: dòng đầu file mà không có ID -> dữ liệu bất thường, bỏ qua
        }

        return groups;
    }

    // ── Upsert batch ───────────────────────────────────────────────────

    private struct ImportBatch
    {
        public List<OrderEntity> OrdersToInsert;
        public List<OrderEntity> OrdersToUpdate;
        public List<DetailEntity> DetailsToDelete;
        public List<DetailEntity> DetailsToInsert;
    }

    private async Task<ImportBatch> BuildUpsertBatchAsync(List<OrderGroup> groups)
    {
        //lấy id của group trên excel hiện có
        var idsInFile = groups.Select(g => g.IdDonHangNhanh).Distinct().ToList();

        //tìm những order có id đó -> tạo thành dictionary
        var existingOrders = idsInFile.Count > 0
            ? await _dbContext.OrderNhanhVns
                .Where(o => o.IdDonHangNhanh != null && idsInFile.Contains(o.IdDonHangNhanh) && !o.IsDeleted)
                .ToDictionaryAsync(o => o.IdDonHangNhanh!)
            : new Dictionary<string, OrderEntity>();
    
        //lấy value của dictionary đó: id
        var existingOrderIds = existingOrders.Values.Select(o => o.Id).ToList();
        
        //dựa vào id đó đi lấy orderDetails
        var existingDetails = existingOrderIds.Count > 0
            ? await _dbContext.OrderNhanhVnDetails
                .Where(d => existingOrderIds.Contains(d.OrderNhanhVnId.Value))
                .ToListAsync()
            : new List<DetailEntity>();
            
        //map thành Id và OrderDetails
        var detailsByOrderId = existingDetails
            .GroupBy(d => d.OrderNhanhVnId)
            .ToDictionary(g => g.Key, g => g.ToList());

        var batch = new ImportBatch
        {
            OrdersToInsert = new List<OrderEntity>(),
            OrdersToUpdate = new List<OrderEntity>(),
            DetailsToDelete = new List<DetailEntity>(),
            DetailsToInsert = new List<DetailEntity>()
        };

        foreach (var group in groups)
        {
            var order = ResolveOrder(group.IdDonHangNhanh, existingOrders, out var isUpdate);
            ApplyHeaderToOrder(group.HeaderRow, order);
            order.UpdatedAt = DateTimeOffset.UtcNow;

            if (isUpdate)
            {
                batch.OrdersToUpdate.Add(order);
                
                if (detailsByOrderId.TryGetValue(order.Id, out var oldDetails))
                    batch.DetailsToDelete.AddRange(oldDetails);
            }
            else
            {
                batch.OrdersToInsert.Add(order);
            }

            foreach (var itemRow in group.ItemRows)
            {
                var detail = new DetailEntity
                {
                    Id = Guid.NewGuid(),
                    OrderNhanhVnId = order.Id,
                    CreatedAt = DateTimeOffset.UtcNow
                };
                ApplyItemToDetail(itemRow, detail);
                batch.DetailsToInsert.Add(detail);
            }
        }

        return batch;
    }

    private static OrderEntity ResolveOrder(
        string idDonHangNhanh,
        Dictionary<string, OrderEntity> existingMap,
        out bool isUpdate)
    {
        if (existingMap.TryGetValue(idDonHangNhanh, out var existing))
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

    /** Áp dụng thông tin cấp-đơn-hàng từ dòng Excel đầu tiên của đơn vào OrderNhanhVn entity. */
    private static void ApplyHeaderToOrder(Request.ExcelRow row, OrderEntity order)
    {
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
        // order.GiaVonDonHang = ParseDecimal(row.GiaVonDonHang);
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
        // order.NhanVienDongGoi = NullIfEmpty(row.NhanVienDongGoi);
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

    /** Áp dụng thông tin cấp-sản-phẩm từ 1 dòng Excel vào OrderNhanhVnDetail entity. */
    private static void ApplyItemToDetail(Request.ExcelRow row, DetailEntity detail)
    {
        detail.GiaVonDonHang = ParseDecimal(row.GiaVonDonHang);
        detail.NhanVienDongGoi = NullIfEmpty(row.NhanVienDongGoi);
        detail.TenSanPham = NullIfEmpty(row.TenSanPham);
        detail.QuaTangKem = NullIfEmpty(row.QuaTangKem);
        detail.Imei = NullIfEmpty(row.Imei);
        detail.LoHang = NullIfEmpty(row.LoHang);
        detail.MaSanPhamCha = NullIfEmpty(row.MaSanPhamCha);
        detail.TenSanPhamCha = NullIfEmpty(row.TenSanPhamCha);
        detail.MaSanPham = NullIfEmpty(row.MaSanPham);
        detail.MaVach = NullIfEmpty(row.MaVach);
        detail.DanhMucSanPham = NullIfEmpty(row.DanhMucSanPham);
        detail.KhoiLuongSanPham = ParseDecimal(row.KhoiLuongSanPham);
        detail.GiaBan = ParseDecimal(row.GiaBan);
        detail.LoaiGiaSanPham = NullIfEmpty(row.LoaiGiaSanPham);
        detail.GiaBanTruocVat = ParseDecimal(row.GiaBanTruocVat);
        detail.TienVatSanPham = ParseDecimal(row.TienVatSanPham);
        detail.SoLuong = ParseInt(row.SoLuong);
        detail.ChietKhauMotSanPham = ParseDecimal(row.ChietKhauMotSanPham);
        detail.ChietKhauSanPham = ParseDecimal(row.ChietKhauSanPham);
        detail.GiaSanPhamSauChietKhau = ParseDecimal(row.GiaSanPhamSauChietKhau);
        detail.GhiChuSanPham = NullIfEmpty(row.GhiChuSanPham);
        detail.DonViTinh = NullIfEmpty(row.DonViTinh);
        detail.GiaVon = ParseDecimal(row.GiaVon);
        detail.SoLuongDonViNhoNhat = ParseInt(row.SoLuongDonViNhoNhat);
    }

    /** Lưu batch vào DB trong 1 transaction (rollback nếu lỗi). */
    private async Task PersistBatchAsync(ImportBatch batch)
    {
        await using var tx = await _dbContext.Database.BeginTransactionAsync();
        try
        {
            if (batch.OrdersToInsert.Count > 0)
                await _dbContext.OrderNhanhVns.AddRangeAsync(batch.OrdersToInsert);

            if (batch.OrdersToUpdate.Count > 0)
                _dbContext.OrderNhanhVns.UpdateRange(batch.OrdersToUpdate);

            // Phải save trước để các Order mới có Id thật sự tồn tại trong DB
            // trước khi insert Details tham chiếu tới OrderNhanhVnId của chúng.
            await _dbContext.SaveChangesAsync();

            if (batch.DetailsToDelete.Count > 0)
                _dbContext.OrderNhanhVnDetails.RemoveRange(batch.DetailsToDelete);

            if (batch.DetailsToInsert.Count > 0)
                await _dbContext.OrderNhanhVnDetails.AddRangeAsync(batch.DetailsToInsert);

            await _dbContext.SaveChangesAsync();
            await tx.CommitAsync();
        }
        catch
        {
            await tx.RollbackAsync();
            throw;
        }
    }

    // ── Parse helpers ──────────────────────────────────────────────────

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