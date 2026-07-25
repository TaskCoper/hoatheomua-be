using HoaTheoMua.Repository.Abtraction;

namespace HoaTheoMua.Repository.Entity;

public class OrderNhanhVn : BaseEntity<Guid>, IAuditableEntity
{
    public int SourceRowNumber { get; set; }
    //Excel: ID
    public long? IdDonHangNhanh { get; set; }

    //Excel: ID Đơn hàng API
    public long? IdDonHangApi { get; set; }

    //Excel: Loại đơn
    public string? LoaiDon { get; set; }

    //Excel: Thời gian
    public DateTimeOffset? ThoiGianDonHang { get; set; }

    //Excel: Người tạo đơn
    public string? NguoiTaoDon { get; set; }

    //Excel: Cửa hàng
    public string? CuaHang { get; set; }

    //Excel: Nguồn
    public string? Nguon { get; set; }

    //Excel: Kho tạo đơn
    public string? KhoTaoDon { get; set; }

    //Excel: Thành phố (kho hàng)
    public string? ThanhPhoKhoHang { get; set; }

    //Excel: Quận huyện (kho hàng)
    public string? QuanHuyenKhoHang { get; set; }

    //Excel: Kho lấy hàng
    public string? KhoLayHang { get; set; }

    //Excel: Thành phố lấy hàng
    public string? ThanhPhoLayHang { get; set; }

    //Excel: Quận huyện lấy hàng
    public string? QuanHuyenLayHang { get; set; }

    //Excel: Tên khách hàng
    public string? TenKhachHang { get; set; }

    //Excel: Mã khách hàng
    public string? MaKhachHang { get; set; }

    //Excel: Số điện thoại
    public string? SoDienThoaiKhachHang { get; set; }

    //Excel: Email
    public string? EmailKhachHang { get; set; }

    //Excel: Cấp độ khách hàng
    public string? CapDoKhachHang { get; set; }

    //Excel: Sinh nhật
    public DateTimeOffset? NgaySinhKhachHang { get; set; }

    //Excel: Thành phố
    public string? ThanhPhoKhachHang { get; set; }

    //Excel: Quận huyện
    public string? QuanHuyenKhachHang { get; set; }

    //Excel: Phường/xã
    public string? PhuongXaKhachHang { get; set; }

    //Excel: Địa chỉ
    public string? DiaChiKhachHang { get; set; }

    //Excel: Sản phẩm
    public string? TenSanPham { get; set; }

    //Excel: Quà tặng kèm
    public string? QuaTangKem { get; set; }

    //Excel: IMEI
    public string? Imei { get; set; }

    //Excel: Lô hàng
    public string? LoHang { get; set; }

    //Excel: Mã sản phẩm cha
    public string? MaSanPhamCha { get; set; }

    //Excel: Tên sản phẩm cha
    public string? TenSanPhamCha { get; set; }

    //Excel: Mã sản phẩm
    public string? MaSanPham { get; set; }

    //Excel: Mã vạch
    public string? MaVach { get; set; }

    //Excel: Danh mục sản phẩm
    public string? DanhMucSanPham { get; set; }

    //Excel: Khối lượng
    public decimal? KhoiLuongSanPham { get; set; }

    //Excel: Giá bán
    public decimal? GiaBan { get; set; }

    //Excel: Loại
    public string? LoaiGiaSanPham { get; set; }

    //Excel: Giá bán trước VAT
    public decimal? GiaBanTruocVat { get; set; }

    //Excel: VAT sản phẩm
    public decimal? TienVatSanPham { get; set; }

    //Excel: Số lượng
    public int? SoLuong { get; set; }

    //Excel: Chiết khấu 1 SP
    public decimal? ChietKhauMotSanPham { get; set; }

    //Excel: Chiết khấu sản phẩm
    public decimal? ChietKhauSanPham { get; set; }

    //Excel: Giá sản phẩm sau chiết khấu
    public decimal? GiaSanPhamSauChietKhau { get; set; }

    //Excel: Ghi chú sản phẩm
    public string? GhiChuSanPham { get; set; }

