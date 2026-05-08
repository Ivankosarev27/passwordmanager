using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace PasswordManager.Web.Data.Migrations;

/// <inheritdoc />
public partial class AddTagsAndSettings : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        // ── Tags ─────────────────────────────────────────────────────────
        migrationBuilder.CreateTable(
            name: "Tags",
            columns: table => new
            {
                Id = table.Column<int>(type: "integer", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                Name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                Color = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, defaultValue: "#6366f1"),
                CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                UserId = table.Column<string>(type: "text", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Tags", x => x.Id);
                table.ForeignKey("FK_Tags_AspNetUsers_UserId", x => x.UserId, "AspNetUsers", "Id", onDelete: ReferentialAction.Cascade);
            });

        // ── UserSettings (1:1) ──────────────────────────────────────────
        migrationBuilder.CreateTable(
            name: "UserSettings",
            columns: table => new
            {
                Id = table.Column<int>(type: "integer", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                Theme = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, defaultValue: "light"),
                Language = table.Column<string>(type: "character varying(5)", maxLength: 5, nullable: false, defaultValue: "ru"),
                ShowPasswordsByDefault = table.Column<bool>(type: "boolean", nullable: false),
                AutoLockMinutes = table.Column<int>(type: "integer", nullable: false),
                CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                UserId = table.Column<string>(type: "text", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_UserSettings", x => x.Id);
                table.ForeignKey("FK_UserSettings_AspNetUsers_UserId", x => x.UserId, "AspNetUsers", "Id", onDelete: ReferentialAction.Cascade);
            });

        // ── Join-таблица PasswordEntryTags (N:N) ─────────────────────────
        migrationBuilder.CreateTable(
            name: "PasswordEntryTags",
            columns: table => new
            {
                PasswordEntryId = table.Column<int>(type: "integer", nullable: false),
                TagId = table.Column<int>(type: "integer", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_PasswordEntryTags", x => new { x.PasswordEntryId, x.TagId });
                table.ForeignKey("FK_PasswordEntryTags_PasswordEntries_PasswordEntryId",
                    x => x.PasswordEntryId, "PasswordEntries", "Id", onDelete: ReferentialAction.Cascade);
                table.ForeignKey("FK_PasswordEntryTags_Tags_TagId",
                    x => x.TagId, "Tags", "Id", onDelete: ReferentialAction.Cascade);
            });

        // ── Indexes ──────────────────────────────────────────────────────
        migrationBuilder.CreateIndex("IX_Tags_UserId_Name", "Tags", new[] { "UserId", "Name" }, unique: true);
        migrationBuilder.CreateIndex("IX_UserSettings_UserId", "UserSettings", "UserId", unique: true);
        migrationBuilder.CreateIndex("IX_PasswordEntryTags_TagId", "PasswordEntryTags", "TagId");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "PasswordEntryTags");
        migrationBuilder.DropTable(name: "Tags");
        migrationBuilder.DropTable(name: "UserSettings");
    }
}
