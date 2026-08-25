using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Cms.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class SchoolWebsiteFactory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "SiteId",
                table: "TenantDomains",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Address",
                table: "Sites",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Email",
                table: "Sites",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FaviconUrl",
                table: "Sites",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FooterText",
                table: "Sites",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "HeaderImageUrl",
                table: "Sites",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "HomeVariant",
                table: "Sites",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "LogoUrl",
                table: "Sites",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MapEmbedUrl",
                table: "Sites",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Phone",
                table: "Sites",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PrimaryColor",
                table: "Sites",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SecondaryColor",
                table: "Sites",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SocialLinksJson",
                table: "Sites",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Tagline",
                table: "Sites",
                type: "nvarchar(250)",
                maxLength: 250,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "JsonData",
                table: "Pages",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MenuOrder",
                table: "Pages",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "PageType",
                table: "Pages",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "ShowInMenu",
                table: "Pages",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "TemplateKey",
                table: "Pages",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ContactSubmissions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SiteId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Phone = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Subject = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    Message = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: false),
                    IsRead = table.Column<bool>(type: "bit", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContactSubmissions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PageTemplates",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TemplateKey = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    PageType = table.Column<int>(type: "int", nullable: false),
                    DefaultSlug = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    DefaultTitle = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    DefaultContent = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DefaultJsonData = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsStarter = table.Column<bool>(type: "bit", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PageTemplates", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TenantDomains_SiteId",
                table: "TenantDomains",
                column: "SiteId");

            migrationBuilder.CreateIndex(
                name: "IX_Pages_SiteId",
                table: "Pages",
                column: "SiteId");

            migrationBuilder.CreateIndex(
                name: "IX_Pages_TenantId_SiteId_PageType",
                table: "Pages",
                columns: new[] { "TenantId", "SiteId", "PageType" });

            migrationBuilder.CreateIndex(
                name: "IX_ContactSubmissions_TenantId_SiteId_CreatedDate",
                table: "ContactSubmissions",
                columns: new[] { "TenantId", "SiteId", "CreatedDate" });

            migrationBuilder.CreateIndex(
                name: "IX_PageTemplates_DisplayOrder",
                table: "PageTemplates",
                column: "DisplayOrder");

            migrationBuilder.CreateIndex(
                name: "IX_PageTemplates_TemplateKey",
                table: "PageTemplates",
                column: "TemplateKey",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Pages_Sites_SiteId",
                table: "Pages",
                column: "SiteId",
                principalTable: "Sites",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_TenantDomains_Sites_SiteId",
                table: "TenantDomains",
                column: "SiteId",
                principalTable: "Sites",
                principalColumn: "Id",
                onDelete: ReferentialAction.NoAction);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Pages_Sites_SiteId",
                table: "Pages");

            migrationBuilder.DropForeignKey(
                name: "FK_TenantDomains_Sites_SiteId",
                table: "TenantDomains");

            migrationBuilder.DropTable(
                name: "ContactSubmissions");

            migrationBuilder.DropTable(
                name: "PageTemplates");

            migrationBuilder.DropIndex(
                name: "IX_TenantDomains_SiteId",
                table: "TenantDomains");

            migrationBuilder.DropIndex(
                name: "IX_Pages_SiteId",
                table: "Pages");

            migrationBuilder.DropIndex(
                name: "IX_Pages_TenantId_SiteId_PageType",
                table: "Pages");

            migrationBuilder.DropColumn(
                name: "SiteId",
                table: "TenantDomains");

            migrationBuilder.DropColumn(
                name: "Address",
                table: "Sites");

            migrationBuilder.DropColumn(
                name: "Email",
                table: "Sites");

            migrationBuilder.DropColumn(
                name: "FaviconUrl",
                table: "Sites");

            migrationBuilder.DropColumn(
                name: "FooterText",
                table: "Sites");

            migrationBuilder.DropColumn(
                name: "HeaderImageUrl",
                table: "Sites");

            migrationBuilder.DropColumn(
                name: "HomeVariant",
                table: "Sites");

            migrationBuilder.DropColumn(
                name: "LogoUrl",
                table: "Sites");

            migrationBuilder.DropColumn(
                name: "MapEmbedUrl",
                table: "Sites");

            migrationBuilder.DropColumn(
                name: "Phone",
                table: "Sites");

            migrationBuilder.DropColumn(
                name: "PrimaryColor",
                table: "Sites");

            migrationBuilder.DropColumn(
                name: "SecondaryColor",
                table: "Sites");

            migrationBuilder.DropColumn(
                name: "SocialLinksJson",
                table: "Sites");

            migrationBuilder.DropColumn(
                name: "Tagline",
                table: "Sites");

            migrationBuilder.DropColumn(
                name: "JsonData",
                table: "Pages");

            migrationBuilder.DropColumn(
                name: "MenuOrder",
                table: "Pages");

            migrationBuilder.DropColumn(
                name: "PageType",
                table: "Pages");

            migrationBuilder.DropColumn(
                name: "ShowInMenu",
                table: "Pages");

            migrationBuilder.DropColumn(
                name: "TemplateKey",
                table: "Pages");
        }
    }
}