    //Excel: Tổng khối lượng
    public decimal? TongKhoiLuong { get; set; }

    //Excel: Giá trị đơn hàng
    public decimal? GiaTriDonHang { get; set; }

    //Excel: Phí vận chuyển
    public decimal? PhiVanChuyen { get; set; }

    //Excel: Phí vượt cân
    public decimal? PhiVuotCan { get; set; }

    //Excel: Phí khai giá
    public decimal? PhiKhaiGia { get; set; }

    //Excel: Phí chuyển hoàn
    public decimal? PhiChuyenHoan { get; set; }

    //Excel: Phí thu tiền hộ
    public decimal? PhiThuTienHo { get; set; }

    //Excel: Phí ship báo khách
    public decimal? PhiShipBaoKhach { get; set; }

    //Excel: Cổng vận chuyển
    public string? CongVanChuyen { get; set; }

    //Excel: Hãng vận chuyển
    public string? HangVanChuyen { get; set; }

    //Excel: Dịch vụ vận chuyển
    public string? DichVuVanChuyen { get; set; }

    //Excel: Số ngày chuyển hàng
    public int? SoNgayChuyenHang { get; set; }

    //Excel: Mã vận đơn
    public string? MaVanDon { get; set; }

    //Excel: Biên bản
    public string? BienBan { get; set; }

    //Excel: Tiền chuyển khoản
    public decimal? TienChuyenKhoan { get; set; }

    //Excel: Tài khoản chuyển khoản
    public string? TaiKhoanChuyenKhoan { get; set; }

    //Excel: Tiền chiết khấu
    public decimal? TienChietKhau { get; set; }

    //Excel: Tiền đặt cọc
    public decimal? TienDatCoc { get; set; }

    //Excel: Tài khoản đặt cọc
    public string? TaiKhoanDatCoc { get; set; }

    //Excel: Tổng thu
    public decimal? TongThu { get; set; }

    //Excel: Tài khoản quẹt thẻ
    public string? TaiKhoanQuetThe { get; set; }

    //Excel: Tiền quẹt thẻ
    public decimal? TienQuetThe { get; set; }

    //Excel: Mã giao dịch quẹt thẻ
    public string? MaGiaoDichQuetThe { get; set; }

    //Excel: Giá vốn
    public decimal? GiaVon { get; set; }

    //Excel: Giá vốn đơn hàng
    public decimal? GiaVonDonHang { get; set; }

    //Excel: Đơn vị tính
    public string? DonViTinh { get; set; }

    //Excel: Ghi chú nội bộ
    public string? GhiChuNoiBo { get; set; }

    //Excel: Ghi chú của khách
    public string? GhiChuCuaKhach { get; set; }

    //Excel: Trạng thái hóa đơn điện tử
    public string? TrangThaiHoaDonDienTu { get; set; }

    //Excel: Số hóa đơn điện tử
    public string? SoHoaDonDienTu { get; set; }

    //Excel: Ngày xuất hóa đơn điện tử
    public DateTimeOffset? NgayXuatHoaDonDienTu { get; set; }

    //Excel: Mã cơ quan thuế
    public string? MaCoQuanThueHoaDonDienTu { get; set; }

    //Excel: Trạng thái
    public string? TrangThaiDonHang { get; set; }

    //Excel: Lý do hủy
    public string? LyDoHuy { get; set; }

    //Excel: Chăm sóc khách hàng
    public string? NhanVienChamSocKhachHang { get; set; }

    //Excel: Ngày xác nhận
    public DateTimeOffset? NgayXacNhan { get; set; }

    //Excel: Ngày gửi HVC
    public DateTimeOffset? NgayGuiHangVanChuyen { get; set; }

    //Excel: Ngày bàn giao
    public DateTimeOffset? NgayBanGiao { get; set; }

    //Excel: Ngày thành công
    public DateTimeOffset? NgayThanhCong { get; set; }

    //Excel: Giờ thành công
    public string? GioThanhCong { get; set; }

    //Excel: Nguồn đơn hàng
    public string? NguonDonHang { get; set; }

