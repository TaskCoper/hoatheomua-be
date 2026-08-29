using System.Text.Json.Serialization;

namespace HoaTheoMua.Service.NhanhVnService;

public class OrderLabelResponse
{
    [JsonPropertyName("code")]
    public int Code { get; set; }

    [JsonPropertyName("paginator")]
    public ResponsePaginator Paginator { get; set; } = null!;

    [JsonPropertyName("data")]
    public List<LabelData> Data { get; set; } = null!;
}

public class ResponsePaginator
{
    [JsonPropertyName("next")]
    public NextPage Next { get; set; } = null!;
}

public class NextPage
{
    // SỬA TỪ string THÀNH int VÌ THỰC TẾ TRẢ VỀ SỐ (Ví dụ: 262)
    [JsonPropertyName("id")]
    public int Id { get; set; } 
}

public class LabelData
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("businessId")]
    public int BusinessId { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; } = null!;

    [JsonPropertyName("status")]
    public int Status { get; set; }

    // SỬA THÀNH string? VÌ DỮ LIỆU THỰC TẾ NHIỀU ITEM KHÔNG CÓ bgColor
    [JsonPropertyName("bgColor")]
    public string? BgColor { get; set; }

    // SỬA THÀNH string? ĐỂ PHÒNG TRƯỜNG HỢP CÓ ITEM THIẾU CẢ textColor
    [JsonPropertyName("textColor")]
    public string? TextColor { get; set; }

    [JsonPropertyName("createdAt")]
    public long CreatedAt { get; set; }

    // Tiện ích: Tự động decode unicode (như \u1ea3) C# xử lý tự động, không lo lắng
    [JsonIgnore]
    public DateTime CreatedAtDateTime => DateTimeOffset.FromUnixTimeSeconds(CreatedAt).LocalDateTime;
}

public class OrderSourceResponse
{
    [JsonPropertyName("code")]
    public int Code { get; set; }

    [JsonPropertyName("paginator")]
    public SourcePaginator Paginator { get; set; } = null!;
 
    [JsonPropertyName("data")]
    public List<OrderSourceData> Data { get; set; } = null!;
}

public class SourcePaginator
{
    // Dùng dấu ? đề phòng trường hợp trang cuối cùng không có phần tử 'next' (next bị null)
    [JsonPropertyName("next")]
    public SourceNextPage? Next { get; set; }
}

public class SourceNextPage
{
    [JsonPropertyName("id")]
    public int Id { get; set; }
}

public class OrderSourceData
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; } = null!;

    // Trạng thái: 1 = Active, 2 = Inactive
    [JsonPropertyName("status")]
    public int Status { get; set; }

    // Thời gian tạo - định dạng định dạng unix timestamp
    [JsonPropertyName("createdAt")]
    public long CreatedAt { get; set; }

    // Tiện ích: Tự động quy đổi timestamp thành định dạng DateTime trong C# để tiện xử lý logic
    [JsonIgnore]
    public DateTime CreatedAtDateTime => DateTimeOffset.FromUnixTimeSeconds(CreatedAt).LocalDateTime;
}

