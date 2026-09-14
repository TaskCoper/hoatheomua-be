using System;
using HoaTheoMua.Repository.Enum;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HoaTheoMua.Repository.Migrations
{
    /// <inheritdoc />
    public partial class repairMigrationEnum : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ReceiverName",
                table: "GeneratedCard");

            migrationBuilder.DropColumn(
                name: "SenderName",
                table: "GeneratedCard");

            migrationBuilder.RenameColumn(
                name: "RootID",
                table: "ClientHistory",
                newName: "RootId");

            migrationBuilder.AlterColumn<SystemPromptType>(
                name: "Type",
                table: "SystemPrompt",
                type: "\"SystemPromptType\"",
                nullable: false,
                oldClrType: typeof(SystemPromptType),
                oldType: "SystemPromptType");

            migrationBuilder.AlterColumn<PlatformType>(
                name: "Platform",
                table: "PlatformStandard",
                type: "\"PlatformType\"",
                nullable: false,
                oldClrType: typeof(PlatformType),
                oldType: "PlatformType");

            migrationBuilder.AddColumn<string>(
                name: "SnapshotCard",
                table: "OrderDetails",
                type: "jsonb",
                nullable: true);

            migrationBuilder.AlterColumn<CardFormType>(
                name: "FormType",
                table: "GeneratedCard",
                type: "\"CardFormType\"",
                nullable: false,
                oldClrType: typeof(CardFormType),
                oldType: "CardFormType");

            migrationBuilder.AlterColumn<ClientHistoryType>(
                name: "Type",
                table: "ClientHistory",
                type: "\"ClientHistoryType\"",
                nullable: false,
                oldClrType: typeof(ClientHistoryType),
                oldType: "ClientHistoryType");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<ClientHistoryType>(
                name: "Type",
                table: "ClientHistory",
                type: "ClientHistoryType",
                nullable: false,
                oldClrType: typeof(ClientHistoryType),
                oldType: "\"ClientHistoryType\"");

            migrationBuilder.AlterColumn<CardFormType>(
                name: "FormType",
                table: "GeneratedCard",
                type: "CardFormType",
                nullable: false,
                oldClrType: typeof(CardFormType),
                oldType: "\"CardFormType\"");

            migrationBuilder.DropColumn(
                name: "SnapshotCard",
                table: "OrderDetails");

            migrationBuilder.AlterColumn<PlatformType>(
                name: "Platform",
                table: "PlatformStandard",
                type: "PlatformType",
                nullable: false,
                oldClrType: typeof(PlatformType),
                oldType: "\"PlatformType\"");

            migrationBuilder.AlterColumn<SystemPromptType>(
                name: "Type",
                table: "SystemPrompt",
                type: "SystemPromptType",
                nullable: false,
                oldClrType: typeof(SystemPromptType),
                oldType: "\"SystemPromptType\"");

            migrationBuilder.RenameColumn(
                name: "RootId",
                table: "ClientHistory",
                newName: "RootID");

            migrationBuilder.AddColumn<string>(
                name: "SenderName",
                table: "GeneratedCard",
                type: "character varying(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ReceiverName",
                table: "GeneratedCard",
                type: "character varying(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "");
        }
    }
}
