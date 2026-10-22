using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HoaTheoMua.Repository.Migrations
{
    /// <inheritdoc />
    public partial class fixDataProductCategory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 1. Sản phẩm không còn ghim thì không còn vị trí.
            //    Trước đây chúng vẫn bị xếp trước sản phẩm thường trong danh sách storefront.
            migrationBuilder.Sql(@"
                UPDATE ""Products"" SET ""Position"" = NULL
                WHERE NOT ""IsPin"" AND ""Position"" IS NOT NULL;");

            migrationBuilder.Sql(@"
                UPDATE ""CategoryProductDetails"" SET ""Position"" = NULL
                WHERE NOT ""IsPin"" AND ""Position"" IS NOT NULL;");

            // 2. Đánh số lại danh sách ghim toàn cục: 16384, 32768, ...
            //    Giữ nguyên thứ tự đang hiển thị (Position, tên, Id), cùng điều kiện với GetProductPin.
            //    Dòng đã xóa mềm không bị đụng tới.
            migrationBuilder.Sql(@"
                UPDATE ""Products"" p
                SET ""Position"" = (t.rn * 16384)::double precision
                FROM (
                    SELECT ""Id"",
                           ROW_NUMBER() OVER (ORDER BY ""Position"", ""ProductName"", ""Id"") AS rn
                    FROM ""Products""
                    WHERE ""IsPin"" AND NOT ""IsDeleted""
                      AND (""ParentProductId"" IS NULL OR ""ParentProductId"" IN ('', '-1', '-2'))
                ) t
                WHERE p.""Id"" = t.""Id""
                  AND p.""Position"" IS DISTINCT FROM (t.rn * 16384)::double precision;");

            // 3. Đánh số lại danh sách ghim của từng danh mục
            migrationBuilder.Sql(@"
                UPDATE ""CategoryProductDetails"" d
                SET ""Position"" = (t.rn * 16384)::double precision
                FROM (
                    SELECT d2.""Id"",
                           ROW_NUMBER() OVER (PARTITION BY d2.""CategoryProductId""
                                              ORDER BY d2.""Position"", p.""ProductName"", p.""Id"") AS rn
                    FROM ""CategoryProductDetails"" d2
                    JOIN ""Products"" p ON p.""Id"" = d2.""ProductId""
                    JOIN ""CategoryProducts"" c ON c.""Id"" = d2.""CategoryProductId""
                    WHERE d2.""IsPin"" AND NOT d2.""IsDeleted"" AND NOT c.""IsDeleted"" AND NOT p.""IsDeleted""
                      AND (p.""ParentProductId"" IS NULL OR p.""ParentProductId"" IN ('', '-1', '-2'))
                ) t
                WHERE d.""Id"" = t.""Id""
                  AND d.""Position"" IS DISTINCT FROM (t.rn * 16384)::double precision;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Data cleanup is intentionally irreversible because old pin positions are not retained.
        }
    }
}