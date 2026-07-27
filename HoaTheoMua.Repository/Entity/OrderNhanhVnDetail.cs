using HoaTheoMua.Repository.Abtraction;

namespace HoaTheoMua.Repository.Entity;

public class OrderNhanhVnDetail : BaseEntity<Guid>, IAuditableEntity
{
    public Guid? OrderNhanhVnId { get; set; }
    public OrderNhanhVn? OrderNhanhVn { get; set; }

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

    
    //Excel: Giá vốn
    public decimal? GiaVon { get; set; }
    
    //Excel: Giá vốn đơn hàng
    public decimal? GiaVonDonHang { get; set; }
    
    //Excel: Đơn vị tính
    public string? DonViTinh { get; set; }

    //Excel: Số lượng đơn vị nhỏ nhất
    public int? SoLuongDonViNhoNhat { get; set; }
    
    //Excel: Nhân viên đóng gói
    public string? NhanVienDongGoi { get; set; }

    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
}