using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace HoaTheoMua.Repository.Migrations
{
    /// <inheritdoc />
    public partial class Test1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CompanyAddresses",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AddressLine = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Ward = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    District = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    City = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Country = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
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
                name: "NewsList",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    ViewCount = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    CoverImage = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Status = table.Column<int>(type: "integer", maxLength: 50, nullable: false, defaultValue: 0),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NewsList", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    FirstName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    LastName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    HashPassword = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    PhoneNumber = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    Role = table.Column<int>(type: "integer", nullable: false, defaultValue: 1),
                    Avatar = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    IsVerify = table.Column<bool>(type: "boolean", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

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
                    table.ForeignKey(
                        name: "FK_AuditLogs_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
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
                    table.ForeignKey(
                        name: "FK_CategoryNewsList_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "RefreshTokens",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    RefreshTokenHash = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    IpAddress = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    UserAgent = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    DeviceLabel = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    ExpiredAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    RevokedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RefreshTokens", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RefreshTokens_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ResetPasswords",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Otp = table.Column<string>(type: "text", nullable: false),
                    IsUsed = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    ExpiresAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ResetPasswords", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ResetPasswords_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
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

            migrationBuilder.InsertData(
                table: "CompanyAddresses",
                columns: new[] { "Id", "AddressLine", "City", "Country", "CreatedAt", "District", "IsDeleted", "Latitude", "Longitude", "UpdatedAt", "Ward" },
                values: new object[,]
                {
                    { new Guid("e1111111-1111-1111-1111-111111111111"), "123 Main St", "Ho Chi Minh City", "Vietnam", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "District 1", false, null, null, null, "Ward 1" },
                    { new Guid("e1111111-1111-1111-1111-111111111112"), "456 Elm St", "Ho Chi Minh City", "Vietnam", new DateTimeOffset(new DateTime(2026, 1, 2, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "District 2", false, null, null, null, "Ward 2" },
                    { new Guid("e1111111-1111-1111-1111-111111111113"), "789 Oak St", "Hanoi", "Vietnam", new DateTimeOffset(new DateTime(2026, 1, 3, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "District 3", false, null, null, null, "Ward 3" },
                    { new Guid("e1111111-1111-1111-1111-111111111114"), "321 Pine St", "Danang", "Vietnam", new DateTimeOffset(new DateTime(2026, 1, 4, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "District 4", false, null, null, null, "Ward 4" },
                    { new Guid("e1111111-1111-1111-1111-111111111115"), "654 Maple St", "Hue", "Vietnam", new DateTimeOffset(new DateTime(2026, 1, 5, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "District 5", false, null, null, null, "Ward 5" },
                    { new Guid("e1111111-1111-1111-1111-111111111116"), "987 Cedar St", "Can Tho", "Vietnam", new DateTimeOffset(new DateTime(2026, 1, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "District 6", false, null, null, null, "Ward 6" },
                    { new Guid("e1111111-1111-1111-1111-111111111117"), "159 Birch St", "Hai Phong", "Vietnam", new DateTimeOffset(new DateTime(2026, 1, 7, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "District 7", false, null, null, null, "Ward 7" },
                    { new Guid("e1111111-1111-1111-1111-111111111118"), "753 Spruce St", "Nha Trang", "Vietnam", new DateTimeOffset(new DateTime(2026, 1, 8, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "District 8", false, null, null, null, "Ward 8" },
                    { new Guid("e1111111-1111-1111-1111-111111111119"), "852 Fir St", "Da Lat", "Vietnam", new DateTimeOffset(new DateTime(2026, 1, 9, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "District 9", false, null, null, null, "Ward 9" },
                    { new Guid("e1111111-1111-1111-1111-111111111120"), "951 Ash St", "Vung Tau", "Vietnam", new DateTimeOffset(new DateTime(2026, 1, 10, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "District 10", false, null, null, null, "Ward 10" }
                });

            migrationBuilder.InsertData(
                table: "NewsList",
                columns: new[] { "Id", "CoverImage", "CreatedAt", "Description", "IsDeleted", "Status", "Title", "UpdatedAt", "ViewCount" },
                values: new object[,]
                {
                    { new Guid("77777777-7777-7777-7777-777777777771"), "https://example.com/csharp.jpg", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "New features in C# 12 include primary constructors...", false, 1, "C# 12 Released", null, 100 },
                    { new Guid("77777777-7777-7777-7777-777777777772"), "https://example.com/ai.jpg", new DateTimeOffset(new DateTime(2026, 1, 2, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Generative AI is changing the landscape of software engineering.", false, 1, "AI is taking over", null, 550 },
                    { new Guid("77777777-7777-7777-7777-777777777773"), "https://example.com/health.jpg", new DateTimeOffset(new DateTime(2026, 1, 3, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Top 10 tips for maintaining a balanced diet.", false, 1, "Healthy Eating Habits", null, 300 },
                    { new Guid("77777777-7777-7777-7777-777777777774"), "https://example.com/workout.jpg", new DateTimeOffset(new DateTime(2026, 1, 4, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Quick 15-minute workouts for busy people.", false, 1, "Workout Routines", null, 120 }
                });

            migrationBuilder.InsertData(
                table: "NewsList",
                columns: new[] { "Id", "CoverImage", "CreatedAt", "Description", "IsDeleted", "Title", "UpdatedAt", "ViewCount" },
                values: new object[] { new Guid("77777777-7777-7777-7777-777777777775"), "https://example.com/finance.jpg", new DateTimeOffset(new DateTime(2026, 1, 5, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Tech stocks reach an all-time high.", false, "Stock Market Surge", null, 800 });

            migrationBuilder.InsertData(
                table: "NewsList",
                columns: new[] { "Id", "CoverImage", "CreatedAt", "Description", "IsDeleted", "Status", "Title", "UpdatedAt", "ViewCount" },
                values: new object[,]
                {
                    { new Guid("77777777-7777-7777-7777-777777777776"), "https://example.com/movies.jpg", new DateTimeOffset(new DateTime(2026, 1, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "A review of the best movies released this year.", false, 1, "Top 10 Movies of 2024", null, 1500 },
                    { new Guid("77777777-7777-7777-7777-777777777777"), "https://example.com/realestate.jpg", new DateTimeOffset(new DateTime(2026, 1, 7, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "What to expect in the housing market.", false, 1, "Real Estate Trends", null, 420 }
                });

            migrationBuilder.InsertData(
                table: "NewsList",
                columns: new[] { "Id", "CoverImage", "CreatedAt", "Description", "IsDeleted", "Title", "UpdatedAt", "ViewCount" },
                values: new object[] { new Guid("77777777-7777-7777-7777-777777777778"), "https://example.com/gadgets.jpg", new DateTimeOffset(new DateTime(2026, 1, 8, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "The most anticipated gadgets of the season.", false, "Upcoming Tech Gadgets", null, 750 });

            migrationBuilder.InsertData(
                table: "NewsList",
                columns: new[] { "Id", "CoverImage", "CreatedAt", "Description", "IsDeleted", "Status", "Title", "UpdatedAt", "ViewCount" },
                values: new object[,]
                {
                    { new Guid("77777777-7777-7777-7777-777777777779"), "https://example.com/mentalhealth.jpg", new DateTimeOffset(new DateTime(2026, 1, 9, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Importance of taking mental health seriously.", false, 1, "Mental Health Awareness", null, 600 },
                    { new Guid("77777777-7777-7777-7777-777777777780"), "https://example.com/travel.jpg", new DateTimeOffset(new DateTime(2026, 1, 10, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Must-visit places for your next vacation.", false, 1, "Travel Destinations", null, 950 }
                });

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "Avatar", "CreatedAt", "Email", "FirstName", "HashPassword", "IsVerify", "LastName", "PhoneNumber", "UpdatedAt" },
                values: new object[] { new Guid("11111111-1111-1111-1111-111111111111"), null, new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "admin@example.com", "Admin", "hashed_password_1", true, "User", "0123456780", null });

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "Avatar", "CreatedAt", "Email", "FirstName", "HashPassword", "IsVerify", "LastName", "PhoneNumber", "Role", "UpdatedAt" },
                values: new object[,]
                {
                    { new Guid("22222222-2222-2222-2222-222222222222"), null, new DateTimeOffset(new DateTime(2026, 1, 2, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "staff@example.com", "Staff", "hashed_password_2", true, "User", "0123456781", 2, null },
                    { new Guid("33333333-3333-3333-3333-333333333333"), null, new DateTimeOffset(new DateTime(2026, 1, 3, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "user@example.com", "Normal", "hashed_password_3", true, "User", "0123456782", 1, null },
                    { new Guid("33333333-3333-3333-3333-333333333334"), null, new DateTimeOffset(new DateTime(2026, 1, 4, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "alice@example.com", "Alice", "hashed_password_4", true, "Smith", "0123456783", 1, null },
                    { new Guid("33333333-3333-3333-3333-333333333335"), null, new DateTimeOffset(new DateTime(2026, 1, 5, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "bob@example.com", "Bob", "hashed_password_5", true, "Johnson", "0123456784", 1, null },
                    { new Guid("33333333-3333-3333-3333-333333333336"), null, new DateTimeOffset(new DateTime(2026, 1, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "charlie@example.com", "Charlie", "hashed_password_6", true, "Brown", "0123456785", 1, null },
                    { new Guid("33333333-3333-3333-3333-333333333337"), null, new DateTimeOffset(new DateTime(2026, 1, 7, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "diana@example.com", "Diana", "hashed_password_7", true, "Prince", "0123456786", 1, null },
                    { new Guid("33333333-3333-3333-3333-333333333338"), null, new DateTimeOffset(new DateTime(2026, 1, 8, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "evan@example.com", "Evan", "hashed_password_8", true, "Wright", "0123456787", 1, null },
                    { new Guid("33333333-3333-3333-3333-333333333339"), null, new DateTimeOffset(new DateTime(2026, 1, 9, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "fiona@example.com", "Fiona", "hashed_password_9", true, "Gallagher", "0123456788", 1, null },
                    { new Guid("33333333-3333-3333-3333-333333333340"), null, new DateTimeOffset(new DateTime(2026, 1, 10, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "george@example.com", "George", "hashed_password_10", true, "Lucas", "0123456789", 1, null }
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
                table: "RefreshTokens",
                columns: new[] { "Id", "CreatedAt", "DeviceLabel", "ExpiredAt", "IpAddress", "IsActive", "IsDeleted", "RefreshTokenHash", "RevokedAt", "UpdatedAt", "UserAgent", "UserId" },
                values: new object[,]
                {
                    { new Guid("a1111111-1111-1111-1111-111111111111"), new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Desktop", new DateTimeOffset(new DateTime(2026, 1, 31, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "192.168.1.1", true, false, "hash1", null, null, "Mozilla", new Guid("11111111-1111-1111-1111-111111111111") },
                    { new Guid("a1111111-1111-1111-1111-111111111112"), new DateTimeOffset(new DateTime(2026, 1, 2, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Mobile", new DateTimeOffset(new DateTime(2026, 2, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "192.168.1.2", true, false, "hash2", null, null, "Chrome", new Guid("22222222-2222-2222-2222-222222222222") },
                    { new Guid("a1111111-1111-1111-1111-111111111113"), new DateTimeOffset(new DateTime(2026, 1, 3, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Tablet", new DateTimeOffset(new DateTime(2026, 2, 2, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "192.168.1.3", true, false, "hash3", null, null, "Safari", new Guid("33333333-3333-3333-3333-333333333333") },
                    { new Guid("a1111111-1111-1111-1111-111111111114"), new DateTimeOffset(new DateTime(2026, 1, 4, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Desktop", new DateTimeOffset(new DateTime(2026, 2, 3, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "192.168.1.4", true, false, "hash4", null, null, "Edge", new Guid("33333333-3333-3333-3333-333333333334") },
                    { new Guid("a1111111-1111-1111-1111-111111111115"), new DateTimeOffset(new DateTime(2026, 1, 5, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Mobile", new DateTimeOffset(new DateTime(2026, 2, 4, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "192.168.1.5", true, false, "hash5", null, null, "Firefox", new Guid("33333333-3333-3333-3333-333333333335") },
                    { new Guid("a1111111-1111-1111-1111-111111111116"), new DateTimeOffset(new DateTime(2026, 1, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Desktop", new DateTimeOffset(new DateTime(2026, 2, 5, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "192.168.1.6", true, false, "hash6", null, null, "Opera", new Guid("33333333-3333-3333-3333-333333333336") },
                    { new Guid("a1111111-1111-1111-1111-111111111117"), new DateTimeOffset(new DateTime(2026, 1, 7, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Tablet", new DateTimeOffset(new DateTime(2026, 2, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "192.168.1.7", true, false, "hash7", null, null, "Mozilla", new Guid("33333333-3333-3333-3333-333333333337") },
                    { new Guid("a1111111-1111-1111-1111-111111111118"), new DateTimeOffset(new DateTime(2026, 1, 8, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Mobile", new DateTimeOffset(new DateTime(2026, 2, 7, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "192.168.1.8", true, false, "hash8", null, null, "Chrome", new Guid("33333333-3333-3333-3333-333333333338") },
                    { new Guid("a1111111-1111-1111-1111-111111111119"), new DateTimeOffset(new DateTime(2026, 1, 9, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Desktop", new DateTimeOffset(new DateTime(2026, 2, 8, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "192.168.1.9", true, false, "hash9", null, null, "Safari", new Guid("33333333-3333-3333-3333-333333333339") },
                    { new Guid("a1111111-1111-1111-1111-111111111120"), new DateTimeOffset(new DateTime(2026, 1, 10, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Tablet", new DateTimeOffset(new DateTime(2026, 2, 9, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "192.168.1.10", true, false, "hash10", null, null, "Edge", new Guid("33333333-3333-3333-3333-333333333340") }
                });

            migrationBuilder.InsertData(
                table: "ResetPasswords",
                columns: new[] { "Id", "CreatedAt", "ExpiresAt", "IsDeleted", "IsUsed", "Otp", "UpdatedAt", "UserId" },
                values: new object[] { new Guid("f1111111-1111-1111-1111-111111111111"), new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), new DateTimeOffset(new DateTime(2026, 1, 1, 0, 15, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), false, true, "123456", null, new Guid("33333333-3333-3333-3333-333333333333") });

            migrationBuilder.InsertData(
                table: "ResetPasswords",
                columns: new[] { "Id", "CreatedAt", "ExpiresAt", "IsDeleted", "Otp", "UpdatedAt", "UserId" },
                values: new object[] { new Guid("f1111111-1111-1111-1111-111111111112"), new DateTimeOffset(new DateTime(2026, 1, 2, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), new DateTimeOffset(new DateTime(2026, 1, 2, 0, 15, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), false, "654321", null, new Guid("33333333-3333-3333-3333-333333333334") });

            migrationBuilder.InsertData(
                table: "ResetPasswords",
                columns: new[] { "Id", "CreatedAt", "ExpiresAt", "IsDeleted", "IsUsed", "Otp", "UpdatedAt", "UserId" },
                values: new object[] { new Guid("f1111111-1111-1111-1111-111111111113"), new DateTimeOffset(new DateTime(2026, 1, 3, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), new DateTimeOffset(new DateTime(2026, 1, 3, 0, 15, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), false, true, "111222", null, new Guid("33333333-3333-3333-3333-333333333335") });

            migrationBuilder.InsertData(
                table: "ResetPasswords",
                columns: new[] { "Id", "CreatedAt", "ExpiresAt", "IsDeleted", "Otp", "UpdatedAt", "UserId" },
                values: new object[] { new Guid("f1111111-1111-1111-1111-111111111114"), new DateTimeOffset(new DateTime(2026, 1, 4, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), new DateTimeOffset(new DateTime(2026, 1, 4, 0, 15, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), false, "333444", null, new Guid("33333333-3333-3333-3333-333333333336") });

            migrationBuilder.InsertData(
                table: "ResetPasswords",
                columns: new[] { "Id", "CreatedAt", "ExpiresAt", "IsDeleted", "IsUsed", "Otp", "UpdatedAt", "UserId" },
                values: new object[] { new Guid("f1111111-1111-1111-1111-111111111115"), new DateTimeOffset(new DateTime(2026, 1, 5, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), new DateTimeOffset(new DateTime(2026, 1, 5, 0, 15, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), false, true, "555666", null, new Guid("33333333-3333-3333-3333-333333333337") });

            migrationBuilder.InsertData(
                table: "ResetPasswords",
                columns: new[] { "Id", "CreatedAt", "ExpiresAt", "IsDeleted", "Otp", "UpdatedAt", "UserId" },
                values: new object[] { new Guid("f1111111-1111-1111-1111-111111111116"), new DateTimeOffset(new DateTime(2026, 1, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), new DateTimeOffset(new DateTime(2026, 1, 6, 0, 15, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), false, "777888", null, new Guid("33333333-3333-3333-3333-333333333338") });

            migrationBuilder.InsertData(
                table: "ResetPasswords",
                columns: new[] { "Id", "CreatedAt", "ExpiresAt", "IsDeleted", "IsUsed", "Otp", "UpdatedAt", "UserId" },
                values: new object[] { new Guid("f1111111-1111-1111-1111-111111111117"), new DateTimeOffset(new DateTime(2026, 1, 7, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), new DateTimeOffset(new DateTime(2026, 1, 7, 0, 15, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), false, true, "999000", null, new Guid("33333333-3333-3333-3333-333333333339") });

            migrationBuilder.InsertData(
                table: "ResetPasswords",
                columns: new[] { "Id", "CreatedAt", "ExpiresAt", "IsDeleted", "Otp", "UpdatedAt", "UserId" },
                values: new object[] { new Guid("f1111111-1111-1111-1111-111111111118"), new DateTimeOffset(new DateTime(2026, 1, 8, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), new DateTimeOffset(new DateTime(2026, 1, 8, 0, 15, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), false, "121212", null, new Guid("33333333-3333-3333-3333-333333333340") });

            migrationBuilder.InsertData(
                table: "ResetPasswords",
                columns: new[] { "Id", "CreatedAt", "ExpiresAt", "IsDeleted", "IsUsed", "Otp", "UpdatedAt", "UserId" },
                values: new object[] { new Guid("f1111111-1111-1111-1111-111111111119"), new DateTimeOffset(new DateTime(2026, 1, 9, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), new DateTimeOffset(new DateTime(2026, 1, 9, 0, 15, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), false, true, "343434", null, new Guid("11111111-1111-1111-1111-111111111111") });

            migrationBuilder.InsertData(
                table: "ResetPasswords",
                columns: new[] { "Id", "CreatedAt", "ExpiresAt", "IsDeleted", "Otp", "UpdatedAt", "UserId" },
                values: new object[] { new Guid("f1111111-1111-1111-1111-111111111120"), new DateTimeOffset(new DateTime(2026, 1, 10, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), new DateTimeOffset(new DateTime(2026, 1, 10, 0, 15, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), false, "565656", null, new Guid("22222222-2222-2222-2222-222222222222") });

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
                name: "IX_AuditLogs_UserId",
                table: "AuditLogs",
                column: "UserId");

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
                name: "IX_CategoryNewsList_UserId",
                table: "CategoryNewsList",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_RefreshTokens_RefreshTokenHash",
                table: "RefreshTokens",
                column: "RefreshTokenHash",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RefreshTokens_UserId",
                table: "RefreshTokens",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_ResetPasswords_Otp",
                table: "ResetPasswords",
                column: "Otp",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ResetPasswords_UserId",
                table: "ResetPasswords",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_Email",
                table: "Users",
                column: "Email",
                unique: true);
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
                name: "RefreshTokens");

            migrationBuilder.DropTable(
                name: "ResetPasswords");

            migrationBuilder.DropTable(
                name: "CategoryNewsList");

            migrationBuilder.DropTable(
                name: "NewsList");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
