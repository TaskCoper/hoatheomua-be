using System;
using MiniExcelLibs.Attributes;

namespace HoaTheoMua.Service.ProductImport;

public class Request
{
    public class ExcelRow
    {
        [ExcelColumnIndex(0)]
        public string? NhanhProductId { get; set; }

        [ExcelColumnIndex(1)]
        public string? ParentProductId { get; set; }

        [ExcelColumnIndex(2)]
        public string? Barcode { get; set; }

        [ExcelColumnIndex(3)]
        public string? ProductType { get; set; }

        [ExcelColumnIndex(4)]
        public string? ProductCode { get; set; }

        [ExcelColumnIndex(5)]
        public string? ProductName { get; set; }

        [ExcelColumnIndex(6)]
        public string? Unit { get; set; }

        [ExcelColumnIndex(7)]
        public string? CoverImage { get; set; }

        [ExcelColumnIndex(8)]
        public string? WarrantyMonthsRaw { get; set; }

        [ExcelColumnIndex(9)]
        public string? WeightRaw { get; set; }

        [ExcelColumnIndex(10)]
        public string? ImportPriceRaw { get; set; }

        // Col L (index 11) is VAT nhập (intentionally skipped/ignored)

        [ExcelColumnIndex(12)]
        public string? ImportPriceWithVatRaw { get; set; }

        [ExcelColumnIndex(13)]
        public string? ImportVatMode { get; set; }

        [ExcelColumnIndex(14)]
        public string? PriceRaw { get; set; }

        [ExcelColumnIndex(15)]
        public string? PriceWithVatRaw { get; set; }

        // Col Q, R (indexes 16, 17) skipped (Giá bán chi nhánh, Giá bán chi nhánh có VAT)

        [ExcelColumnIndex(18)]
        public string? SaleVatMode { get; set; }

        // Col T, U (indexes 19, 20) skipped (Giá cũ, VAT)

        [ExcelColumnIndex(21)]
        public string? ProfitPercentageRaw { get; set; }

        [ExcelColumnIndex(22)]
        public string? CostPriceRaw { get; set; }

        [ExcelColumnIndex(23)]
        public string? WholesalePriceRaw { get; set; }

        [ExcelColumnIndex(24)]
        public string? WholesalePriceWithVatRaw { get; set; }

        // Col Z, AA (indexes 25, 26) skipped (Giá sỉ chi nhánh, Giá sỉ chi nhánh có VAT)

        [ExcelColumnIndex(27)]
        public string? TotalStockRaw { get; set; }

        // Col AC (index 28) skipped (Đang chuyển kho)

        [ExcelColumnIndex(29)]
        public string? StockRaw { get; set; }

        [ExcelColumnIndex(30)]
        public string? DeliveringStockRaw { get; set; }

        [ExcelColumnIndex(31)]
        public string? WarehouseStockRaw { get; set; }

        // Col AG (index 32) skipped (Lỗi)

        [ExcelColumnIndex(33)]
        public string? HeldRaw { get; set; }

        [ExcelColumnIndex(34)]
        public string? AvailableForSaleRaw { get; set; }

        [ExcelColumnIndex(35)]
        public string? PendingImportStockRaw { get; set; }

        [ExcelColumnIndex(36)]
        public string? PreOrderRaw { get; set; }

        [ExcelColumnIndex(37)]
        public string? Status { get; set; }

        [ExcelColumnIndex(38)]
        public string? CategoryCode { get; set; }

        [ExcelColumnIndex(39)]
        public string? CategoryName { get; set; }

        [ExcelColumnIndex(40)]
        public string? Brand { get; set; }

        [ExcelColumnIndex(41)]
        public string? Supplier { get; set; }

        [ExcelColumnIndex(42)]
        public string? Slug { get; set; }

        // Col AR (index 43) skipped (Thứ tự trên Website)

        [ExcelColumnIndex(44)]
        public string? NhanhCreatedAtRaw { get; set; }

        // Col AT, AU (indexes 45, 46) skipped (Duplicate branch pricing headers)

        [ExcelColumnIndex(47)]
        public string? PriceLabel { get; set; }

        [ExcelColumnIndex(48)]
        public string? Size { get; set; }

        [ExcelColumnIndex(49)]
        public string? Color { get; set; }
    }
}
