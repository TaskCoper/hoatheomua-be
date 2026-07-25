using MiniExcelLibs.Attributes;

namespace HoaTheoMua.Service.OrderNhanhVnImport;

public class Request
{
    public class ExcelRow
    {
        public int SourceRowNumber { get; set; }

        [ExcelColumnIndex(0)] // A - ID
        public string? IdDonHangNhanh { get; set; }

        [ExcelColumnIndex(1)] // B - ID Đơn hàng API
        public string? IdDonHangApi { get; set; }

        [ExcelColumnIndex(2)] // C - Loại đơn
        public string? LoaiDon { get; set; }

        [ExcelColumnIndex(3)] // D - Thời gian
        public string? ThoiGianDonHang { get; set; }

        [ExcelColumnIndex(4)] // E - Người tạo đơn
        public string? NguoiTaoDon { get; set; }

        [ExcelColumnIndex(5)] // F - Cửa hàng
        public string? CuaHang { get; set; }

        [ExcelColumnIndex(6)] // G - Nguồn
        public string? Nguon { get; set; }

        [ExcelColumnIndex(7)] // H - Kho tạo đơn
        public string? KhoTaoDon { get; set; }

        [ExcelColumnIndex(8)] // I - Thành phố (kho hàng)
        public string? ThanhPhoKhoHang { get; set; }

        [ExcelColumnIndex(9)] // J - Quận huyện (kho hàng)
        public string? QuanHuyenKhoHang { get; set; }

        [ExcelColumnIndex(10)] // K - Kho lấy hàng
        public string? KhoLayHang { get; set; }

        [ExcelColumnIndex(11)] // L - Thành phố lấy hàng
        public string? ThanhPhoLayHang { get; set; }

        [ExcelColumnIndex(12)] // M - Quận huyện lấy hàng
        public string? QuanHuyenLayHang { get; set; }

        [ExcelColumnIndex(13)] // N - Tên khách hàng
        public string? TenKhachHang { get; set; }

        [ExcelColumnIndex(14)] // O - Mã khách hàng
        public string? MaKhachHang { get; set; }

        [ExcelColumnIndex(15)] // P - Số điện thoại
        public string? SoDienThoaiKhachHang { get; set; }

        [ExcelColumnIndex(16)] // Q - Email
        public string? EmailKhachHang { get; set; }

        [ExcelColumnIndex(17)] // R - Cấp độ khách hàng
        public string? CapDoKhachHang { get; set; }

        [ExcelColumnIndex(18)] // S - Sinh nhật
        public string? NgaySinhKhachHang { get; set; }

        [ExcelColumnIndex(19)] // T - Thành phố
        public string? ThanhPhoKhachHang { get; set; }

        [ExcelColumnIndex(20)] // U - Quận huyện
        public string? QuanHuyenKhachHang { get; set; }

        [ExcelColumnIndex(21)] // V - Phường/xã
        public string? PhuongXaKhachHang { get; set; }

        [ExcelColumnIndex(22)] // W - Địa chỉ
        public string? DiaChiKhachHang { get; set; }

        [ExcelColumnIndex(23)] // X - Sản phẩm
        public string? TenSanPham { get; set; }

        [ExcelColumnIndex(24)] // Y - Quà tặng kèm
        public string? QuaTangKem { get; set; }

        [ExcelColumnIndex(25)] // Z - IMEI
        public string? Imei { get; set; }

        [ExcelColumnIndex(26)] // AA - Lô hàng
        public string? LoHang { get; set; }

        [ExcelColumnIndex(27)] // AB - Mã sản phẩm cha
        public string? MaSanPhamCha { get; set; }

        [ExcelColumnIndex(28)] // AC - Tên sản phẩm cha
        public string? TenSanPhamCha { get; set; }

        [ExcelColumnIndex(29)] // AD - Mã sản phẩm
        public string? MaSanPham { get; set; }

        [ExcelColumnIndex(30)] // AE - Mã vạch
        public string? MaVach { get; set; }

        [ExcelColumnIndex(31)] // AF - Danh mục sản phẩm
        public string? DanhMucSanPham { get; set; }

