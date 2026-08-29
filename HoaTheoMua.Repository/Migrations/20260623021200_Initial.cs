using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace HoaTheoMua.Repository.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AuditLogs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Action = table.Column<int>(type: "integer", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    OldValue = table.Column<string>(type: "text", nullable: false),
                    NewValue = table.Column<string>(type: "text", nullable: false),
                    EntityType = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    EntityId = table.Column<Guid>(type: "uuid", maxLength: 255, nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuditLogs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CategoryNewsList",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    ParentCategoryNewsId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CategoryNewsList", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CategoryNewsList_CategoryNewsList_ParentCategoryNewsId",
                        column: x => x.ParentCategoryNewsId,
                        principalTable: "CategoryNewsList",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CompanyAddresses",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AddressLine = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Ward = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    City = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Country = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    MapId = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    Latitude = table.Column<decimal>(type: "numeric", nullable: true),
                    Longitude = table.Column<decimal>(type: "numeric", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CompanyAddresses", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Configs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Key = table.Column<string>(type: "text", nullable: false),
                    Value = table.Column<string>(type: "jsonb", nullable: false),
                    IsPublic = table.Column<bool>(type: "boolean", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Configs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "JobDescriptions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    title = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    location = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    avatarUrl = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: false),
                    coverImageUrl = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: false),
                    minExperienceYears = table.Column<float>(type: "real", nullable: true),
                    maxExperienceYears = table.Column<float>(type: "real", nullable: true),
                    level = table.Column<int>(type: "integer", nullable: false),
                    employmentType = table.Column<int>(type: "integer", nullable: false),
                    salaryMin = table.Column<float>(type: "real", nullable: true),
                    salaryMax = table.Column<float>(type: "real", nullable: true),
                    salaryNegotiable = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    description = table.Column<string>(type: "text", nullable: false),
                    status = table.Column<int>(type: "integer", nullable: false),
                    openings = table.Column<int>(type: "integer", nullable: false, defaultValue: 1),
                    slug = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JobDescriptions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "NewsList",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    ViewCount = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    CoverImage = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Status = table.Column<int>(type: "integer", maxLength: 50, nullable: false, defaultValue: 0),
                    slug = table.Column<string>(type: "text", nullable: true),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    AuthorName = table.Column<string>(type: "text", nullable: true),
                    AuthorEmail = table.Column<string>(type: "text", nullable: true),
                    AuthorAvatar = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NewsList", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Orders",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Latitude = table.Column<decimal>(type: "numeric", nullable: false),
                    Longitude = table.Column<decimal>(type: "numeric", nullable: false),
                    Ward = table.Column<int>(type: "integer", nullable: false),
                    City = table.Column<int>(type: "integer", nullable: false),
                    Address = table.Column<string>(type: "text", nullable: false),
                    ShortAddress = table.Column<string>(type: "text", nullable: false),
                    FullName = table.Column<string>(type: "text", nullable: false),
                    Phone = table.Column<string>(type: "text", nullable: false),
                    Remarks = table.Column<string>(type: "text", nullable: false),
                    Status = table.Column<string>(type: "text", nullable: false),
                    TrackNumber = table.Column<string>(type: "text", nullable: false),
                    NhanhOrderId = table.Column<int>(type: "integer", nullable: true),
                    NhanhTrackingUrl = table.Column<string>(type: "text", nullable: true),
                    AhahaMoveOrderId = table.Column<string>(type: "text", nullable: true),
                    AhahaMoveTrackingUrl = table.Column<string>(type: "text", nullable: true),
                    ShippingFee = table.Column<decimal>(type: "numeric", nullable: false),
                    ProductFree = table.Column<decimal>(type: "numeric", nullable: false),
                    TotalFee = table.Column<decimal>(type: "numeric", nullable: false),
                    OrderTime = table.Column<long>(type: "bigint", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Orders", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RecruitmentRequest",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    title = table.Column<string>(type: "text", nullable: false),
                    coverLetter = table.Column<string>(type: "text", nullable: false),
                    attachments = table.Column<List<string>>(type: "jsonb", nullable: false),
                    isRead = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    AuthorName = table.Column<string>(type: "text", nullable: true),
                    AuthorEmail = table.Column<string>(type: "text", nullable: true),
                    AuthorAvatar = table.Column<string>(type: "text", nullable: true),
                    JobDescriptionId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RecruitmentRequest", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RecruitmentRequest_JobDescriptions_JobDescriptionId",
                        column: x => x.JobDescriptionId,
                        principalTable: "JobDescriptions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CategoryNewsDetails",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CategoryNewsId = table.Column<Guid>(type: "uuid", nullable: false),
                    NewsId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CategoryNewsDetails", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CategoryNewsDetails_CategoryNewsList_CategoryNewsId",
                        column: x => x.CategoryNewsId,
                        principalTable: "CategoryNewsList",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CategoryNewsDetails_NewsList_NewsId",
                        column: x => x.NewsId,
                        principalTable: "NewsList",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "OrderDetails",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    ItemId = table.Column<int>(type: "integer", nullable: false),
                    Number = table.Column<int>(type: "integer", nullable: false),
                    Price = table.Column<decimal>(type: "numeric", nullable: false),
                    Height = table.Column<double>(type: "double precision", nullable: false),
                    Weight = table.Column<double>(type: "double precision", nullable: false),
                    Length = table.Column<double>(type: "double precision", nullable: false),
                    Width = table.Column<double>(type: "double precision", nullable: false),
                    OrderId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderDetails", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OrderDetails_Orders_OrderId",
                        column: x => x.OrderId,
                        principalTable: "Orders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Transactions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SePayId = table.Column<long>(type: "bigint", nullable: false),
                    Gateway = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    TransactionDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    AccountNumber = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    SubAccount = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Code = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Content = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    TransferType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    TransferAmount = table.Column<decimal>(type: "numeric", nullable: false),
                    Accumulated = table.Column<decimal>(type: "numeric", nullable: false),
                    ReferenceCode = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    OrderId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Transactions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Transactions_Orders_OrderId",
                        column: x => x.OrderId,
                        principalTable: "Orders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "AuditLogs",
                columns: new[] { "Id", "Action", "CreatedAt", "Description", "EntityId", "EntityType", "IsDeleted", "NewValue", "OldValue", "UpdatedAt", "UserId" },
                values: new object[,]
                {
                    { new Guid("d1111111-1111-1111-1111-111111111111"), 2, new DateTimeOffset(new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Created a new user", new Guid("33333333-3333-3333-3333-333333333333"), "User", false, "User Data", "", null, new Guid("11111111-1111-1111-1111-111111111111") },
                    { new Guid("d1111111-1111-1111-1111-111111111112"), 1, new DateTimeOffset(new DateTime(2024, 1, 2, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Updated news title", new Guid("77777777-7777-7777-7777-777777777777"), "News", false, "New Title", "Old Title", null, new Guid("22222222-2222-2222-2222-222222222222") },
                    { new Guid("d1111111-1111-1111-1111-111111111113"), 0, new DateTimeOffset(new DateTime(2024, 1, 3, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Deleted old category", new Guid("44444444-4444-4444-4444-444444444444"), "CategoryNews", false, "", "Category Data", null, new Guid("11111111-1111-1111-1111-111111111111") },
                    { new Guid("d1111111-1111-1111-1111-111111111114"), 0, new DateTimeOffset(new DateTime(2024, 1, 4, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Created news article", new Guid("88888888-8888-8888-8888-888888888888"), "News", false, "News Data", "", null, new Guid("22222222-2222-2222-2222-222222222222") },
                    { new Guid("d1111111-1111-1111-1111-111111111115"), 0, new DateTimeOffset(new DateTime(2024, 1, 5, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "User logged in", new Guid("33333333-3333-3333-3333-333333333333"), "User", false, "Session Started", "", null, new Guid("33333333-3333-3333-3333-333333333333") },
                    { new Guid("d1111111-1111-1111-1111-111111111116"), 1, new DateTimeOffset(new DateTime(2024, 1, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Updated user profile", new Guid("33333333-3333-3333-3333-333333333333"), "User", false, "New Name", "Old Name", null, new Guid("33333333-3333-3333-3333-333333333333") },
                    { new Guid("d1111111-1111-1111-1111-111111111117"), 1, new DateTimeOffset(new DateTime(2024, 1, 7, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Added category detail", new Guid("c1111111-1111-1111-1111-111111111111"), "CategoryNewsDetail", false, "Mapping Data", "", null, new Guid("11111111-1111-1111-1111-111111111111") },
                    { new Guid("d1111111-1111-1111-1111-111111111118"), 2, new DateTimeOffset(new DateTime(2024, 1, 8, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Added company address", new Guid("e1111111-1111-1111-1111-111111111111"), "CompanyAddress", false, "Address Data", "", null, new Guid("11111111-1111-1111-1111-111111111111") },
                    { new Guid("d1111111-1111-1111-1111-111111111119"), 1, new DateTimeOffset(new DateTime(2024, 1, 9, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Requested password reset", new Guid("33333333-3333-3333-3333-333333333333"), "User", false, "Token Generated", "", null, new Guid("33333333-3333-3333-3333-333333333333") },
                    { new Guid("d1111111-1111-1111-1111-111111111120"), 0, new DateTimeOffset(new DateTime(2024, 1, 10, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "User logged out", new Guid("22222222-2222-2222-2222-222222222222"), "User", false, "Session Ended", "Session Active", null, new Guid("22222222-2222-2222-2222-222222222222") }
                });

            migrationBuilder.InsertData(
                table: "CategoryNewsList",
                columns: new[] { "Id", "CreatedAt", "IsDeleted", "ParentCategoryNewsId", "Title", "UpdatedAt", "UserId" },
                values: new object[,]
                {
                    { new Guid("44444444-4444-4444-4444-444444444441"), new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), false, null, "Technology", null, new Guid("11111111-1111-1111-1111-111111111111") },
                    { new Guid("44444444-4444-4444-4444-444444444444"), new DateTimeOffset(new DateTime(2026, 1, 4, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), false, null, "Health & Wellness", null, new Guid("22222222-2222-2222-2222-222222222222") },
                    { new Guid("44444444-4444-4444-4444-444444444447"), new DateTimeOffset(new DateTime(2026, 1, 7, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), false, null, "Finance", null, new Guid("11111111-1111-1111-1111-111111111111") },
                    { new Guid("44444444-4444-4444-4444-444444444449"), new DateTimeOffset(new DateTime(2026, 1, 9, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), false, null, "Entertainment", null, new Guid("22222222-2222-2222-2222-222222222222") }
                });

            migrationBuilder.InsertData(
                table: "CompanyAddresses",
                columns: new[] { "Id", "AddressLine", "City", "Country", "CreatedAt", "IsDeleted", "Latitude", "Longitude", "MapId", "UpdatedAt", "Ward" },
                values: new object[] { new Guid("e1111111-1111-1111-1111-111111111111"), "123 Main St", "Ho Chi Minh City", "Vietnam", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), false, 10.7769m, 106.7009m, "111-1-1-1-1-1", null, "Ward 1" });

            migrationBuilder.InsertData(
                table: "NewsList",
                columns: new[] { "Id", "AuthorAvatar", "AuthorEmail", "AuthorName", "CoverImage", "CreatedAt", "Description", "IsDeleted", "Status", "Title", "UpdatedAt", "UserId", "ViewCount", "slug" },
                values: new object[,]
                {
                    { new Guid("77777777-7777-7777-7777-777777777771"), null, "admin@example.com", "Admin User", "https://example.com/csharp.jpg", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "New features in C# 12 include primary constructors...", false, 1, "C# 12 Released", null, new Guid("11111111-1111-1111-1111-111111111111"), 100, null },
                    { new Guid("77777777-7777-7777-7777-777777777772"), null, "admin@example.com", "Admin User", "https://example.com/ai.jpg", new DateTimeOffset(new DateTime(2026, 1, 2, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Generative AI is changing the landscape of software engineering.", false, 1, "AI is taking over", null, new Guid("11111111-1111-1111-1111-111111111111"), 550, null },
                    { new Guid("77777777-7777-7777-7777-777777777773"), null, "staff@example.com", "Staff User", "https://example.com/health.jpg", new DateTimeOffset(new DateTime(2026, 1, 3, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Top 10 tips for maintaining a balanced diet.", false, 1, "Healthy Eating Habits", null, new Guid("22222222-2222-2222-2222-222222222222"), 300, null },
                    { new Guid("77777777-7777-7777-7777-777777777774"), null, "staff@example.com", "Staff User", "https://example.com/workout.jpg", new DateTimeOffset(new DateTime(2026, 1, 4, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Quick 15-minute workouts for busy people.", false, 1, "Workout Routines", null, new Guid("22222222-2222-2222-2222-222222222222"), 120, null }
                });

            migrationBuilder.InsertData(
                table: "NewsList",
                columns: new[] { "Id", "AuthorAvatar", "AuthorEmail", "AuthorName", "CoverImage", "CreatedAt", "Description", "IsDeleted", "Title", "UpdatedAt", "UserId", "ViewCount", "slug" },
                values: new object[] { new Guid("77777777-7777-7777-7777-777777777775"), null, "user@example.com", "Normal User", "https://example.com/finance.jpg", new DateTimeOffset(new DateTime(2026, 1, 5, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Tech stocks reach an all-time high.", false, "Stock Market Surge", null, new Guid("33333333-3333-3333-3333-333333333333"), 800, null });

            migrationBuilder.InsertData(
                table: "NewsList",
                columns: new[] { "Id", "AuthorAvatar", "AuthorEmail", "AuthorName", "CoverImage", "CreatedAt", "Description", "IsDeleted", "Status", "Title", "UpdatedAt", "UserId", "ViewCount", "slug" },
                values: new object[,]
                {
                    { new Guid("77777777-7777-7777-7777-777777777776"), null, "user@example.com", "Normal User", "https://example.com/movies.jpg", new DateTimeOffset(new DateTime(2026, 1, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "A review of the best movies released this year.", false, 1, "Top 10 Movies of 2024", null, new Guid("33333333-3333-3333-3333-333333333333"), 1500, null },
                    { new Guid("77777777-7777-7777-7777-777777777777"), null, "admin@example.com", "Admin User", "https://example.com/realestate.jpg", new DateTimeOffset(new DateTime(2026, 1, 7, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "What to expect in the housing market.", false, 1, "Real Estate Trends", null, new Guid("11111111-1111-1111-1111-111111111111"), 420, null }
                });

            migrationBuilder.InsertData(
                table: "NewsList",
                columns: new[] { "Id", "AuthorAvatar", "AuthorEmail", "AuthorName", "CoverImage", "CreatedAt", "Description", "IsDeleted", "Title", "UpdatedAt", "UserId", "ViewCount", "slug" },
                values: new object[] { new Guid("77777777-7777-7777-7777-777777777778"), null, "staff@example.com", "Staff User", "https://example.com/gadgets.jpg", new DateTimeOffset(new DateTime(2026, 1, 8, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "The most anticipated gadgets of the season.", false, "Upcoming Tech Gadgets", null, new Guid("22222222-2222-2222-2222-222222222222"), 750, null });

            migrationBuilder.InsertData(
                table: "NewsList",
                columns: new[] { "Id", "AuthorAvatar", "AuthorEmail", "AuthorName", "CoverImage", "CreatedAt", "Description", "IsDeleted", "Status", "Title", "UpdatedAt", "UserId", "ViewCount", "slug" },
                values: new object[,]
                {
                    { new Guid("77777777-7777-7777-7777-777777777779"), null, "admin@example.com", "Admin User", "https://example.com/mentalhealth.jpg", new DateTimeOffset(new DateTime(2026, 1, 9, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Importance of taking mental health seriously.", false, 1, "Mental Health Awareness", null, new Guid("11111111-1111-1111-1111-111111111111"), 600, null },
                    { new Guid("77777777-7777-7777-7777-777777777780"), null, "staff@example.com", "Staff User", "https://example.com/travel.jpg", new DateTimeOffset(new DateTime(2026, 1, 10, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Must-visit places for your next vacation.", false, 1, "Travel Destinations", null, new Guid("22222222-2222-2222-2222-222222222222"), 950, null }
                });

            migrationBuilder.InsertData(
                table: "CategoryNewsDetails",
                columns: new[] { "Id", "CategoryNewsId", "CreatedAt", "IsDeleted", "NewsId", "UpdatedAt" },
                values: new object[,]
                {
                    { new Guid("c1111111-1111-1111-1111-111111111117"), new Guid("44444444-4444-4444-4444-444444444447"), new DateTimeOffset(new DateTime(2026, 1, 7, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), false, new Guid("77777777-7777-7777-7777-777777777777"), null },
                    { new Guid("c1111111-1111-1111-1111-111111111118"), new Guid("44444444-4444-4444-4444-444444444441"), new DateTimeOffset(new DateTime(2026, 1, 8, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), false, new Guid("77777777-7777-7777-7777-777777777778"), null },
                    { new Guid("c1111111-1111-1111-1111-111111111119"), new Guid("44444444-4444-4444-4444-444444444444"), new DateTimeOffset(new DateTime(2026, 1, 9, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), false, new Guid("77777777-7777-7777-7777-777777777779"), null },
                    { new Guid("c1111111-1111-1111-1111-111111111120"), new Guid("44444444-4444-4444-4444-444444444449"), new DateTimeOffset(new DateTime(2026, 1, 10, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), false, new Guid("77777777-7777-7777-7777-777777777780"), null }
                });

            migrationBuilder.InsertData(
                table: "CategoryNewsList",
                columns: new[] { "Id", "CreatedAt", "IsDeleted", "ParentCategoryNewsId", "Title", "UpdatedAt", "UserId" },
                values: new object[,]
                {
                    { new Guid("44444444-4444-4444-4444-444444444442"), new DateTimeOffset(new DateTime(2026, 1, 2, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), false, new Guid("44444444-4444-4444-4444-444444444441"), "Software Development", null, new Guid("11111111-1111-1111-1111-111111111111") },
                    { new Guid("44444444-4444-4444-4444-444444444443"), new DateTimeOffset(new DateTime(2026, 1, 3, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), false, new Guid("44444444-4444-4444-4444-444444444441"), "AI & Machine Learning", null, new Guid("11111111-1111-1111-1111-111111111111") },
                    { new Guid("44444444-4444-4444-4444-444444444445"), new DateTimeOffset(new DateTime(2026, 1, 5, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), false, new Guid("44444444-4444-4444-4444-444444444444"), "Nutrition", null, new Guid("22222222-2222-2222-2222-222222222222") },
                    { new Guid("44444444-4444-4444-4444-444444444446"), new DateTimeOffset(new DateTime(2026, 1, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), false, new Guid("44444444-4444-4444-4444-444444444444"), "Fitness", null, new Guid("22222222-2222-2222-2222-222222222222") },
                    { new Guid("44444444-4444-4444-4444-444444444448"), new DateTimeOffset(new DateTime(2026, 1, 8, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), false, new Guid("44444444-4444-4444-4444-444444444447"), "Investing", null, new Guid("11111111-1111-1111-1111-111111111111") },
                    { new Guid("44444444-4444-4444-4444-444444444450"), new DateTimeOffset(new DateTime(2026, 1, 10, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), false, new Guid("44444444-4444-4444-4444-444444444449"), "Movies", null, new Guid("22222222-2222-2222-2222-222222222222") }
                });

            migrationBuilder.InsertData(
                table: "CategoryNewsDetails",
                columns: new[] { "Id", "CategoryNewsId", "CreatedAt", "IsDeleted", "NewsId", "UpdatedAt" },
                values: new object[,]
                {
                    { new Guid("c1111111-1111-1111-1111-111111111111"), new Guid("44444444-4444-4444-4444-444444444442"), new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), false, new Guid("77777777-7777-7777-7777-777777777771"), null },
                    { new Guid("c1111111-1111-1111-1111-111111111112"), new Guid("44444444-4444-4444-4444-444444444443"), new DateTimeOffset(new DateTime(2026, 1, 2, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), false, new Guid("77777777-7777-7777-7777-777777777772"), null },
                    { new Guid("c1111111-1111-1111-1111-111111111113"), new Guid("44444444-4444-4444-4444-444444444445"), new DateTimeOffset(new DateTime(2026, 1, 3, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), false, new Guid("77777777-7777-7777-7777-777777777773"), null },
                    { new Guid("c1111111-1111-1111-1111-111111111114"), new Guid("44444444-4444-4444-4444-444444444446"), new DateTimeOffset(new DateTime(2026, 1, 4, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), false, new Guid("77777777-7777-7777-7777-777777777774"), null },
                    { new Guid("c1111111-1111-1111-1111-111111111115"), new Guid("44444444-4444-4444-4444-444444444448"), new DateTimeOffset(new DateTime(2026, 1, 5, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), false, new Guid("77777777-7777-7777-7777-777777777775"), null },
                    { new Guid("c1111111-1111-1111-1111-111111111116"), new Guid("44444444-4444-4444-4444-444444444450"), new DateTimeOffset(new DateTime(2026, 1, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), false, new Guid("77777777-7777-7777-7777-777777777776"), null }
                });

            migrationBuilder.CreateIndex(
                name: "IX_CategoryNewsDetails_CategoryNewsId_NewsId",
                table: "CategoryNewsDetails",
                columns: new[] { "CategoryNewsId", "NewsId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CategoryNewsDetails_NewsId",
                table: "CategoryNewsDetails",
                column: "NewsId");

            migrationBuilder.CreateIndex(
                name: "IX_CategoryNewsList_ParentCategoryNewsId",
                table: "CategoryNewsList",
                column: "ParentCategoryNewsId");

            migrationBuilder.CreateIndex(
                name: "IX_JobDescriptions_slug",
                table: "JobDescriptions",
                column: "slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_OrderDetails_OrderId",
                table: "OrderDetails",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_RecruitmentRequest_JobDescriptionId",
                table: "RecruitmentRequest",
                column: "JobDescriptionId");

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_OrderId",
                table: "Transactions",
                column: "OrderId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AuditLogs");

            migrationBuilder.DropTable(
                name: "CategoryNewsDetails");

            migrationBuilder.DropTable(
                name: "CompanyAddresses");

            migrationBuilder.DropTable(
                name: "Configs");

            migrationBuilder.DropTable(
                name: "OrderDetails");

            migrationBuilder.DropTable(
                name: "RecruitmentRequest");

            migrationBuilder.DropTable(
                name: "Transactions");

            migrationBuilder.DropTable(
                name: "CategoryNewsList");

            migrationBuilder.DropTable(
                name: "NewsList");

            migrationBuilder.DropTable(
                name: "JobDescriptions");

            migrationBuilder.DropTable(
                name: "Orders");
        }
    }
}
