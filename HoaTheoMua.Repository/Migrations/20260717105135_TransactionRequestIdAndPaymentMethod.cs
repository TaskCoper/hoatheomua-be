using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HoaTheoMua.Repository.Migrations
{
    /// <inheritdoc />
    public partial class TransactionRequestIdAndPaymentMethod : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Hand-written as a rename + cast. EF scaffolded this as DropColumn("SePayId") +
            // AddColumn("RequestId"), which would destroy every historical SePay transaction id —
            // and since Program.cs runs MigrateAsync() on startup, that loss would happen silently
            // on the first deploy. The values carry over cleanly: RequestId holds the gateway's id
            // for the payment, which for SePay rows is exactly what SePayId held, just as text
            // (the handler now writes request.Id.ToString()).
            migrationBuilder.Sql("""ALTER TABLE "Transactions" RENAME COLUMN "SePayId" TO "RequestId";""");

            migrationBuilder.Sql("""
                ALTER TABLE "Transactions"
                ALTER COLUMN "RequestId" TYPE character varying(100) USING "RequestId"::text;
                """);

            migrationBuilder.AddColumn<int>(
                name: "paymentMethod",
                table: "Transactions",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            // Every row predating this migration came from SePay, which is always VNPayQR (6).
            // Without the backfill they would all read as 0 — not a member of the PaymentMethod
            // enum at all.
            migrationBuilder.Sql("""UPDATE "Transactions" SET "paymentMethod" = 6;""");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "paymentMethod",
                table: "Transactions");

            // Only reversible while every RequestId is still numeric. Once a BaoKim callback has
            // landed, its alphanumeric request_id ("B2B6A278C9815BD7EQ4AVPVZCI") does not fit in a
            // bigint and this cast fails — the honest outcome, rather than silently discarding it.
            migrationBuilder.Sql("""
                ALTER TABLE "Transactions"
                ALTER COLUMN "RequestId" TYPE bigint USING "RequestId"::bigint;
                """);

            migrationBuilder.Sql("""ALTER TABLE "Transactions" RENAME COLUMN "RequestId" TO "SePayId";""");
        }
    }
}