        [ExcelColumnIndex(32)] // AG - Khối lượng
        public string? KhoiLuongSanPham { get; set; }

        [ExcelColumnIndex(33)] // AH - Giá bán
        public string? GiaBan { get; set; }

        [ExcelColumnIndex(34)] // AI - Loại
        public string? LoaiGiaSanPham { get; set; }

        [ExcelColumnIndex(35)] // AJ - Giá bán trước VAT
        public string? GiaBanTruocVat { get; set; }

        [ExcelColumnIndex(36)] // AK - VAT sản phẩm
        public string? TienVatSanPham { get; set; }

        [ExcelColumnIndex(37)] // AL - Số lượng
        public string? SoLuong { get; set; }

        [ExcelColumnIndex(38)] // AM - Chiết khấu 1 SP
        public string? ChietKhauMotSanPham { get; set; }

        [ExcelColumnIndex(39)] // AN - Chiết khấu sản phẩm
        public string? ChietKhauSanPham { get; set; }

        [ExcelColumnIndex(40)] // AO - Giá sản phẩm sau chiết khấu
        public string? GiaSanPhamSauChietKhau { get; set; }

        [ExcelColumnIndex(41)] // AP - Ghi chú sản phẩm
        public string? GhiChuSanPham { get; set; }

        [ExcelColumnIndex(42)] // AQ - Tổng khối lượng
        public string? TongKhoiLuong { get; set; }

        [ExcelColumnIndex(43)] // AR - Giá trị đơn hàng
        public string? GiaTriDonHang { get; set; }

        [ExcelColumnIndex(44)] // AS - Phí vận chuyển
        public string? PhiVanChuyen { get; set; }

        [ExcelColumnIndex(45)] // AT - Phí vượt cân
        public string? PhiVuotCan { get; set; }

        [ExcelColumnIndex(46)] // AU - Phí khai giá
        public string? PhiKhaiGia { get; set; }

        [ExcelColumnIndex(47)] // AV - Phí chuyển hoàn
        public string? PhiChuyenHoan { get; set; }

        [ExcelColumnIndex(48)] // AW - Phí thu tiền hộ
        public string? PhiThuTienHo { get; set; }

        [ExcelColumnIndex(49)] // AX - Phí ship báo khách
        public string? PhiShipBaoKhach { get; set; }

        [ExcelColumnIndex(50)] // AY - Cổng vận chuyển
        public string? CongVanChuyen { get; set; }

        [ExcelColumnIndex(51)] // AZ - Hãng vận chuyển
        public string? HangVanChuyen { get; set; }

        [ExcelColumnIndex(52)] // BA - Dịch vụ vận chuyển
        public string? DichVuVanChuyen { get; set; }

        [ExcelColumnIndex(53)] // BB - Số ngày chuyển hàng
        public string? SoNgayChuyenHang { get; set; }

        [ExcelColumnIndex(54)] // BC - Mã vận đơn
        public string? MaVanDon { get; set; }

        [ExcelColumnIndex(55)] // BD - Biên bản
        public string? BienBan { get; set; }

        [ExcelColumnIndex(56)] // BE - Tiền chuyển khoản
        public string? TienChuyenKhoan { get; set; }

        [ExcelColumnIndex(57)] // BF - Tài khoản chuyển khoản
        public string? TaiKhoanChuyenKhoan { get; set; }

        [ExcelColumnIndex(58)] // BG - Tiền chiết khấu
        public string? TienChietKhau { get; set; }

        [ExcelColumnIndex(59)] // BH - Tiền đặt cọc
        public string? TienDatCoc { get; set; }

        [ExcelColumnIndex(60)] // BI - Tài khoản đặt cọc
        public string? TaiKhoanDatCoc { get; set; }

        [ExcelColumnIndex(61)] // BJ - Tổng thu
        public string? TongThu { get; set; }

        [ExcelColumnIndex(62)] // BK - Tài khoản quẹt thẻ
        public string? TaiKhoanQuetThe { get; set; }

