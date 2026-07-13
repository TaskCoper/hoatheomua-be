using MiniExcelLibs.Attributes;

namespace HoaTheoMua.Service.ProductImport;

public class Request
{
    public class ExcelRow
    {
        public int SourceRowNumber { get; set; }

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

        [ExcelColumnIndex(11)]
        public string? ImportVatRaw { get; set; }

        [ExcelColumnIndex(12)]
        public string? ImportPriceWithVatRaw { get; set; }

        [ExcelColumnIndex(13)]
        public string? ImportVatMode { get; set; }

        [ExcelColumnIndex(14)]
        public string? PriceRaw { get; set; }

        [ExcelColumnIndex(15)]
        public string? PriceWithVatRaw { get; set; }

        [ExcelColumnIndex(16)]
        public string? BranchPriceRaw { get; set; }

        [ExcelColumnIndex(17)]
        public string? BranchPriceWithVatRaw { get; set; }

        [ExcelColumnIndex(18)]
        public string? SaleVatMode { get; set; }

        [ExcelColumnIndex(19)]
        public string? OldPriceRaw { get; set; }

        [ExcelColumnIndex(20)]
        public string? VatRaw { get; set; }

        [ExcelColumnIndex(21)]
        public string? ProfitPercentageRaw { get; set; }

        [ExcelColumnIndex(22)]
        public string? CostPriceRaw { get; set; }

        [ExcelColumnIndex(23)]
        public string? WholesalePriceRaw { get; set; }

        [ExcelColumnIndex(24)]
        public string? WholesalePriceWithVatRaw { get; set; }

        [ExcelColumnIndex(25)]
        public string? WholesaleBranchPriceRaw { get; set; }

        [ExcelColumnIndex(26)]
        public string? WholesaleBranchPriceWithVatRaw { get; set; }

        [ExcelColumnIndex(27)]
        public string? TotalStockRaw { get; set; }

        [ExcelColumnIndex(28)]
        public string? StockTransferringRaw { get; set; }

        [ExcelColumnIndex(29)]
        public string? StockRaw { get; set; }

        [ExcelColumnIndex(30)]
        public string? DeliveringStockRaw { get; set; }

        [ExcelColumnIndex(31)]
        public string? WarehouseStockRaw { get; set; }

        [ExcelColumnIndex(32)]
        public string? StockErrorRaw { get; set; }

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

        [ExcelColumnIndex(43)]
        public string? WebOrderRaw { get; set; }

        [ExcelColumnIndex(44)]
        public string? NhanhCreatedAtRaw { get; set; }

        [ExcelColumnIndex(45)]
        public string? BranchPriceDuplicateRaw { get; set; }

        [ExcelColumnIndex(46)]
        public string? WholesaleBranchPriceDuplicateRaw { get; set; }

        [ExcelColumnIndex(47)]
        public string? PriceLabel { get; set; }

        [ExcelColumnIndex(48)]
        public string? Size { get; set; }

        [ExcelColumnIndex(49)]
        public string? Color { get; set; }
    }
}
