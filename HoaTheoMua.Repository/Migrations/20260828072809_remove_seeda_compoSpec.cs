using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace HoaTheoMua.Repository.Migrations
{
    /// <inheritdoc />
    public partial class remove_seeda_compoSpec : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                WITH seed_products("Id") AS (
                    VALUES
                        ('0c629e7a-4d13-48b5-a967-f21d80537c94'::uuid),
                        ('46fa8b21-c953-41de-a607-128d7e3c95b4'::uuid),
                        ('5d7c48f1-2e63-4a9b-8f14-cb7291d63805'::uuid),
                        ('6e19d4b7-52c8-469a-b031-f8472d5e16c3'::uuid),
                        ('83c5f920-1a74-4d6e-98b3-2f617c4a5d80'::uuid),
                        ('a31f762c-9b48-4e05-bd67-285cf194a730'::uuid),
                        ('a9682f4c-75d1-438b-90e6-3c517d8a2f49'::uuid),
                        ('b7523a9d-81e6-457c-93f0-6ad4182ce705'::uuid),
                        ('d80416c3-27a9-4fb1-85e6-39c7502ad184'::uuid),
                        ('e84b15d9-6c27-4fa3-91de-73b5082ac649'::uuid),
                        ('f24a7d18-93b6-40ce-a852-7c105e4d9b63'::uuid)
                )
                DELETE FROM "ComboSpecifications" AS cs
                USING seed_products AS sp
                WHERE cs."SubId" = sp."Id"
                   OR cs."MatId" = sp."Id";
                """);

            migrationBuilder.DeleteData(
                table: "ComboSpecifications",
                keyColumn: "Id",
                keyValue: new Guid("1f47b6c2-8d59-4ea3-a716-95c30b2f684d"));

            migrationBuilder.DeleteData(
                table: "ComboSpecifications",
                keyColumn: "Id",
                keyValue: new Guid("28f6c913-b45e-4d72-a801-69e3f25c7b04"));

            migrationBuilder.DeleteData(
                table: "ComboSpecifications",
                keyColumn: "Id",
                keyValue: new Guid("34e9a107-68d2-4c5f-91b7-a8032d6e4f59"));

            migrationBuilder.DeleteData(
                table: "ComboSpecifications",
                keyColumn: "Id",
                keyValue: new Guid("65a7c134-2e89-4bd0-b563-d918f40c7a26"));

            migrationBuilder.DeleteData(
                table: "ComboSpecifications",
                keyColumn: "Id",
                keyValue: new Guid("7a93d6e1-5b42-4f8c-a017-6e29c4b581d3"));

            migrationBuilder.DeleteData(
                table: "ComboSpecifications",
                keyColumn: "Id",
                keyValue: new Guid("8c35e7a2-61f9-42bd-b470-15d8a3e69c24"));

            migrationBuilder.DeleteData(
                table: "ComboSpecifications",
                keyColumn: "Id",
                keyValue: new Guid("9b61e2d8-3f74-49a5-8c20-e7164d93b052"));

            migrationBuilder.DeleteData(
                table: "ComboSpecifications",
                keyColumn: "Id",
                keyValue: new Guid("b03d98e6-64a1-45cf-87e2-319ab750d426"));

            migrationBuilder.DeleteData(
                table: "ComboSpecifications",
                keyColumn: "Id",
                keyValue: new Guid("c2184f75-9a36-47ed-b082-51f63d0a9c24"));

            migrationBuilder.DeleteData(
                table: "ComboSpecifications",
                keyColumn: "Id",
                keyValue: new Guid("d7412b8f-30c5-46ea-9d17-84f6932b0c51"));

            migrationBuilder.DeleteData(
                table: "ComboSpecifications",
                keyColumn: "Id",
                keyValue: new Guid("e5c82a49-17d6-43fb-9a35-70b4d2618e93"));

            migrationBuilder.DeleteData(
                table: "ComboSpecifications",
                keyColumn: "Id",
                keyValue: new Guid("f9124d68-7b30-4ce5-93a1-2d846f5b7c09"));

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: new Guid("0c629e7a-4d13-48b5-a967-f21d80537c94"));

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: new Guid("46fa8b21-c953-41de-a607-128d7e3c95b4"));

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: new Guid("5d7c48f1-2e63-4a9b-8f14-cb7291d63805"));

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: new Guid("6e19d4b7-52c8-469a-b031-f8472d5e16c3"));

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: new Guid("83c5f920-1a74-4d6e-98b3-2f617c4a5d80"));

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: new Guid("a31f762c-9b48-4e05-bd67-285cf194a730"));

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: new Guid("a9682f4c-75d1-438b-90e6-3c517d8a2f49"));

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: new Guid("b7523a9d-81e6-457c-93f0-6ad4182ce705"));

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: new Guid("d80416c3-27a9-4fb1-85e6-39c7502ad184"));

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: new Guid("e84b15d9-6c27-4fa3-91de-73b5082ac649"));

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: new Guid("f24a7d18-93b6-40ce-a852-7c105e4d9b63"));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Products",
                columns: new[] { "Id", "AvailableForSale", "Barcode", "BranchPrice", "BranchPriceDuplicate", "BranchPriceWithVat", "Brand", "CategoryCode", "CategoryName", "Color", "CostPrice", "CoverImage", "CreatedAt", "DeliveringStock", "Factor", "Height", "Held", "HeldComponents", "ImportPrice", "ImportPriceWithVat", "ImportVat", "ImportVatMode", "InternalCategoryCode", "IsActive", "IsDeleted", "Length", "NhanhBrandId", "NhanhCategoryId", "NhanhCreatedAt", "NhanhProductId", "NhanhTypeId", "NhanhUpdatedAt", "OldPrice", "Origin", "OtherImages", "OtherName", "ParentProductCode", "ParentProductId", "ParentProductName", "PendingImportStock", "Position", "PreOrder", "Price", "PriceLabel", "PriceWithVat", "ProductCode", "ProductLabel", "ProductName", "ProductType", "ProfitPercentage", "SaleVatMode", "Size", "Slug", "SourceRowNumber", "Status", "Stock", "StockError", "StockTransferring", "Supplier", "TotalStock", "Unit", "UpdatedAt", "Vat", "WarehouseStock", "WarrantyAddress", "WarrantyMonths", "WarrantyStock", "WebOrder", "Weight", "WholesaleBranchPrice", "WholesaleBranchPriceDuplicate", "WholesaleBranchPriceWithVat", "WholesalePrice", "WholesalePriceWithVat", "Width" },
                values: new object[,]
                {
                    { new Guid("0c629e7a-4d13-48b5-a967-f21d80537c94"), 50m, null, null, null, null, null, null, null, null, null, null, new DateTimeOffset(new DateTime(2026, 8, 24, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null, null, null, null, null, null, null, null, null, null, true, false, null, null, null, null, "SEED-MAT-001", null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, "MAT-ROSE-001", "Thành phần", "Hoa hồng Julibee", "Thành phần", null, null, null, null, 0, null, null, null, null, null, null, "Cành", null, null, null, null, null, null, null, null, null, null, null, null, null, null },
                    { new Guid("46fa8b21-c953-41de-a607-128d7e3c95b4"), 60m, null, null, null, null, null, null, null, null, null, null, new DateTimeOffset(new DateTime(2026, 8, 24, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null, null, null, null, null, null, null, null, null, null, true, false, null, null, null, null, "SEED-MAT-004", null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, "MAT-FOAM-001", "Phụ liệu", "Xốp cắm hoa", "Phụ liệu", null, null, null, null, 0, null, null, null, null, null, null, "Miếng", null, null, null, null, null, null, null, null, null, null, null, null, null, null },
                    { new Guid("5d7c48f1-2e63-4a9b-8f14-cb7291d63805"), 0m, null, null, null, null, null, null, null, null, null, null, new DateTimeOffset(new DateTime(2026, 8, 24, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null, null, null, null, null, null, null, null, null, null, true, false, null, null, null, null, "SEED-COMBO-001", null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, "CB-ROSE-001", "Combo", "Bó hoa hồng Julibee", "Combo", null, null, null, null, 0, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null },
                    { new Guid("6e19d4b7-52c8-469a-b031-f8472d5e16c3"), 48m, null, null, null, null, null, null, null, null, null, null, new DateTimeOffset(new DateTime(2026, 8, 24, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null, null, null, null, null, null, null, null, null, null, true, false, null, null, null, null, "SEED-MAT-005", null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, "MAT-LILY-001", "Thành phần", "Hoa Lily trắng", "Thành phần", null, null, null, null, 0, null, null, null, null, null, null, "Cành", null, null, null, null, null, null, null, null, null, null, null, null, null, null },
                    { new Guid("83c5f920-1a74-4d6e-98b3-2f617c4a5d80"), 40m, null, null, null, null, null, null, null, null, null, null, new DateTimeOffset(new DateTime(2026, 8, 24, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null, null, null, null, null, null, null, null, null, null, true, false, null, null, null, null, "SEED-MAT-006", null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, "MAT-CARNATION-001", "Thành phần", "Hoa lá phụ cẩm chướng", "Thành phần", null, null, null, null, 0, null, null, null, null, null, null, "Cành", null, null, null, null, null, null, null, null, null, null, null, null, null, null },
                    { new Guid("a31f762c-9b48-4e05-bd67-285cf194a730"), 0m, null, null, null, null, null, null, null, null, null, null, new DateTimeOffset(new DateTime(2026, 8, 24, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null, null, null, null, null, null, null, null, null, null, true, false, null, null, null, null, "SEED-COMBO-002", null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, "CB-LILY-001", "Combo", "Giỏ hoa Lily thanh lịch", "Combo", null, null, null, null, 0, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null },
                    { new Guid("a9682f4c-75d1-438b-90e6-3c517d8a2f49"), 64m, null, null, null, null, null, null, null, null, null, null, new DateTimeOffset(new DateTime(2026, 8, 24, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null, null, null, null, null, null, null, null, null, null, true, false, null, null, null, null, "SEED-MAT-008", null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, "MAT-SUNFLOWER-001", "Thành phần", "Hoa hướng dương", "Thành phần", null, null, null, null, 0, null, null, null, null, null, null, "Cành", null, null, null, null, null, null, null, null, null, null, null, null, null, null },
                    { new Guid("b7523a9d-81e6-457c-93f0-6ad4182ce705"), 30m, null, null, null, null, null, null, null, null, null, null, new DateTimeOffset(new DateTime(2026, 8, 24, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null, null, null, null, null, null, null, null, null, null, true, false, null, null, null, null, "SEED-MAT-002", null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, "MAT-PAPER-001", "Phụ liệu", "Giấy gói hoa Hàn Quốc", "Phụ liệu", null, null, null, null, 0, null, null, null, null, null, null, "Tờ", null, null, null, null, null, null, null, null, null, null, null, null, null, null },
                    { new Guid("d80416c3-27a9-4fb1-85e6-39c7502ad184"), 100m, null, null, null, null, null, null, null, null, null, null, new DateTimeOffset(new DateTime(2026, 8, 24, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null, null, null, null, null, null, null, null, null, null, true, false, null, null, null, null, "SEED-MAT-003", null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, "MAT-RIBBON-001", "Phụ liệu", "Ruy băng satin", "Phụ liệu", null, null, null, null, 0, null, null, null, null, null, null, "Mét", null, null, null, null, null, null, null, null, null, null, null, null, null, null },
                    { new Guid("e84b15d9-6c27-4fa3-91de-73b5082ac649"), 0m, null, null, null, null, null, null, null, null, null, null, new DateTimeOffset(new DateTime(2026, 8, 24, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null, null, null, null, null, null, null, null, null, null, true, false, null, null, null, null, "SEED-COMBO-003", null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, "CB-SUNFLOWER-001", "Combo", "Bó hoa hướng dương", "Combo", null, null, null, null, 0, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null },
                    { new Guid("f24a7d18-93b6-40ce-a852-7c105e4d9b63"), 20m, null, null, null, null, null, null, null, null, null, null, new DateTimeOffset(new DateTime(2026, 8, 24, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null, null, null, null, null, null, null, null, null, null, true, false, null, null, null, null, "SEED-MAT-007", null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, "MAT-BASKET-001", "Phụ liệu", "Giỏ mây cắm hoa", "Phụ liệu", null, null, null, null, 0, null, null, null, null, null, null, "Cái", null, null, null, null, null, null, null, null, null, null, null, null, null, null }
                });

            migrationBuilder.InsertData(
                table: "ComboSpecifications",
                columns: new[] { "Id", "CreatedAt", "IsActive", "IsCore", "IsDeleted", "MatId", "Quantity", "SubId", "UpdatedAt" },
                values: new object[,]
                {
                    { new Guid("1f47b6c2-8d59-4ea3-a716-95c30b2f684d"), new DateTimeOffset(new DateTime(2026, 8, 24, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), true, true, false, new Guid("83c5f920-1a74-4d6e-98b3-2f617c4a5d80"), 4m, new Guid("a31f762c-9b48-4e05-bd67-285cf194a730"), null },
                    { new Guid("28f6c913-b45e-4d72-a801-69e3f25c7b04"), new DateTimeOffset(new DateTime(2026, 8, 24, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), true, false, false, new Guid("d80416c3-27a9-4fb1-85e6-39c7502ad184"), 2m, new Guid("e84b15d9-6c27-4fa3-91de-73b5082ac649"), null },
                    { new Guid("34e9a107-68d2-4c5f-91b7-a8032d6e4f59"), new DateTimeOffset(new DateTime(2026, 8, 24, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), true, false, false, new Guid("d80416c3-27a9-4fb1-85e6-39c7502ad184"), 1m, new Guid("5d7c48f1-2e63-4a9b-8f14-cb7291d63805"), null },
                    { new Guid("65a7c134-2e89-4bd0-b563-d918f40c7a26"), new DateTimeOffset(new DateTime(2026, 8, 24, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), true, false, false, new Guid("f24a7d18-93b6-40ce-a852-7c105e4d9b63"), 1m, new Guid("a31f762c-9b48-4e05-bd67-285cf194a730"), null },
                    { new Guid("7a93d6e1-5b42-4f8c-a017-6e29c4b581d3"), new DateTimeOffset(new DateTime(2026, 8, 24, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), true, true, false, new Guid("0c629e7a-4d13-48b5-a967-f21d80537c94"), 10m, new Guid("5d7c48f1-2e63-4a9b-8f14-cb7291d63805"), null },
                    { new Guid("8c35e7a2-61f9-42bd-b470-15d8a3e69c24"), new DateTimeOffset(new DateTime(2026, 8, 24, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), true, false, false, new Guid("46fa8b21-c953-41de-a607-128d7e3c95b4"), 2m, new Guid("e84b15d9-6c27-4fa3-91de-73b5082ac649"), null },
                    { new Guid("9b61e2d8-3f74-49a5-8c20-e7164d93b052"), new DateTimeOffset(new DateTime(2026, 8, 24, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), true, false, false, new Guid("46fa8b21-c953-41de-a607-128d7e3c95b4"), 3m, new Guid("5d7c48f1-2e63-4a9b-8f14-cb7291d63805"), null },
                    { new Guid("b03d98e6-64a1-45cf-87e2-319ab750d426"), new DateTimeOffset(new DateTime(2026, 8, 24, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), true, false, false, new Guid("b7523a9d-81e6-457c-93f0-6ad4182ce705"), 2m, new Guid("a31f762c-9b48-4e05-bd67-285cf194a730"), null },
                    { new Guid("c2184f75-9a36-47ed-b082-51f63d0a9c24"), new DateTimeOffset(new DateTime(2026, 8, 24, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), true, false, false, new Guid("b7523a9d-81e6-457c-93f0-6ad4182ce705"), 2m, new Guid("5d7c48f1-2e63-4a9b-8f14-cb7291d63805"), null },
                    { new Guid("d7412b8f-30c5-46ea-9d17-84f6932b0c51"), new DateTimeOffset(new DateTime(2026, 8, 24, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), true, true, false, new Guid("a9682f4c-75d1-438b-90e6-3c517d8a2f49"), 8m, new Guid("e84b15d9-6c27-4fa3-91de-73b5082ac649"), null },
                    { new Guid("e5c82a49-17d6-43fb-9a35-70b4d2618e93"), new DateTimeOffset(new DateTime(2026, 8, 24, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), true, true, false, new Guid("6e19d4b7-52c8-469a-b031-f8472d5e16c3"), 6m, new Guid("a31f762c-9b48-4e05-bd67-285cf194a730"), null },
                    { new Guid("f9124d68-7b30-4ce5-93a1-2d846f5b7c09"), new DateTimeOffset(new DateTime(2026, 8, 24, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), true, false, false, new Guid("f24a7d18-93b6-40ce-a852-7c105e4d9b63"), 1m, new Guid("e84b15d9-6c27-4fa3-91de-73b5082ac649"), null }
                });
        }
    }
}
