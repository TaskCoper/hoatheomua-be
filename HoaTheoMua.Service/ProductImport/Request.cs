using MiniExcelLibs.Attributes;

namespace HoaTheoMua.Service.ProductImport;

public class Request
{
    /// <summary>
    /// Map 1-1 theo 62 cột (A → BJ) của file Excel export từ Nhanh.vn.
    /// Thứ tự cột đối chiếu tại Product_Excel_Entity_Mapping.md.
    /// </summary>
    public class ExcelRow
    {
        public int SourceRowNumber { get; set; }

        [ExcelColumnIndex(0)] // A - ID sản phẩm
        public string? NhanhProductId { get; set; }

        [ExcelColumnIndex(1)] // B - Id sản phẩm cha
        public string? ParentProductId { get; set; }

        [ExcelColumnIndex(2)] // C - Mã sản phẩm cha
        public string? ParentProductCode { get; set; }

        [ExcelColumnIndex(3)] // D - Tên sản phẩm cha
        public string? ParentProductName { get; set; }

        [ExcelColumnIndex(4)] // E - Mã vạch
        public string? Barcode { get; set; }

        [ExcelColumnIndex(5)] // F - Loại sản phẩm
        public string? ProductType { get; set; }

        [ExcelColumnIndex(6)] // G - Mã sản phẩm
        public string? ProductCode { get; set; }

        [ExcelColumnIndex(7)] // H - Tên sản phẩm
        public string? ProductName { get; set; }

        [ExcelColumnIndex(8)] // I - Tên khác
        public string? OtherName { get; set; }

        [ExcelColumnIndex(9)] // J - Đơn vị tính
        public string? Unit { get; set; }

        [ExcelColumnIndex(10)] // K - Link ảnh sản phẩm
        public string? CoverImage { get; set; }

        [ExcelColumnIndex(11)] // L - Số tháng bảo hành
        public string? WarrantyMonthsRaw { get; set; }

        [ExcelColumnIndex(12)] // M - Cân nặng cả vỏ hộp
        public string? WeightRaw { get; set; }

        [ExcelColumnIndex(13)] // N - Giá nhập
        public string? ImportPriceRaw { get; set; }

        [ExcelColumnIndex(14)] // O - VAT nhập
        public string? ImportVatRaw { get; set; }

        [ExcelColumnIndex(15)] // P - Giá nhập + VAT
        public string? ImportPriceWithVatRaw { get; set; }

        [ExcelColumnIndex(16)] // Q - Vat Mode giá nhập
        public string? ImportVatMode { get; set; }

        [ExcelColumnIndex(17)] // R - Giá bán
        public string? PriceRaw { get; set; }

        [ExcelColumnIndex(18)] // S - Giá bán + VAT
        public string? PriceWithVatRaw { get; set; }

        [ExcelColumnIndex(19)] // T - Giá bán chi nhánh
        public string? BranchPriceRaw { get; set; }

        [ExcelColumnIndex(20)] // U - Giá bán chi nhánh có VAT
        public string? BranchPriceWithVatRaw { get; set; }

        [ExcelColumnIndex(21)] // V - Vat mode giá bán
        public string? SaleVatMode { get; set; }

        [ExcelColumnIndex(22)] // W - Giá cũ
        public string? OldPriceRaw { get; set; }

        [ExcelColumnIndex(23)] // X - VAT
        public string? VatRaw { get; set; }

        [ExcelColumnIndex(24)] // Y - % Lợi nhuận
        public string? ProfitPercentageRaw { get; set; }

        [ExcelColumnIndex(25)] // Z - Giá vốn
        public string? CostPriceRaw { get; set; }

        [ExcelColumnIndex(26)] // AA - Giá sỉ
        public string? WholesalePriceRaw { get; set; }

        [ExcelColumnIndex(27)] // AB - Giá sỉ + VAT
        public string? WholesalePriceWithVatRaw { get; set; }

        [ExcelColumnIndex(28)] // AC - Giá sỉ chi nhánh
        public string? WholesaleBranchPriceRaw { get; set; }