        [ExcelColumnIndex(63)] // BL - Tiền quẹt thẻ
        public string? TienQuetThe { get; set; }

        [ExcelColumnIndex(64)] // BM - Mã giao dịch quẹt thẻ
        public string? MaGiaoDichQuetThe { get; set; }

        [ExcelColumnIndex(65)] // BN - Giá vốn
        public string? GiaVon { get; set; }

        [ExcelColumnIndex(66)] // BO - Giá vốn đơn hàng
        public string? GiaVonDonHang { get; set; }

        [ExcelColumnIndex(67)] // BP - Đơn vị tính
        public string? DonViTinh { get; set; }

        [ExcelColumnIndex(68)] // BQ - Ghi chú nội bộ
        public string? GhiChuNoiBo { get; set; }

        [ExcelColumnIndex(69)] // BR - Ghi chú của khách
        public string? GhiChuCuaKhach { get; set; }

        [ExcelColumnIndex(70)] // BS - Trạng thái hóa đơn điện tử
        public string? TrangThaiHoaDonDienTu { get; set; }

        [ExcelColumnIndex(71)] // BT - Số hóa đơn điện tử
        public string? SoHoaDonDienTu { get; set; }

        [ExcelColumnIndex(72)] // BU - Ngày xuất hóa đơn điện tử
        public string? NgayXuatHoaDonDienTu { get; set; }

        [ExcelColumnIndex(73)] // BV - Mã cơ quan thuế
        public string? MaCoQuanThueHoaDonDienTu { get; set; }

        [ExcelColumnIndex(74)] // BW - Trạng thái
        public string? TrangThaiDonHang { get; set; }

        [ExcelColumnIndex(75)] // BX - Lý do hủy
        public string? LyDoHuy { get; set; }

        [ExcelColumnIndex(76)] // BY - Chăm sóc khách hàng
        public string? NhanVienChamSocKhachHang { get; set; }

        [ExcelColumnIndex(77)] // BZ - Ngày xác nhận
        public string? NgayXacNhan { get; set; }

        [ExcelColumnIndex(78)] // CA - Ngày gửi HVC
        public string? NgayGuiHangVanChuyen { get; set; }

        [ExcelColumnIndex(79)] // CB - Ngày bàn giao
        public string? NgayBanGiao { get; set; }

        [ExcelColumnIndex(80)] // CC - Ngày thành công
        public string? NgayThanhCong { get; set; }

        [ExcelColumnIndex(81)] // CD - Giờ thành công
        public string? GioThanhCong { get; set; }

        [ExcelColumnIndex(82)] // CE - Nguồn đơn hàng
        public string? NguonDonHang { get; set; }

        [ExcelColumnIndex(83)] // CF - Ghi chú HVC
        public string? GhiChuHangVanChuyen { get; set; }

        [ExcelColumnIndex(84)] // CG - Cân nặng HVC
        public string? CanNangHangVanChuyen { get; set; }

        [ExcelColumnIndex(85)] // CH - Mã giảm giá
        public string? MaGiamGia { get; set; }

        [ExcelColumnIndex(86)] // CI - ID đơn trả
        public string? IdDonTra { get; set; }

        [ExcelColumnIndex(87)] // CJ - ID đơn gốc
        public string? IdDonGoc { get; set; }

        [ExcelColumnIndex(88)] // CK - STT theo ngày
        public string? SoThuTuTheoNgay { get; set; }

        [ExcelColumnIndex(89)] // CL - Biên bản bàn giao
        public string? BienBanBanGiao { get; set; }

        [ExcelColumnIndex(90)] // CM - Mã affiliate
        public string? MaAffiliate { get; set; }

        [ExcelColumnIndex(91)] // CN - Nhóm khách hàng
        public string? NhomKhachHang { get; set; }

        [ExcelColumnIndex(92)] // CO - VAT
        public string? TienVat { get; set; }

        [ExcelColumnIndex(93)] // CP - % VAT
        public string? TyLeVat { get; set; }

        [ExcelColumnIndex(94)] // CQ - Tiền tiêu điểm
        public string? TienTieuDiem { get; set; }