public class OrderNhanhVnDto
{
    public Guid Id { get; set; }
    public string? IdDonHangNhanh { get; set; }
    public string? IdDonHangApi { get; set; }
    public string? LoaiDon { get; set; }
    public DateTimeOffset? ThoiGianDonHang { get; set; }
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
    public DateTimeOffset? NgaySinhKhachHang { get; set; }
    public string? ThanhPhoKhachHang { get; set; }
    public string? QuanHuyenKhachHang { get; set; }
    public string? PhuongXaKhachHang { get; set; }
    public string? DiaChiKhachHang { get; set; }
    public decimal? TongKhoiLuong { get; set; }
    public decimal? GiaTriDonHang { get; set; }
    public decimal? PhiVanChuyen { get; set; }
    public decimal? PhiVuotCan { get; set; }
    public decimal? PhiKhaiGia { get; set; }
    public decimal? PhiChuyenHoan { get; set; }
    public decimal? PhiThuTienHo { get; set; }
    public decimal? PhiShipBaoKhach { get; set; }
    public string? CongVanChuyen { get; set; }
    public string? HangVanChuyen { get; set; }
    public string? DichVuVanChuyen { get; set; }
    public int? SoNgayChuyenHang { get; set; }
    public string? MaVanDon { get; set; }
    public string? BienBan { get; set; }
    public decimal? TienChuyenKhoan { get; set; }
    public string? TaiKhoanChuyenKhoan { get; set; }
    public decimal? TienChietKhau { get; set; }
    public decimal? TienDatCoc { get; set; }
    public string? TaiKhoanDatCoc { get; set; }
    public decimal? TongThu { get; set; }
    public string? TaiKhoanQuetThe { get; set; }
    public decimal? TienQuetThe { get; set; }
    public string? MaGiaoDichQuetThe { get; set; }
    public string? GhiChuNoiBo { get; set; }
    public string? GhiChuCuaKhach { get; set; }
    public string? TrangThaiHoaDonDienTu { get; set; }
    public string? SoHoaDonDienTu { get; set; }
    public DateTimeOffset? NgayXuatHoaDonDienTu { get; set; }
    public string? MaCoQuanThueHoaDonDienTu { get; set; }
    public string? TrangThaiDonHang { get; set; }
    public string? LyDoHuy { get; set; }
    public string? NhanVienChamSocKhachHang { get; set; }
    public DateTimeOffset? NgayXacNhan { get; set; }
    public DateTimeOffset? NgayGuiHangVanChuyen { get; set; }
    public DateTimeOffset? NgayBanGiao { get; set; }
    public DateTimeOffset? NgayThanhCong { get; set; }
    public string? GioThanhCong { get; set; }
    public string? NguonDonHang { get; set; }
    public string? GhiChuHangVanChuyen { get; set; }
    public decimal? CanNangHangVanChuyen { get; set; }
    public string? MaGiamGia { get; set; }
    public string? IdDonTra { get; set; }
    public string? IdDonGoc { get; set; }
    public int? SoThuTuTheoNgay { get; set; }
    public string? BienBanBanGiao { get; set; }
    public string? MaAffiliate { get; set; }
    public string? NhomKhachHang { get; set; }
    public decimal? TienVat { get; set; }
    public string? TyLeVat { get; set; }
    public decimal? TienTieuDiem { get; set; }
    public string? Nhan { get; set; }
    public DateTimeOffset? NgayHenThanhToan { get; set; }
    public string? TrangThaiDoiSoat { get; set; }
    public DateTimeOffset? NgayDoiSoat { get; set; }
    public string? TrangThaiThanhToanDoanhNghiep { get; set; }
    public DateTimeOffset? NgayThanhToanDoanhNghiep { get; set; }
    public string? NhanVienBanHang { get; set; }
    public decimal? LoiNhuan { get; set; }
    public string? TyLeLoiNhuan { get; set; }
    public string? FacebookPsid { get; set; }
    public string? FacebookPageUserId { get; set; }
    public string? SoDienThoaiFacebook { get; set; }
    public string? GiaTriFacebook { get; set; }
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
    public DateTimeOffset? NgayPhatHanhHoaDon { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }

    public List<OrderNhanhVnDetailDto> OrderNhanhVnDetails { get; set; } = new();
}

public class OrderNhanhVnDetailDto
{
    public Guid Id { get; set; }
    public Guid? OrderNhanhVnId { get; set; }
    public string? TenSanPham { get; set; }
    public string? QuaTangKem { get; set; }
    public string? Imei { get; set; }
    public string? LoHang { get; set; }
    public string? MaSanPhamCha { get; set; }
    public string? TenSanPhamCha { get; set; }
    public string? MaSanPham { get; set; }
    public string? MaVach { get; set; }
    public string? DanhMucSanPham { get; set; }
    public decimal? KhoiLuongSanPham { get; set; }
    public decimal? GiaBan { get; set; }
    public string? LoaiGiaSanPham { get; set; }
    public decimal? GiaBanTruocVat { get; set; }
    public decimal? TienVatSanPham { get; set; }
    public int? SoLuong { get; set; }
    public decimal? ChietKhauMotSanPham { get; set; }
    public decimal? ChietKhauSanPham { get; set; }
    public decimal? GiaSanPhamSauChietKhau { get; set; }
    public string? GhiChuSanPham { get; set; }
    public decimal? GiaVon { get; set; }
    public decimal? GiaVonDonHang { get; set; }
    public string? DonViTinh { get; set; }
    public int? SoLuongDonViNhoNhat { get; set; }
    public string? NhanVienDongGoi { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
}