        [ExcelColumnIndex(29)] // AD - Giá sỉ chi nhánh có VAT
        public string? WholesaleBranchPriceWithVatRaw { get; set; }

        [ExcelColumnIndex(30)] // AE - Tổng tồn
        public string? TotalStockRaw { get; set; }

        [ExcelColumnIndex(31)] // AF - Đang chuyển kho
        public string? StockTransferringRaw { get; set; }

        [ExcelColumnIndex(32)] // AG - Tồn
        public string? StockRaw { get; set; }

        [ExcelColumnIndex(33)] // AH - Đang Giao hàng
        public string? DeliveringStockRaw { get; set; }

        [ExcelColumnIndex(34)] // AI - Tồn trong kho
        public string? WarehouseStockRaw { get; set; }

        [ExcelColumnIndex(35)] // AJ - Lỗi
        public string? StockErrorRaw { get; set; }

        [ExcelColumnIndex(36)] // AK - Tạm giữ
        public string? HeldRaw { get; set; }

        [ExcelColumnIndex(37)] // AL - Có thể bán
        public string? AvailableForSaleRaw { get; set; }

        [ExcelColumnIndex(38)] // AM - Bảo hành (tồn)
        public string? WarrantyStockRaw { get; set; }

        [ExcelColumnIndex(39)] // AN - Tạm giữ linh kiện
        public string? HeldComponentsRaw { get; set; }

        [ExcelColumnIndex(40)] // AO - Chờ nhập hàng
        public string? PendingImportStockRaw { get; set; }

        [ExcelColumnIndex(41)] // AP - Đặt trước
        public string? PreOrderRaw { get; set; }

        [ExcelColumnIndex(42)] // AQ - Trạng thái
        public string? Status { get; set; }

        [ExcelColumnIndex(43)] // AR - Mã danh mục
        public string? CategoryCode { get; set; }

        [ExcelColumnIndex(44)] // AS - Danh mục
        public string? CategoryName { get; set; }

        [ExcelColumnIndex(45)] // AT - Mã danh mục nội bộ
        public string? InternalCategoryCode { get; set; }

        [ExcelColumnIndex(46)] // AU - Thương hiệu
        public string? Brand { get; set; }

        [ExcelColumnIndex(47)] // AV - Nhà cung cấp
        public string? Supplier { get; set; }

        [ExcelColumnIndex(48)] // AW - Địa chỉ bảo hành
        public string? WarrantyAddress { get; set; }

        [ExcelColumnIndex(49)] // AX - Link trên website
        public string? Slug { get; set; }

        [ExcelColumnIndex(50)] // AY - Chiều dài
        public string? LengthRaw { get; set; }

        [ExcelColumnIndex(51)] // AZ - Chiều rộng
        public string? WidthRaw { get; set; }

        [ExcelColumnIndex(52)] // BA - Chiều cao
        public string? HeightRaw { get; set; }

        [ExcelColumnIndex(53)] // BB - Thứ tự trên Website
        public string? WebOrderRaw { get; set; }

        [ExcelColumnIndex(54)] // BC - Ngày tạo
        public string? NhanhCreatedAtRaw { get; set; }

        [ExcelColumnIndex(55)] // BD - Nhãn sản phẩm
        public string? ProductLabel { get; set; }

        [ExcelColumnIndex(56)] // BE - Giá bán chi nhánh (trùng)
        public string? BranchPriceDuplicateRaw { get; set; }

        [ExcelColumnIndex(57)] // BF - Giá sỉ chi nhánh (trùng)
        public string? WholesaleBranchPriceDuplicateRaw { get; set; }

        [ExcelColumnIndex(58)] // BG - Giá Tiền
        public string? PriceLabel { get; set; }

        [ExcelColumnIndex(59)] // BH - Kích thước
        public string? Size { get; set; }

        [ExcelColumnIndex(60)] // BI - Màu sắc
        public string? Color { get; set; }

        [ExcelColumnIndex(61)] // BJ - Xuất xứ
        public string? Origin { get; set; }
    }
}