        [ExcelColumnIndex(95)] // CR - Nhãn
        public string? Nhan { get; set; }

        [ExcelColumnIndex(96)] // CS - Ngày hẹn thanh toán
        public string? NgayHenThanhToan { get; set; }

        [ExcelColumnIndex(97)] // CT - Trạng thái đối soát
        public string? TrangThaiDoiSoat { get; set; }

        [ExcelColumnIndex(98)] // CU - Ngày đối soát
        public string? NgayDoiSoat { get; set; }

        [ExcelColumnIndex(99)] // CV - Thanh toán cho doanh nghiệp
        public string? TrangThaiThanhToanDoanhNghiep { get; set; }

        [ExcelColumnIndex(100)] // CW - Ngày thanh toán cho doanh nghiệp
        public string? NgayThanhToanDoanhNghiep { get; set; }

        [ExcelColumnIndex(101)] // CX - Nhân viên bán hàng
        public string? NhanVienBanHang { get; set; }

        [ExcelColumnIndex(102)] // CY - Lợi nhuận
        public string? LoiNhuan { get; set; }

        [ExcelColumnIndex(103)] // CZ - Phần trăm lợi nhuận
        public string? TyLeLoiNhuan { get; set; }

        [ExcelColumnIndex(104)] // DA - Facebook PSID
        public string? FacebookPsid { get; set; }

        [ExcelColumnIndex(105)] // DB - PageUserId (Facebook PSID)
        public string? FacebookPageUserId { get; set; }

        [ExcelColumnIndex(106)] // DC - Phone (Facebook PSID)
        public string? SoDienThoaiFacebook { get; set; }

        [ExcelColumnIndex(107)] // DD - Value (Facebook PSID)
        public string? GiaTriFacebook { get; set; }

        [ExcelColumnIndex(108)] // DE - Số lượng đơn vị nhỏ nhất
        public string? SoLuongDonViNhoNhat { get; set; }

        [ExcelColumnIndex(109)] // DF - Nhân viên đóng gói
        public string? NhanVienDongGoi { get; set; }

        [ExcelColumnIndex(110)] // DG - ID pháp nhân
        public string? IdPhapNhan { get; set; }

        [ExcelColumnIndex(111)] // DH - Mã số thuế
        public string? MaSoThue { get; set; }

        [ExcelColumnIndex(112)] // DI - Email người liên hệ
        public string? EmailNguoiLienHe { get; set; }

        [ExcelColumnIndex(113)] // DJ - Email người liên hệ 2
        public string? EmailNguoiLienHeThuHai { get; set; }

        [ExcelColumnIndex(114)] // DK - Số điện thoại người liên hệ
        public string? SoDienThoaiNguoiLienHe { get; set; }

        [ExcelColumnIndex(115)] // DL - Tên người liên hệ
        public string? TenNguoiLienHe { get; set; }

        [ExcelColumnIndex(116)] // DM - Mã tham chiếu
        public string? MaThamChieu { get; set; }

        [ExcelColumnIndex(117)] // DN - Địa chỉ khách hàng hiển thị ở trên hóa đơn điện tử
        public string? DiaChiKhachHangTrenHoaDonDienTu { get; set; }

        [ExcelColumnIndex(118)] // DO - Tên khách hàng hiển thị ở trên hóa đơn điện tử
        public string? TenKhachHangTrenHoaDonDienTu { get; set; }

        [ExcelColumnIndex(119)] // DP - Loại
        public string? LoaiKhachHangHoaDon { get; set; }

        [ExcelColumnIndex(120)] // DQ - Có lấy hóa đơn
        public string? CoLayHoaDon { get; set; }

        [ExcelColumnIndex(121)] // DR - Mã cơ quan thuế
        public string? MaCoQuanThueHoaDon { get; set; }

        [ExcelColumnIndex(122)] // DS - Số hóa đơn
        public string? SoHoaDon { get; set; }

        [ExcelColumnIndex(123)] // DT - Ngày phát hành
        public string? NgayPhatHanhHoaDon { get; set; }
    }
}