    //Excel: Ghi chú HVC
    public string? GhiChuHangVanChuyen { get; set; }

    //Excel: Cân nặng HVC
    public decimal? CanNangHangVanChuyen { get; set; }

    //Excel: Mã giảm giá
    public string? MaGiamGia { get; set; }

    //Excel: ID đơn trả
    public long? IdDonTra { get; set; }

    //Excel: ID đơn gốc
    public long? IdDonGoc { get; set; }

    //Excel: STT theo ngày
    public int? SoThuTuTheoNgay { get; set; }

    //Excel: Biên bản bàn giao
    public string? BienBanBanGiao { get; set; }

    //Excel: Mã affiliate
    public string? MaAffiliate { get; set; }

    //Excel: Nhóm khách hàng
    public string? NhomKhachHang { get; set; }

    //Excel: VAT
    public decimal? TienVat { get; set; }

    //Excel: % VAT
    public string? TyLeVat { get; set; }

    //Excel: Tiền tiêu điểm
    public decimal? TienTieuDiem { get; set; }

    //Excel: Nhãn
    public string? Nhan { get; set; }

    //Excel: Ngày hẹn thanh toán
    public DateTimeOffset? NgayHenThanhToan { get; set; }

    //Excel: Trạng thái đối soát
    public string? TrangThaiDoiSoat { get; set; }

    //Excel: Ngày đối soát
    public DateTimeOffset? NgayDoiSoat { get; set; }

    //Excel: Thanh toán cho doanh nghiệp
    public string? TrangThaiThanhToanDoanhNghiep { get; set; }

    //Excel: Ngày thanh toán cho doanh nghiệp
    public DateTimeOffset? NgayThanhToanDoanhNghiep { get; set; }

    //Excel: Nhân viên bán hàng
    public string? NhanVienBanHang { get; set; }

    //Excel: Lợi nhuận
    public decimal? LoiNhuan { get; set; }

    //Excel: Phần trăm lợi nhuận
    public string? TyLeLoiNhuan { get; set; }

    //Excel: Facebook PSID
    public string? FacebookPsid { get; set; }

    //Excel: PageUserId (Facebook PSID)
    public string? FacebookPageUserId { get; set; }

    //Excel: Phone (Facebook PSID)
    public string? SoDienThoaiFacebook { get; set; }

    //Excel: Value (Facebook PSID)
    public string? GiaTriFacebook { get; set; }

    //Excel: Số lượng đơn vị nhỏ nhất
    public int? SoLuongDonViNhoNhat { get; set; }

    //Excel: Nhân viên đóng gói
    public string? NhanVienDongGoi { get; set; }

    //Excel: ID pháp nhân
    public long? IdPhapNhan { get; set; }

    //Excel: Mã số thuế
    public string? MaSoThue { get; set; }

    //Excel: Email người liên hệ
    public string? EmailNguoiLienHe { get; set; }

    //Excel: Email người liên hệ 2
    public string? EmailNguoiLienHeThuHai { get; set; }

    //Excel: Số điện thoại người liên hệ
    public string? SoDienThoaiNguoiLienHe { get; set; }

    //Excel: Tên người liên hệ
    public string? TenNguoiLienHe { get; set; }

    //Excel: Mã tham chiếu
    public string? MaThamChieu { get; set; }

    //Excel: Địa chỉ khách hàng hiển thị ở trên hóa đơn điện tử
    public string? DiaChiKhachHangTrenHoaDonDienTu { get; set; }

    //Excel: Tên khách hàng hiển thị ở trên hóa đơn điện tử
    public string? TenKhachHangTrenHoaDonDienTu { get; set; }

    //Excel: Loại
    public string? LoaiKhachHangHoaDon { get; set; }

    //Excel: Có lấy hóa đơn
    public string? CoLayHoaDon { get; set; }

    //Excel: Mã cơ quan thuế
    public string? MaCoQuanThueHoaDon { get; set; }

    //Excel: Số hóa đơn
    public string? SoHoaDon { get; set; }

    //Excel: Ngày phát hành
    public DateTimeOffset? NgayPhatHanhHoaDon { get; set; }
    
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
}